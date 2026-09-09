using HarmonyLib;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
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

            // Title screen
            DredgeEvent.OnTitleOpen += OnTitleOpen;
            ApplicationEvents.Instance.OnTitleClosed += OnTitleClosed;

            // Game world
            ApplicationEvents.Instance.OnGameLoaded += OnGameLoaded;
            ApplicationEvents.Instance.OnGameUnloaded += OnGameUnloaded;

            // Gameplay logic
            GameManager.Instance.OnGameStarted += OnGameStarted;
            GameManager.Instance.OnGameEnded += OnGameEnded;

            // Config
            DredgeEvent.OnBuildModConfigMenu += OnBuildModConfigMenu;
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

            // Prehistoric arthropod-ish creatures
            "trilobite",
            "sea-scorpion",
            "opabinia",
            "kerygmachela",
            "sollasina"
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
            "frilled-shark"
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
            "xiphactinus"
        };

        private static readonly HashSet<string> Cephalopods = new()
        {
            "squid",
            "colossal-squid",
            "firefly-squid",
            "glowing-octopus"
        };

        private static readonly HashSet<string> LeggyCreatures = new()
        {
            "decorator-crab",
            "giant-amphipod",
            "horseshoe-crab",
            "king-crab",
            "spider-crab",
            "squat-lobster",
            "spiny-lobster",
            "sea-scorpion",
            "trilobite"
        };

        private static readonly HashSet<string> TeethAndMouths = new()
        {
            "anglerfish",
            "fangtooth",
            "stoplight-loosejaw",
            "giant-dragonfish",
            "viperfish",
            "gulper-eel",
            "nipponites",
            "goblin-shark",
            "frilled-shark",
            "black-grouper",
            "stonefish"
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

        private static void OnTitleOpen()
        {
            // When the title screen is opened
            // Best for modifying the title screen.

        }

        private static void OnTitleClosed()
        {
            // When the title screen is closed
            // Happens when leaving the title screen to load into the world.

        }

        private static void OnGameLoaded()
        {
            // When the game world finishes loading
            // Happens after loading from the title screen or after loading the last save from game over screen.
            // Best for creating or modifying world objects and/or data.

        }

        private static void OnGameUnloaded()
        {
            // When the game world is unloaded
            // Happens when leaving to the title screen or loading the last save.
            // Best for logic that should run after the world has been unloaded.

        }

        private static void OnGameStarted()
        {
            // When the game world starts
            // Happens when loading screen fades and intro cutscene finishes and the player can control the game.
            // Best for gameplay logic and subscribing to gameplay events.

            DefaultConfigGenerator.Generate(Path.Combine(BasePath, "default_config.json"));
        }

        private static void OnGameEnded()
        {
            // When the current game session ends
            // Happens on game over, or before the world is unloaded if it has not already ended.
            // Game over does not unload the world by itself. (only happens when player presses continue or quit to title)
            // Best for stopping gameplay logic and unsubscribing from gameplay events.

        }
    }
}