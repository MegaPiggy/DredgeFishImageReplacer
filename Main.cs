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
using Winch.Components.UI;
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

#if DEBUG
        public static readonly bool Debug = true;
#else
        public static readonly bool Debug = false;
#endif

        public static void Initialize()
        {
            WinchCore.Log.Success($"My mod {ModAssembly.Name} is initializing!");

            // Game world
            ApplicationEvents.Instance.OnGameLoaded += OnGameLoaded;

            // Gameplay logic
            GameManager.Instance.OnGameStarted += OnGameStarted;
            GameManager.Instance.OnGameEnded += OnGameEnded;

            // Config
            DredgeEvent.OnBuildModConfigMenu += OnBuildModConfigMenu;
            ModConfig.OnConfigValueChanged += ModConfig_OnConfigValueChanged;

            FishImageGuideScreen.Initialize();
        }

        private static void OnBuildModConfigMenu(ModAssembly mod, ModsTab tab)
        {
            if (mod.GUID != Main.GUID)
                return;

            FishImagePresets.OnBuildModConfigMenu(tab);
            FishImageReplacement.OnBuildModConfigMenu(tab);
        }

        private static void OnGameLoaded()
        {
            FishImageReplacement.Initialize();
        }

        private static void OnGameStarted()
        {
            if (Debug)
            {
                DefaultConfigGenerator.Generate(Path.Combine(BasePath, "default_config.json"));
                FishImageReplacement.GenerateTextureMap();
            }
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