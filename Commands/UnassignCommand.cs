using CommandSystem;

using System;

namespace Permissions.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class ÚnassignCommand : ICommand
    {
        public string Command { get; } = "perms_unassign";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Removes a permission pair.";

        [Permission("perms.unassign")]
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = $"Invalid usage! perms_unassign <group key>";
                return false;
            }

            var groupKey = arguments.At(0);

            if (!PermissionFile.Permissions.Remove(groupKey))
            {
                response = $"Failed to remove key: {groupKey}";
                return false;
            }

            PermissionFile.Save();

            response = $"Removed key: {groupKey}";
            return true;
        }
    }
}