using CommandSystem;

using System;

namespace Permissions.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class ReloadCommand : ICommand
    {
        public string Command => "reload_perms";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Reloads all permissions.";

        [Permission("perms.reload")]
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PermissionFile.Load();

            response = "Permissions reloaded.";
            return true;
        }
    }
}