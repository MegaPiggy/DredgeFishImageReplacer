using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Winch.Core;
using Winch.Util;

namespace FishImageReplacer;

public static class DefaultConfigGenerator
{
    public static void Generate(string outputPath)
    {
        var root = new JObject();

        var fish = ItemUtil.GetAllFishItemData()
            .Where(x => x.IsVanilla())
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
                entitlement = "base_game";
            }

            var key = entitlement
                .Replace("_", " ")
                .ToTitleCase()
                .Replace(" ", "");

            var title = $"entitlement.{entitlement}.name";

            root[$"{MakeSafeKey(key)}Separator"] = new JObject
            {
                ["type"] = "separator",
                ["title"] = title
            };

            foreach (var fishItemData in group.OrderBy(x => x.id))
            {
                root[fishItemData.id] = new JObject
                {
                    ["type"] = "toggle",
                    ["title"] = $"item.{fishItemData.id}.name",
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

    private static Entitlement GetEntitlementGroup(FishItemData fish)
    {
        if (fish.entitlementsRequired == null ||
            fish.entitlementsRequired.Count == 0)
        {
            return Entitlement.NONE;
        }

        return fish.entitlementsRequired.FirstOrDefault();
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

    private static string MakeSafeKey(string value)
    {
        return new string(
            value.Where(char.IsLetterOrDigit).ToArray()
        );
    }
}