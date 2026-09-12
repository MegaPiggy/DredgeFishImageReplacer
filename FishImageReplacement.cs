using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Winch.Components;
using Winch.Components.UI;
using Winch.Util;

namespace FishImageReplacer;

public static class FishImageReplacement
{
    private static readonly Dictionary<string, Sprite> OriginalSprites = new();
    private static readonly Dictionary<string, Sprite> ReplacementSprites = new();
    private static readonly Dictionary<string, FishItemData> ReplaceableFish = new();

    private static Sprite ReplacementSprite;

    private static string TextureDirectory =>
        Path.Combine(Main.BasePath, "Assets", "Textures");

    private static bool ForceRegenerate => Main.Debug;

    public static void Initialize()
    {
        CacheReplacementSprites();

        foreach (var fishId in ReplaceableFish.Keys)
            ApplyReplacement(fishId);
    }

    public static void ApplyReplacement(string fishId)
    {
        if (!ReplaceableFish.TryGetValue(fishId, out var fish))
            return;

        fish.sprite = ShouldReplace(fishId)
            ? ReplacementSprites[fishId]
            : OriginalSprites[fishId];
    }

    public static void Clear()
    {
        OriginalSprites.Clear();
        ReplaceableFish.Clear();
    }

    private static void CacheReplacementSprites()
    {
        string guid = Main.GUID.ToLowerInvariant();

        ReplacementSprite ??=
            TextureUtil.GetSprite($"{guid}.default");

        Directory.CreateDirectory(TextureDirectory);

        foreach (var fish in ItemUtil.GetAllFishItemData())
        {
            if (!CouldReplace(fish))
                continue;

            ReplaceableFish[fish.id] = fish;

            if (!OriginalSprites.ContainsKey(fish.id))
                OriginalSprites[fish.id] = fish.sprite;

            if (ReplacementSprites.ContainsKey(fish.id))
                continue;

            string textureName = GetReplacementTextureName(fish);
            string texturePath = Path.Combine(
                TextureDirectory,
                $"{guid}.{textureName}.png"
            );

            if (!File.Exists(texturePath) || ForceRegenerate)
            {
                GenerateReplacementTexture(
                    fish,
                    OriginalSprites[fish.id],
                    ReplacementSprite,
                    texturePath
                );
            }

            ReplacementSprites[fish.id] =
                TextureUtil.GetSprite($"{guid}.{textureName}");
        }
    }

    public static string GetReplacementTexturePath(FishItemData fish)
    {
        return Path.Combine(
            TextureDirectory,
            $"{Main.GUID.ToLowerInvariant()}.{GetReplacementTextureName(fish)}.png"
        );
    }

    private static string GetReplacementTextureName(FishItemData fish)
    {
        string name = fish.id;//$"generated.{fish.id}";

        foreach (char invalid in Path.GetInvalidFileNameChars())
            name = name.Replace(invalid, '_');

        return name;
    }

    private static void GenerateReplacementTexture(
        FishItemData fish,
        Sprite original,
        Sprite replacement,
        string path)
    {
        int width = Mathf.RoundToInt(original.rect.width);
        int height = Mathf.RoundToInt(original.rect.height);

        var texture = ReadSpriteTexture(
            replacement,
            width,
            height
        );

        Color tint = GetAverageSpriteColor(original);

        TintTexture(texture, tint);
        MaskEmptyDimensionCells(fish, texture);
        InsetOpaqueArea(texture, 2);
        AddInnerOutline(texture, 8, GetOutlineColor(tint));

        File.WriteAllBytes(
            path,
            texture.EncodeToPNG()
        );

        GameObject.Destroy(texture);
    }

    private static void MaskEmptyDimensionCells(
        FishItemData fish,
        Texture2D texture)
    {
        int columns = fish.GetWidth();
        int rows = fish.GetHeight();

        if (columns <= 0 || rows <= 0)
            return;

        var occupied = new HashSet<Vector2Int>(fish.dimensions);
        var pixels = texture.GetPixels32();

        int width = texture.width;
        int height = texture.height;

        for (int y = 0; y < height; y++)
        {
            // Texture pixels count from bottom to top, while dimensions
            // parsed from JSON count from top to bottom.
            int gridY = rows - 1 - Mathf.Clamp(
                Mathf.FloorToInt((y + 0.5f) * rows / height),
                0,
                rows - 1
            );

            for (int x = 0; x < width; x++)
            {
                int gridX = Mathf.Clamp(
                    Mathf.FloorToInt((x + 0.5f) * columns / width),
                    0,
                    columns - 1
                );

                if (!occupied.Contains(new Vector2Int(gridX, gridY)))
                {
                    pixels[y * width + x] =
                        new Color32(0, 0, 0, 0);
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private static void TintTexture(
        Texture2D texture,
        Color tint)
    {
        var pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];

            if (pixel.a == 0)
                continue;

            float brightness =
                (pixel.r + pixel.g + pixel.b) /
                (3f * 255f);

            pixels[i] = new Color(
                tint.r * brightness,
                tint.g * brightness,
                tint.b * brightness,
                pixel.a / 255f
            );
        }

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private static Texture2D ReadSpriteTexture(
        Sprite sprite,
        int width,
        int height)
    {
        var renderTexture = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32
        );

        var previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, Color.clear);

        Rect rect = sprite.textureRect;

        var scale = new Vector2(
            rect.width / sprite.texture.width,
            rect.height / sprite.texture.height
        );

        var offset = new Vector2(
            rect.x / sprite.texture.width,
            rect.y / sprite.texture.height
        );

        Graphics.Blit(
            sprite.texture,
            renderTexture,
            scale,
            offset
        );

        var texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        texture.ReadPixels(
            new Rect(0, 0, width, height),
            0,
            0
        );

        texture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture;
    }

    private static Color GetAverageSpriteColor(Sprite sprite)
    {
        int width = Mathf.RoundToInt(sprite.rect.width);
        int height = Mathf.RoundToInt(sprite.rect.height);

        var texture = ReadSpriteTexture(
            sprite,
            width,
            height
        );

        var pixels = texture.GetPixels32();

        long r = 0;
        long g = 0;
        long b = 0;
        long weight = 0;

        foreach (var pixel in pixels)
        {
            if (pixel.a == 0)
                continue;

            int alpha = pixel.a;

            r += pixel.r * alpha;
            g += pixel.g * alpha;
            b += pixel.b * alpha;
            weight += alpha;
        }

        GameObject.Destroy(texture);

        if (weight == 0)
            return Color.white;

        return new Color(
            (float)r / weight / 255f,
            (float)g / weight / 255f,
            (float)b / weight / 255f,
            1f
        );
    }

    private static Color GetOutlineColor(Color tint)
    {
        float luminance =
            tint.r * 0.2126f +
            tint.g * 0.7152f +
            tint.b * 0.0722f;

        Color.RGBToHSV(
            tint,
            out float h,
            out float s,
            out float v
        );

        if (luminance < 0.5f)
        {
            v = Mathf.Lerp(v, 1f, 0.4f);
        }
        else
        {
            v = Mathf.Lerp(v, 0f, 0.4f);
        }

        return Color.HSVToRGB(h, s, v);
    }

    private static void AddInnerOutline(
        Texture2D texture,
        int thickness,
        Color outlineColor)
    {
        var source = texture.GetPixels32();
        var result = (Color32[])source.Clone();

        int width = texture.width;
        int height = texture.height;

        Color32 outline = outlineColor;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                if (source[index].a == 0)
                    continue;

                bool isBorder = false;

                for (
                    int oy = -thickness;
                    oy <= thickness && !isBorder;
                    oy++)
                {
                    for (int ox = -thickness; ox <= thickness; ox++)
                    {
                        if (ox == 0 && oy == 0)
                            continue;

                        int nx = x + ox;
                        int ny = y + oy;

                        if (
                            nx < 0 ||
                            nx >= width ||
                            ny < 0 ||
                            ny >= height)
                        {
                            isBorder = true;
                            break;
                        }

                        if (source[ny * width + nx].a == 0)
                        {
                            isBorder = true;
                            break;
                        }
                    }
                }

                if (isBorder)
                {
                    result[index] = new Color32(
                        outline.r,
                        outline.g,
                        outline.b,
                        source[index].a
                    );
                }
            }
        }

        texture.SetPixels32(result);
        texture.Apply();
    }

    private static void InsetOpaqueArea(
        Texture2D texture,
        int inset)
    {
        if (inset <= 0)
            return;

        var source = texture.GetPixels32();
        var result = (Color32[])source.Clone();

        int width = texture.width;
        int height = texture.height;
        int radiusSq = inset * inset;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                if (source[index].a == 0)
                    continue;

                bool shouldClear = false;

                for (
                    int oy = -inset;
                    oy <= inset && !shouldClear;
                    oy++)
                {
                    for (int ox = -inset; ox <= inset; ox++)
                    {
                        if (ox == 0 && oy == 0)
                            continue;

                        if ((ox * ox) + (oy * oy) > radiusSq)
                            continue;

                        int nx = x + ox;
                        int ny = y + oy;

                        if (
                            nx < 0 ||
                            nx >= width ||
                            ny < 0 ||
                            ny >= height)
                        {
                            shouldClear = true;
                            break;
                        }

                        if (source[ny * width + nx].a == 0)
                        {
                            shouldClear = true;
                            break;
                        }
                    }
                }

                if (shouldClear)
                {
                    result[index] =
                        new Color32(0, 0, 0, 0);
                }
            }
        }

        texture.SetPixels32(result);
        texture.Apply();
    }

    private static bool CouldReplace(FishItemData fish)
    {
        return fish.IsVanilla() &&
               Main.ModConfig.HasProperty(fish.id);
    }

    private static bool ShouldReplace(string fishId)
    {
        return ReplaceableFish.ContainsKey(fishId) &&
               Main.ModConfig.GetProperty<bool>(fishId);
    }

    public static void OpenTextureDirectory()
    {
        Directory.CreateDirectory(TextureDirectory);
        OpenPath(TextureDirectory);
    }

    private static void OpenPath(string path)
    {
        try
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    break;

                case RuntimePlatform.OSXPlayer:
                    Process.Start("open", $"\"{path}\"");
                    break;

                default:
                    Process.Start("xdg-open", $"\"{path}\"");
                    break;
            }
        }
        catch (Exception ex)
        {
            Winch.Core.WinchCore.Log.Error(
                $"Could not open path '{path}': {ex}"
            );
        }
    }

    public static string TextureMapPath =>
        Path.Combine(Main.BasePath, "texture-map.json");

    private static TextureMapEntry[] TextureMapEntries;

    public static TextureMapEntry[] GetTextureMapEntries()
    {
        if (TextureMapEntries != null)
            return TextureMapEntries;

        if (!File.Exists(TextureMapPath))
            return Array.Empty<TextureMapEntry>();

        TextureMapEntries =
            Newtonsoft.Json.JsonConvert.DeserializeObject<TextureMapEntry[]>(
                File.ReadAllText(TextureMapPath)
            ) ?? Array.Empty<TextureMapEntry>();

        return TextureMapEntries;
    }

    public static void GenerateTextureMap()
    {
        var map = ReplaceableFish.Values
            .OrderBy(fish => fish.id)
            .Select(CreateTextureMapEntry)
            .ToArray();

        File.WriteAllText(
            TextureMapPath,
            Newtonsoft.Json.JsonConvert.SerializeObject(
                map,
                Newtonsoft.Json.Formatting.Indented
            )
        );
    }

    [Serializable]
    public class TextureMapEntry
    {
        public string itemId;
        public LocalizedString localizedNameKey;
        public string localizedName => GetCurrentString(localizedNameKey);
        public string englishName => GetEnglishString(localizedNameKey);
        public string aberrationOf;
        public string replacementFile;
    }

    private static TextureMapEntry CreateTextureMapEntry(
        FishItemData fish)
    {
        var sprite = OriginalSprites[fish.id];

        return new TextureMapEntry
        {
            localizedNameKey = PreferLocalizedString(
                fish.itemInsaneTitleKey,
                fish.itemNameKey
            ),
            itemId = fish.id,

            aberrationOf =
                fish.IsAberration &&
                fish.NonAberrationParent != null
                    ? fish.NonAberrationParent.id
                    : null,

            //sourceTexture = sprite?.texture?.name,

            replacementFile =
                Path.GetFileName(
                    GetReplacementTexturePath(fish)
                )
        };
    }

    private static LocalizedString PreferLocalizedString(
        LocalizedString preferred,
        LocalizedString fallback)
    {
        return preferred != null && !preferred.IsEmpty
            ? preferred
            : fallback;
    }

    private static Locale English => 
        LocalizationSettings.AvailableLocales.Locales.Find(
            locale =>
                locale.Identifier.Code == "en"
        );

    private static string GetEnglishString(LocalizedString key) =>
        GetStringFromLocale(key, English);

    private static string GetCurrentString(LocalizedString key) =>
        GetStringFromLocale(key, null);

    private static string GetStringFromLocale(LocalizedString key, Locale locale)
    {
        return LocalizationSettings.StringDatabase
            .GetLocalizedString(
                key.TableReference,
                key.TableEntryReference,
                locale,
                FallbackBehavior.UseProjectSettings
            );
    }

    public static void ReloadTextures()
    {
        string guid = Main.GUID.ToLowerInvariant();

        foreach (var fish in ReplaceableFish.Values)
        {
            string textureName = GetReplacementTextureName(fish);
            string key = $"{guid}.{textureName}";

            TextureUtil.ReloadTexture(key);

            ReplacementSprites[fish.id] =
                TextureUtil.GetSprite(key);

            ApplyReplacement(fish.id);
        }
    }

    public static Sprite GetReplacementSprite(string fishId)
    {
        if (ReplacementSprites.TryGetValue(fishId, out var sprite) &&
            sprite != null)
        {
            return sprite;
        }

        var entry = GetTextureMapEntries()
            .FirstOrDefault(entry => entry.itemId == fishId);

        if (entry == null ||
            string.IsNullOrWhiteSpace(entry.replacementFile))
        {
            return null;
        }

        string key =
            Path.GetFileNameWithoutExtension(entry.replacementFile);

        sprite = TextureUtil.GetSprite(key);

        if (sprite != null)
            ReplacementSprites[fishId] = sprite;

        return sprite;
    }

    public static void OnBuildModConfigMenu(ModsTab tab)
    {
        var utilityButtons = new[]
        {
            tab.AddOptionButtonLocalized(
                "OpenImageMenu",
                "megapiggy.fishimagereplacer.config.openimagemenu",
                "megapiggy.fishimagereplacer.config.openimagemenu.tooltip",
                FishImageGuideScreen.Show
            ),
            tab.AddOptionButtonLocalized(
                "ReloadTextures",
                "megapiggy.fishimagereplacer.config.reloadtextures",
                "megapiggy.fishimagereplacer.config.reloadtextures.tooltip",
                ReloadTextures
            ),
            tab.AddOptionButtonLocalized(
                "OpenTextureFolder",
                "megapiggy.fishimagereplacer.config.openfolder",
                "megapiggy.fishimagereplacer.config.openfolder.tooltip",
                OpenTextureDirectory
            )
        };

        tab.MoveOptionsToStart(
            utilityButtons
                .ToArray()
        );
    }
}