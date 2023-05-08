using CommandSystem;

using System;

namespace Permissions.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SaveCommand : ICommand
    {
        public string Command { get; } = "perms_save";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Saves all permissions.";

        [Permission("perms.save")]
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PermissionFile.Save();

            response = "Saved all permissions.";
            return true;
        }
    }
}