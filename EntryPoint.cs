using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using PluginAPI.Helpers;

using HarmonyLib;

using System;
using System.Reflection;

namespace Permissions
{
    public class EntryPoint
    {
        public static EntryPoint Instance { get; set; }
        public static Config Config { get => Instance.ConfigInstance; }

        public static MethodInfo PatchMethodInfo { get; } = typeof(EntryPoint).GetMethod("PatchMethod");
        public static HarmonyMethod HarmonyPatchMethod { get; } = new HarmonyMethod(PatchMethodInfo);

        [PluginConfig]
        public Config ConfigInstance;
        public Harmony Harmony;

        [PluginEntryPoint(
            "Permissions",
            "1.0.0",
            "A plugin that adds an automatic permission system via attributes.",
            "fleccker")]
        public void Load()
        {
            Instance = this;

            Log.Info("Loading permissions ..");

            if (Config.UseGlobal)
            {
                Log.Debug($"Using global config path.", Config.ShowDebug);
                PermissionFile.Path = $"{Paths.PluginAPI}/permissions.ini";
            }
            else
            {
                Log.Debug($"Using server config path.", Config.ShowDebug);
                PermissionFile.Path = $"{Paths.Configs}/permissions.ini";
            }

            Log.Info($"Using file path: {PermissionFile.Path}");

            Log.Debug($"Registering event handlers ..", Config.ShowDebug);
            EventManager.RegisterEvents<Events>(this);
            Log.Debug($"Registered event handlers.", Config.ShowDebug);

            Log.Debug($"Creating a harmony instance ..", Config.ShowDebug);
            Harmony = new Harmony($"permissions.{DateTime.Now.Ticks}");
            Log.Debug($"Harmony instance created.", Config.ShowDebug);

            Log.Debug($"Patching ..", Config.ShowDebug);
            Harmony.PatchAll();
            Log.Debug($"Patching completed.", Config.ShowDebug);

            Log.Info("Loading complete.");
        }

        [PluginUnload]
        public void Unload()
        {
            Instance = null;

            Harmony.UnpatchAll();
            Harmony = null;

            PermissionFile.Save();
            EventManager.UnregisterEvents<Events>(this);

            Log.Debug($"Unloaded.", Config.ShowDebug);
        }

        [PluginReload]
        public void Reload()
        {
            PermissionFile.Load();

            Log.Debug($"Reloaded.", Config.ShowDebug);
        }
    }
}