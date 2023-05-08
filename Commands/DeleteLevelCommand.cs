using CommandSystem;

using System;

using Utils.NonAllocLINQ;

namespace Permissions.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class DeleteLevelCommand : ICommand
    {
        public string Command { get; } = "perms_deletelevel";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Deletes a permission level.";

        [Permission("perms.delete")]
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Invalid usage! perms_deletelevel <name>";
                return false;
            }

            if (!PermissionFile.Levels.TryGetFirst(x => x.Name == string.Join(" ", arguments), out var level))
            {
                response = "Failed to find that level.";
                return false;
            }

            if (PermissionFile.Levels.Remove(level))
            {
                response = $"Failed to remove level {level.Name}";
                return false;
            }

            PermissionFile.Save();

            response = $"Removed level {level.Name}";
            return true;
        }
    }
}