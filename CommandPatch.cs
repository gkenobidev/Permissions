using HarmonyLib;

using PluginAPI.Enums;
using PluginAPI.Events;

using RemoteAdmin;
using RemoteAdmin.Communication;

using System.Linq;
using System;

using helpers;

using Log = PluginAPI.Core.Log;

namespace Permissions
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static class CommandPatch
    {
        public static bool Prefix(string q, CommandSender sender, ref string __result)
        {
            if (q.StartsWith("$"))
            {
                var array = q.Remove(0, 1).Split(' ');

                if (array.Length <= 1)
                {
                    __result = null;
                    return false;
                }

                if (!int.TryParse(array[0], out var key))
                {
                    __result = null;
                    return false;
                }

                if (!CommunicationProcessor.ServerCommunication.TryGetValue(key, out var value))
                {
                    __result = null;
                    return false;
                }

                value.ReceiveData(sender, string.Join(" ", array.Skip(1)));

                __result = null;
                return false;
            }
            else
            {
                if (sender is PlayerCommandSender playerCommandSender)
                {
                    if (q.StartsWith("@"))
                    {
                        if (!CommandProcessor.CheckPermissions(sender, "Admin Chat", PlayerPermissions.AdminChat))
                        {
                            playerCommandSender?.ReferenceHub.queryProcessor.TargetAdminChatAccessDenied(playerCommandSender.ReferenceHub.connectionToClient);

                            __result = "You don't have permissions to access Admin Chat.";
                            return false;
                        }

                        q += $" ~{sender.Nickname}";

                        foreach (var hub in ReferenceHub.AllHubs)
                        {
                            if (hub.Mode != ClientInstanceMode.ReadyClient) continue;
                            if (hub.serverRoles.AdminChatPerms || hub.serverRoles.RaEverywhere)
                            {
                                hub.queryProcessor.TargetReply(hub.connectionToClient, q, true, false, string.Empty);
                            }
                        }

                        __result = null;
                        return false;
                    }
                    else
                    {
                        var args = q
                            .Trim()
                            .Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);

                        if (!EventManager.ExecuteEvent(ServerEventType.RemoteAdminCommand, sender, args[0], args.Skip(1).ToArray()))
                        {
                            __result = null;
                            return false;
                        }

                        if (CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(args[0], out var command))
                        {
                            var commandType = command.GetType();

                            Log.Debug($"Executing command: {command.Command} ({commandType.FullName})", EntryPoint.Config.ShowDebug, "Command Patch");

                            var commandMethod = commandType.GetMethod("Execute");

                            if (commandMethod != null)
                            {
                                if (commandMethod.TryGetAttribute<PermissionAttribute>(out var permission))
                                {
                                    Log.Debug($"Found attribute on method {commandType.FullName}::{commandMethod.Name} - {permission._requiredPerms}", EntryPoint.Config.ShowDebug, "Command Patch");

                                    if (!PermissionFile.IsAllowed(sender, permission._requiredPerms))
                                    {
                                        Log.Debug($"{sender.LogName} is not allowed to use {command.Command}!", EntryPoint.Config.ShowDebug, "Command Patch");

                                        __result = EntryPoint.Config.MissingPermissionsResponse.Replace("%missing%", permission._requiredPerms);
                                        return false;
                                    }

                                    Log.Debug($"{sender.LogName} is allowed to use the {command.Command} command.", EntryPoint.Config.ShowDebug, "Command Patch");

                                    return true;
                                }
                                else
                                {
                                    Log.Debug($"Command {command.Command} ({commandType.FullName}::{commandMethod.Name}) does not have the permission attribute, skipping.", EntryPoint.Config.ShowDebug, "Command Patch");
                                    return true;
                                }
                            }
                            else
                            {
                                Log.Error($"Failed to find the Execute method of command {command.Command} in type {commandType.FullName}!");
                                return true;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}