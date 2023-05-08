using CommandSystem;

using helpers.Configuration;
using helpers.Configuration.Converters.Json;

using PluginAPI.Core;

using System.Collections.Generic;
using System.Linq;

namespace Permissions
{
    public static class PermissionFile
    {
        private static IniConfigHandler _iniHandler;

        [IniConfig("Levels", null, "Assignable permission levels.")]
        public static List<PermissionLevel> Levels { get; set; } = new List<PermissionLevel>()
        {
            new PermissionLevel()
        };

        [IniConfig("Permissions", null, "Assignable permissions.")]
        public static Dictionary<string, string> Permissions { get; set; } = new Dictionary<string, string>()
        {
            ["example"] = "examplelevel"
        };

        public static PermissionsHandler Handler { get => ServerStatic.PermissionsHandler; }
        public static string Path { get; set; }

        public static bool TryGetLevel(string group, out PermissionLevel level)
            => (Permissions.TryGetValue(group, out var levelName) 
                ? (level = Levels.FirstOrDefault(x => x.Name == levelName)) 
                : (level = Levels.FirstOrDefault(x => x.Name == group))) != null;

        public static bool TryGetLevelByName(string levelName, out PermissionLevel level)
            => (level = Levels.FirstOrDefault(x => x.Name == levelName)) != null;

        public static bool IsAllowed(ICommandSender sender, string permissionId)
        {
            Log.Debug($"Checking permissions of {sender.LogName} ({permissionId})", EntryPoint.Config.ShowDebug, $"Permission File");

            if (Player.TryGet(sender, out var player))
            {
                if (player.IsServer)
                {
                    Log.Debug($"Requested player is a server, which overrides all permissions.", EntryPoint.Config.ShowDebug, $"Permission File");
                    return true;
                }

                if (Permissions.TryGetValue(player.UserId, out var permissions)
                    || Permissions.TryGetValue(player.IpAddress, out permissions)
                    || (Handler._members.TryGetValue(player.UserId, out var playerGroupKey) 
                        && Permissions.TryGetValue(playerGroupKey, out permissions)))
                {
                    if (TryGetLevelByName(permissions, out var level))
                    {
                        if (int.TryParse(permissionId, out var permissionPoints))
                        {
                            Log.Debug($"Parsed score: {permissionPoints}", EntryPoint.Config.ShowDebug, $"Permission File");

                            if (level.IsOverride || level.Points >= permissionPoints)
                            {
                                Log.Debug($"Level {level.Name} either overrides or has the required level.", EntryPoint.Config.ShowDebug, $"Permission File");
                                return true;
                            }
                            else
                            {
                                Log.Debug($"Level {level.Name} is not sufficient.", EntryPoint.Config.ShowDebug, $"Permission File");
                                return false;
                            }
                        }
                        else
                        {
                            if (level.AllowedPermissionNodes.Contains(permissionId) 
                                || level.AllowedPermissionNodes.Contains("*"))
                            {
                                Log.Debug($"Level {level.Name} has the {permissionId} node defined.", EntryPoint.Config.ShowDebug, $"Permission File");
                                return true;
                            }
                            else if (permissionId.Contains("."))
                            {
                                var permParts = permissionId.Split('.');

                                if (level.AllowedPermissionNodes.Any(x => x == $"{permParts[0]}.*"))
                                {
                                    Log.Debug($"Level {level.Name} has the {permParts[0]}.* node defined.", EntryPoint.Config.ShowDebug, $"Permission File");
                                    return true;
                                }
                                else
                                {
                                    Log.Debug($"Level {level.Name} is missing the {permissionId} node.", EntryPoint.Config.ShowDebug, $"Permission File");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Debug($"Level {level.Name} is missing the {permissionId} node.", EntryPoint.Config.ShowDebug, $"Permission File");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Log.Debug($"Failed to retrieve a level by it's name ({permissions})", EntryPoint.Config.ShowDebug, $"Permission File");
                        return false;
                    }
                }
                else
                {
                    Log.Debug($"Player {player.Nickname} ({player.UserId}) does not have a defined permission level.", EntryPoint.Config.ShowDebug, $"Permission File");
                    return false;
                }
            }
            else
            {
                Log.Debug($"Failed to retrieve a player from the provided ICommandSender object.", EntryPoint.Config.ShowDebug, $"Permission File");
                return false;
            }
        }

        public static void Load()
        {
            if (_iniHandler is null) BuildHandler();

            Log.Debug($"Loading ..", EntryPoint.Config.ShowDebug, $"Permission File");
            _iniHandler.Read();
            Log.Debug($"Loaded {Levels.Count} levels with {Permissions.Count} permissions.", EntryPoint.Config.ShowDebug, $"Permission File");
        }

        public static void Save() 
        {
            Log.Debug($"Saving ..", EntryPoint.Config.ShowDebug, $"Permission File");
            _iniHandler.Save();
            Log.Debug($"Saved {Levels.Count} levels with {Permissions.Count} permissions.", EntryPoint.Config.ShowDebug, $"Permission File");
        }

        public static void Validate()
        {
            Log.Debug($"Validating permissions ..", EntryPoint.Config.ShowDebug, $"Permission File");

            var modifiedPerms = 0;

            foreach (var pair in Permissions)
            {
                if (!TryGetLevel(pair.Key, out var level))
                {
                    Permissions.Remove(pair.Key);
                    Log.Warning($"Removed group: {pair.Key} - invalid level", $"Permission File");

                    modifiedPerms++;
                }
            }

            Log.Debug($"Permissions validated; modified {modifiedPerms} permissions.", EntryPoint.Config.ShowDebug, $"Permission File");

            Save();
        }

        private static void BuildHandler()
        {
            Log.Debug($"Building the INI config handler ..", EntryPoint.Config.ShowDebug, $"Permission File");

            new IniConfigBuilder()
                .WithConverter<JsonConfigConverter>()
                .WithGlobalPath(Path)
                .WithType(typeof(PermissionFile), null)
                .Register(ref _iniHandler);

            Log.Debug($"Config handler built.", EntryPoint.Config.ShowDebug, $"Permission File");
        }
    }
}