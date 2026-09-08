using HarmonyLib;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
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
            //DredgeEvent.OnTitleOpen += OnTitleOpen;
            ApplicationEvents.Instance.OnTitleClosed += OnTitleClosed;

            // Game world
            ApplicationEvents.Instance.OnGameLoaded += OnGameLoaded;
            ApplicationEvents.Instance.OnGameUnloaded += OnGameUnloaded;

            // Gameplay logic
            GameManager.Instance.OnGameStarted += OnGameStarted;
            GameManager.Instance.OnGameEnded += OnGameEnded;
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

            ItemUtil.GetAllFishItemData().ForEach(fishItemData =>
            {
                if (!fishItemData.IsVanilla())
                    return;

                var entitlements = fishItemData.entitlementsRequired;

                WinchCore.Log.Info(
                    $"Fish Item: {fishItemData.id} | DLC: " +
                    $"{(entitlements == null || entitlements.Count == 0 ? "None" : string.Join(", ", entitlements))}"
                );
            });
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