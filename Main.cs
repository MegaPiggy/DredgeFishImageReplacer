using HarmonyLib;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using Winch.Components;
using Winch.Config;
using Winch.Core;
using Winch.Core.API;
using Winch.Util;
using Yarn;

namespace FishImageReplacer
{
    public static class Main
    {
        private static ModConfig _modConfig;
        public static ModConfig ModConfig =>
            _modConfig ??= ModConfig.GetConfig();

        private static ModAssembly _modAssembly;
        public static ModAssembly ModAssembly =>
            _modAssembly ??= ModAssemblyLoader.GetCurrentMod();
        public static string BasePath => ModAssembly.BasePath;
        public static string GUID => ModAssembly.GUID;

        public static void Initialize()
        {
            WinchCore.Log.Info($"My mod {ModAssembly.Name} is initializing!");

            // Game world
            ApplicationEvents.Instance.OnGameLoaded += OnGameLoaded;

            // Gameplay logic
            GameManager.Instance.OnGameStarted += OnGameStarted;
            GameManager.Instance.OnGameEnded += OnGameEnded;

            // Config
            DredgeEvent.OnBuildModConfigMenu += OnBuildModConfigMenu;
            ModConfig.OnConfigValueChanged += ModConfig_OnConfigValueChanged;
        }

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

        private static void OnBuildModConfigMenu(ModAssembly mod, ModsTab tab)
        {
            if (mod.GUID != GUID) return;

            var presetButtons = new[]
            {
                tab.AddOptionButtonLocalized(
                    "PresetNone",
                    "megapiggy.fishimagereplacer.config.preset.none",
                    "megapiggy.fishimagereplacer.config.preset.none.tooltip",
                    () =>
                    {
                        ApplyNonePreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetAll",
                    "megapiggy.fishimagereplacer.config.preset.all",
                    "megapiggy.fishimagereplacer.config.preset.all.tooltip",
                    () =>
                    {
                        ApplyAllPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetAberrations",
                    "megapiggy.fishimagereplacer.config.preset.aberrations",
                    "megapiggy.fishimagereplacer.config.preset.aberrations.tooltip",
                    () =>
                    {
                        ApplyAberrationsPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetSharks",
                    "megapiggy.fishimagereplacer.config.preset.sharks",
                    "megapiggy.fishimagereplacer.config.preset.sharks.tooltip",
                    () =>
                    {
                        ApplySharksPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetEels",
                    "megapiggy.fishimagereplacer.config.preset.eels",
                    "megapiggy.fishimagereplacer.config.preset.eels.tooltip",
                    () =>
                    {
                        ApplyEelsPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetCrustaceansAndArthropods",
                    "megapiggy.fishimagereplacer.config.preset.crustaceansandarthropods",
                    "megapiggy.fishimagereplacer.config.preset.crustaceansandarthropods.tooltip",
                    () =>
                    {
                        ApplyCrustaceansAndArthropodsPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetDeepSea",
                    "megapiggy.fishimagereplacer.config.preset.deepsea",
                    "megapiggy.fishimagereplacer.config.preset.deepsea.tooltip",
                    () =>
                    {
                        ApplyDeepSeaPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetPrehistoric",
                    "megapiggy.fishimagereplacer.config.preset.prehistoric",
                    "megapiggy.fishimagereplacer.config.preset.prehistoric.tooltip",
                    () =>
                    {
                        ApplyPrehistoricPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetCephalopods",
                    "megapiggy.fishimagereplacer.config.preset.cephalopods",
                    "megapiggy.fishimagereplacer.config.preset.cephalopods.tooltip",
                    () =>
                    {
                        ApplyCephalopodsPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetLeggyCreatures",
                    "megapiggy.fishimagereplacer.config.preset.leggycreatures",
                    "megapiggy.fishimagereplacer.config.preset.leggycreatures.tooltip",
                    () =>
                    {
                        ApplyLeggyCreaturesPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetTeethAndMouths",
                    "megapiggy.fishimagereplacer.config.preset.teethandmouths",
                    "megapiggy.fishimagereplacer.config.preset.teethandmouths.tooltip",
                    () =>
                    {
                        ApplyTeethAndMouthsPreset();
                        tab.RefreshAllInputs();
                    }
                ),

                tab.AddOptionButtonLocalized(
                    "PresetBorderlineMonster",
                    "megapiggy.fishimagereplacer.config.preset.borderlinemonster",
                    "megapiggy.fishimagereplacer.config.preset.borderlinemonster.tooltip",
                    () =>
                    {
                        ApplyBorderlineMonsterPreset();
                        tab.RefreshAllInputs();
                    }
                )
            };

            tab.MoveOptionsToStart(presetButtons);
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
            return ModConfig.GetKeys()
                .Where(key => IsInFamily(key, fishIds));
        }

        private static void ToggleFamily(HashSet<string> fishIds)
        {
            var keys = GetFamilyKeys(fishIds).ToList();

            // If anything is disabled, enable the whole family.
            // Otherwise disable the whole family.
            var enabled = keys.Any(key => !ModConfig.GetProperty<bool>(key));

            foreach (var key in keys)
            {
                ModConfig.SetProperty(key, enabled);
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
            foreach (var key in ModConfig.GetKeys())
            {
                if (key.EndsWith("Separator", StringComparison.Ordinal))
                    continue;

                ModConfig.SetProperty(key, isEnabled(key));
            }
        }

        private static void OnGameLoaded()
        {
            FishImageReplacement.Initialize();
        }

        private static void OnGameStarted()
        {
            DefaultConfigGenerator.Generate(Path.Combine(BasePath, "default_config.json"));
        }

        private static void OnGameEnded()
        {
            FishImageReplacement.Clear();
        }

        private static void ModConfig_OnConfigValueChanged(string key)
        {
            FishImageReplacement.ApplyReplacement(key);
        }
    }
}