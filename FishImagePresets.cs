using System;
using System.Collections.Generic;
using System.Linq;
using Winch.Components;
using Winch.Components.UI;
using Winch.Core;
using Winch.Core.API;

namespace FishImageReplacer;

public static class FishImagePresets
{
    private static readonly HashSet<string> Sharks = new()
    {
        "blacktip-reef-shark",
        "bronze-whaler",
        "goblin-shark",
        "sleeper-shark",
        "eagle-shark",
        "frilled-shark",
        "ghost-shark",
        "hammerhead-shark"
    };

    private static readonly HashSet<string> Eels = new()
    {
        "eel",
        "conger-eel",
        "cusk-eel",
        "longfin-eel",
        "gulper-eel"
    };

    private static readonly HashSet<string> CrustaceansAndArthropods = new()
    {
        "blue-crab",
        "crab",
        "decorator-crab",
        "fiddler-crab",
        "giant-mud-crab",
        "horseshoe-crab",
        "rock-crab",
        "spider-crab",
        "king-crab",
        "spiny-lobster",
        "squat-lobster",
        "giant-amphipod",
        "scarlet-prawn",

        // Prehistoric arthropod / stem-arthropod creatures
        "trilobite",
        "sea-scorpion",
        "opabinia",
        "kerygmachela"
    };

    private static readonly HashSet<string> DeepSeaCreatures = new()
    {
        "anglerfish",
        "barreleye",
        "fangtooth",
        "stoplight-loosejaw",
        "giant-dragonfish",
        "viperfish",
        "gulper-eel",
        "rattail",
        "lancetfish",
        "tripod-spiderfish",
        "goblin-shark",
        "frilled-shark",

        "snailfish",
        "giant-amphipod",
        "ghost-shark",
        "pale-skate",
        "firefly-squid",
        "scarlet-prawn",
        "sleeper-shark",
        "lizardfish",
        "colossal-squid",
        "abyssal-gar",
        "coelacanth",
        "oarfish",
        "nautilus",
        "squat-lobster",
        "spider-crab"
    };

    private static readonly HashSet<string> PrehistoricCreatures = new()
    {
        "boreaspis",
        "coelacanth",
        "dunkleosteus",
        "eagle-shark",
        "kerygmachela",
        "nautilus",
        "nipponites",
        "opabinia",
        "osteostracan",
        "sea-scorpion",
        "sollasina",
        "trilobite",
        "tullimonstrum",
        "vetulicolia",
        "xiphactinus",

        // Living fossils / ancient lineages
        "horseshoe-crab",
        "frilled-shark",
        "sturgeon",
        "gar"
    };

    private static readonly HashSet<string> Cephalopods = new()
    {
        "squid",
        "colossal-squid",
        "firefly-squid",
        "glowing-octopus",

        "nautilus",
        "nipponites"
    };

    private static readonly HashSet<string> LeggyCreatures = new()
    {
        "crab",
        "blue-crab",
        "decorator-crab",
        "fiddler-crab",
        "giant-mud-crab",
        "rock-crab",
        "horseshoe-crab",
        "king-crab",
        "spider-crab",

        "spiny-lobster",
        "squat-lobster",
        "scarlet-prawn",
        "giant-amphipod",

        // Ancient leggy weirdos
        "sea-scorpion",
        "trilobite",
        "kerygmachela",
        "sollasina"
    };

    private static readonly HashSet<string> TeethAndMouths = new()
    {
        // Normal fish where the teeth are a defining feature
        "anglerfish",
        "fangtooth",
        "stoplight-loosejaw",
        "giant-dragonfish",
        "viperfish",
        "gulper-eel",
        "goblin-shark",
        "frilled-shark",
        "snake-mackerel",
        "barracuda",
        "toothfish",
        "wolffish",
        "goliath-tigerfish",
        "dunkleosteus",
        "abyssal-gar",
        "lancetfish",
        "xiphactinus",
        "lizardfish",
        "stargazer",

        // Aberrations specifically focused on mouths / teeth
        "mackerel-ab-1",            // Grotesque Mackerel
        "cod-ab-2",                 // Fanged Cod
        "black-grouper-ab-1",       // Tusked Grouper
        "cusk-eel-ab-1",            // Infernal Eel
        "bronze-whaler-ab-1",       // Bloodskin Shark
        "blacktip-reef-shark-ab-1", // Cleft-mouth Shark
        "eel-ab-1",                 // Barbed Eel
        "squid-ab-2",               // Snag Squid
        "oceanic-perch-ab-1",       // Gnashing Perch
        "viperfish-ab-1",           // Decrepit Viperfish
        "nipponites-ab-1",          // Wretched Nipper
        "gar-ab-2",                 // Grinning Gar
        "moonfish-ab-2",            // Beaked Moonfish
        "vetulicolia-ab-1",         // Unveiled Vetulicolia
        "tullimonstrum-ab-1",       // Axial Matron
        "ghost-shark-ab-1",         // Rapt Shark
        "goblin-shark-ab-1",        // Grisly Shark
        "wolffish-ab-1",            // Hinged Wolffish
        "arapaima-ab-1",            // Broken Arapaima
        "dunkleosteus-ab-1",        // Excoriated Fiend
        "pale-skate-ab-1",          // Defaced Skate
        "longfin-eel-ab-1",         // Twinned Eels
        "sergeant-fish-ab-1",       // Vortex Interloper
        "glowing-octopus-ab-1",     // Medusa Octopus
        "fiddler-crab-ab-1",        // Malignant Pincer
        "swordfish-ab-1",           // Ivory Impaler
    };

    private static readonly HashSet<string> BorderlineMonster = new()
    {
        "stonefish",
        "trilobite",
        "decorator-crab",
        "fangtooth",
        "giant-amphipod",
        "stoplight-loosejaw",
        "anglerfish",
        "kerygmachela",
        "crown-of-thorns",
        "spiny-lobster",
        "opabinia",
        "xiphactinus",
        "horseshoe-crab",
        "nipponites",
        "vetulicolia",
        "giant-dragonfish",
        "squat-lobster",
        "spider-crab",
        "sea-scorpion",
        "viperfish",
        "stargazer",
        "king-crab",
        "gulper-eel",

        "black-grouper-ab-1", // Tusked Grouper
        "stonefish-ab-1",     // Gelatinous Stonefish
        "stonefish-ab-2"      // Enthralled Stonefish
    };

    public static void OnBuildModConfigMenu(ModsTab tab)
    {
        var presetButtons = new[]
        {
            AddPresetButton(
                tab,
                "PresetNone",
                "none",
                ApplyNonePreset),

            AddPresetButton(
                tab,
                "PresetAll",
                "all",
                ApplyAllPreset),

            AddPresetButton(
                tab,
                "PresetAberrations",
                "aberrations",
                ApplyAberrationsPreset),

            AddPresetButton(
                tab,
                "PresetSharks",
                "sharks",
                ApplySharksPreset),

            AddPresetButton(
                tab,
                "PresetEels",
                "eels",
                ApplyEelsPreset),

            AddPresetButton(
                tab,
                "PresetCrustaceansAndArthropods",
                "crustaceansandarthropods",
                ApplyCrustaceansAndArthropodsPreset),

            AddPresetButton(
                tab,
                "PresetDeepSea",
                "deepsea",
                ApplyDeepSeaPreset),

            AddPresetButton(
                tab,
                "PresetPrehistoric",
                "prehistoric",
                ApplyPrehistoricPreset),

            AddPresetButton(
                tab,
                "PresetCephalopods",
                "cephalopods",
                ApplyCephalopodsPreset),

            AddPresetButton(
                tab,
                "PresetLeggyCreatures",
                "leggycreatures",
                ApplyLeggyCreaturesPreset),

            AddPresetButton(
                tab,
                "PresetTeethAndMouths",
                "teethandmouths",
                ApplyTeethAndMouthsPreset),

            AddPresetButton(
                tab,
                "PresetBorderlineMonster",
                "borderlinemonster",
                ApplyBorderlineMonsterPreset)
        };

        tab.MoveOptionsToStart(presetButtons);

        tab.MoveOptionToStart(tab.AddSeparatorAndLabelInput(Main.GUID, "presetsSeparator", "megapiggy.fishimagereplacer.config.presets"));
    }

    private static BasicButtonWrapper AddPresetButton(
        ModsTab tab,
        string key,
        string localizationKey,
        Action applyPreset)
    {
        var localizationBase =
            $"megapiggy.fishimagereplacer.config.preset.{localizationKey}";

        return tab.AddOptionButtonLocalized(
            key,
            localizationBase,
            localizationBase + ".tooltip",
            () =>
            {
                applyPreset();
                tab.RefreshAllInputs();
            });
    }

    private static void ApplyNonePreset()
    {
        ApplyPreset(_ => false);
    }

    private static void ApplyAllPreset()
    {
        ApplyPreset(_ => true);
    }

    private static void ApplyAberrationsPreset()
    {
        ApplyPreset(key => key.Contains("-ab-"));
    }

    private static void ApplyNormalFishPreset()
    {
        ApplyPreset(key => !key.Contains("-ab-"));
    }

    private static void ToggleSharks() =>
        ToggleFamily(Sharks);

    private static void ToggleEels() =>
        ToggleFamily(Eels);

    private static void ToggleCrustaceansAndArthropods() =>
        ToggleFamily(CrustaceansAndArthropods);

    private static void ToggleDeepSeaCreatures() =>
        ToggleFamily(DeepSeaCreatures);

    private static void TogglePrehistoricCreatures() =>
        ToggleFamily(PrehistoricCreatures);

    private static void ToggleCephalopods() =>
        ToggleFamily(Cephalopods);

    private static void ToggleLeggyCreatures() =>
        ToggleFamily(LeggyCreatures);

    private static void ToggleTeethAndMouths() =>
        ToggleFamily(TeethAndMouths);

    private static void ToggleBorderlineMonster() =>
        ToggleFamily(BorderlineMonster);

    private static void ApplySharksPreset() =>
        ApplyFamilyPreset(Sharks);

    private static void ApplyEelsPreset() =>
        ApplyFamilyPreset(Eels);

    private static void ApplyCrustaceansAndArthropodsPreset() =>
        ApplyFamilyPreset(CrustaceansAndArthropods);

    private static void ApplyDeepSeaPreset() =>
        ApplyFamilyPreset(DeepSeaCreatures);

    private static void ApplyPrehistoricPreset() =>
        ApplyFamilyPreset(PrehistoricCreatures);

    private static void ApplyCephalopodsPreset() =>
        ApplyFamilyPreset(Cephalopods);

    private static void ApplyLeggyCreaturesPreset() =>
        ApplyFamilyPreset(LeggyCreatures);

    private static void ApplyTeethAndMouthsPreset() =>
        ApplyFamilyPreset(TeethAndMouths);

    private static void ApplyBorderlineMonsterPreset() =>
        ApplyPreset(BorderlineMonster);

    private static bool IsInFamily(string key, HashSet<string> ids)
    {
        return ids.Any(id =>
            key == id ||
            key.StartsWith(id + "-ab-", StringComparison.Ordinal));
    }

    private static IEnumerable<string> GetFamilyKeys(HashSet<string> fishIds)
    {
        return Main.ModConfig.GetKeys()
            .Where(key => IsInFamily(key, fishIds));
    }

    private static void ToggleFamily(HashSet<string> fishIds)
    {
        var keys = GetFamilyKeys(fishIds).ToList();

        // If anything is disabled, enable the whole family.
        // Otherwise disable the whole family.
        var enabled = keys.Any(key => !Main.ModConfig.GetProperty<bool>(key));

        foreach (var key in keys)
        {
            Main.ModConfig.SetProperty(key, enabled);
        }
    }

    private static void ApplyFamilyPreset(HashSet<string> fishIds)
    {
        ApplyPreset(key => IsInFamily(key, fishIds));
    }

    private static void ApplyPreset(HashSet<string> enabled)
    {
        ApplyPreset(enabled.Contains);
    }

    private static void ApplyPreset(Func<string, bool> isEnabled)
    {
        foreach (var key in Main.ModConfig.GetKeys())
        {
            if (key.EndsWith("Separator", StringComparison.Ordinal))
                continue;

            Main.ModConfig.SetProperty(key, isEnabled(key));
        }
    }
}