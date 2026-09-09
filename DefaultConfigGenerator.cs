using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Winch.Core;
using Winch.Util;

namespace FishImageReplacer;

public static class DefaultConfigGenerator
{
    public static void Generate(string outputPath)
    {
        var root = new JObject();

        root.Add(
            "$schema",
            "https://raw.githubusercontent.com/DREDGE-Mods/Winch/dev/schemas/config_schema.json"
        );

        var fish = ItemUtil.GetAllFishItemData()
            .Where(WinchExtensions.IsVanilla)
            .ToList();

        var entitlementGroups = fish
            .GroupBy(GetEntitlementGroup)
            .OrderBy(x => GetGroupOrder(x.Key));

        foreach (var entitlementGroup in entitlementGroups)
        {
            var entitlementKey = GetEntitlementKey(entitlementGroup.Key);

            AddSeparator(
                root,
                $"{entitlementKey}Separator",
                GetEntitlementTitleKey(entitlementGroup.Key)
            );

            var families = entitlementGroup
                .GroupBy(GetFamilyRoot)
                .OrderBy(x => x.Key.id);

            foreach (var family in families)
            {
                var familyRoot = family.Key;

                var familyNameKey = PreferLocalizedString(
                    familyRoot.itemInsaneTitleKey,
                    familyRoot.itemNameKey
                );

                // Include the entitlement in the key because some fish have
                // aberrations belonging to a different DLC than their parent.
                AddSeparator(
                    root,
                    $"{entitlementKey}-{familyRoot.id}Separator",
                    GetLocalizationReference(familyNameKey)
                );

                // Normal fish first, then its aberrations.
                foreach (var fishItemData in family
                    .OrderBy(x => x.IsAberration)
                    .ThenBy(x => x.id))
                {
                    AddFishToggle(root, fishItemData);
                }
            }
        }

        File.WriteAllText(
            outputPath,
            root.ToString(Formatting.Indented)
        );

        WinchCore.Log.Info(
            $"Generated default config with {fish.Count} fish at: {outputPath}"
        );
    }

    private static void AddFishToggle(
        JObject root,
        FishItemData fishItemData)
    {
        var nameKey = PreferLocalizedString(
            fishItemData.itemInsaneTitleKey,
            fishItemData.itemNameKey
        );

        var descriptionKey = PreferLocalizedString(
            fishItemData.itemInsaneDescriptionKey,
            fishItemData.itemDescriptionKey
        );

        root[fishItemData.id] = new JObject
        {
            ["type"] = "toggle",
            ["title"] = GetLocalizationReference(nameKey),
            ["tooltip"] = GetLocalizationReference(descriptionKey),
            ["value"] = false
        };
    }

    private static void AddSeparator(
        JObject root,
        string key,
        string title)
    {
        root[key] = new JObject
        {
            ["type"] = "separator",
            ["title"] = title
        };
    }

    private static FishItemData GetFamilyRoot(FishItemData fish)
    {
        if (fish.IsAberration && fish.NonAberrationParent != null)
        {
            return fish.NonAberrationParent;
        }

        return fish;
    }

    private static LocalizedString PreferLocalizedString(
        LocalizedString preferred,
        LocalizedString fallback)
    {
        return preferred != null && !preferred.IsEmpty
            ? preferred
            : fallback;
    }

    private static string GetLocalizationReference(
        LocalizedString localizedString)
    {
        var table = LocalizationSettings.StringDatabase
            .GetTableAsync(localizedString.TableReference)
            .WaitForCompletion();

        var key = localizedString.TableEntryReference
            .ResolveKeyName(table.SharedData);

        return $"{table.TableCollectionName}:{key}";
    }

    private static Entitlement GetEntitlementGroup(FishItemData fish)
    {
        // Exotic aberrations should stay with their original fish,
        // regardless of what entitlement the aberration itself has.
        if (fish.IsAberration &&
            fish.NonAberrationParent != null)
        {
            return GetEntitlementGroup(fish.NonAberrationParent);
        }

        if (fish.entitlementsRequired == null ||
            fish.entitlementsRequired.Count == 0)
        {
            return Entitlement.NONE;
        }

        if (fish.entitlementsRequired.Contains(EntitlementExtra.IRON_RIG))
        {
            return EntitlementExtra.IRON_RIG;
        }

        if (fish.entitlementsRequired.Contains(EntitlementExtra.PALE_REACH))
        {
            return EntitlementExtra.PALE_REACH;
        }

        return fish.entitlementsRequired.FirstOrDefault();
    }

    private static string GetEntitlementKey(Entitlement entitlement)
    {
        if (entitlement == Entitlement.NONE)
        {
            return "base";
        }

        return entitlement
            .GetName()
            .ToLowerInvariant()
            .Replace("_", "");
    }

    private static string GetEntitlementTitleKey(Entitlement entitlement)
    {
        if (entitlement == Entitlement.NONE)
            return "tab.tier-0";

        if (entitlement == EntitlementExtra.PALE_REACH)
            return "label.pale-reach";

        if (entitlement == EntitlementExtra.IRON_RIG)
            return "label.the-iron-rig";

        return entitlement.GetName();
    }

    private static int GetGroupOrder(Entitlement group)
    {
        if (group == Entitlement.NONE)
            return 0;

        if (group == EntitlementExtra.PALE_REACH)
            return 1;

        if (group == EntitlementExtra.IRON_RIG)
            return 2;

        return 100;
    }
}