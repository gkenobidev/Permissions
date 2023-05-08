using CommandSystem;

using System;

namespace Permissions.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class AssignCommand : ICommand
    {
        public string Command { get; } = "perms_assign";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Assigns a group to a permission level.";

        [Permission("perms.assign")]
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = $"Invalid usage! perms_assign <group key> <level name>";
                return false;
            }

            var groupKey = arguments.At(0);
            var levelName = arguments.At(1);

            if (!PermissionFile.TryGetLevelByName(levelName, out var level))
            {
                response = $"Failed to find a permission level by name of {levelName}";
                return true;
            }

            PermissionFile.Permissions[groupKey] = level.Name;
            PermissionFile.Save();

            response = $"Assigned group {groupKey} to permission level {level.Name}.";
            return true;
        }
    }
}