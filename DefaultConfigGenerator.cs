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

        root.Add("$schema", "https://raw.githubusercontent.com/DREDGE-Mods/Winch/dev/schemas/config_schema.json");

        var fish = ItemUtil.GetAllFishItemData()
            .Where(WinchExtensions.IsVanilla)
            .ToList();

        var groups = fish
            .GroupBy(GetEntitlementGroup)
            .OrderBy(x => GetGroupOrder(x.Key));

        foreach (var group in groups)
        {
            var entitlement = group.Key
                .GetName()
                .ToLowerInvariant();

            if (group.Key == Entitlement.NONE)
            {
                entitlement = "base";
            }

            var key = entitlement
                .Replace("_", "");

            root[$"{key}Separator"] = new JObject
            {
                ["type"] = "separator",
                ["title"] = GetEntitlementTitleKey(group.Key)
            };

            foreach (var fishItemData in group.OrderBy(x => x.id))
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
        }

        File.WriteAllText(
            outputPath,
            root.ToString(Formatting.Indented)
        );

        WinchCore.Log.Info(
            $"Generated default config with {fish.Count} fish at: {outputPath}"
        );
    }

    private static LocalizedString PreferLocalizedString(
        LocalizedString preferred,
        LocalizedString fallback)
    {
        return preferred != null && !preferred.IsEmpty
            ? preferred
            : fallback;
    }

    private static string GetLocalizationReference(LocalizedString localizedString)
    {
        var table = LocalizationSettings.StringDatabase
            .GetTableAsync(localizedString.TableReference)
            .WaitForCompletion();

        var key = localizedString.TableEntryReference.ResolveKeyName(table.SharedData);

        return $"{table.TableCollectionName}:{key}";
    }

    private static Entitlement GetEntitlementGroup(FishItemData fish)
    {
        if (fish.entitlementsRequired == null ||
            fish.entitlementsRequired.Count == 0)
        {
            return Entitlement.NONE;
        }

        return fish.entitlementsRequired.FirstOrDefault();
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