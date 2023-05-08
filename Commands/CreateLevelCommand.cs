using CommandSystem;

using System;
using System.Linq;

namespace Permissions.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class CreateLevelCommand : ICommand
    {
        public string Command { get; } = "perms_createlevel";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Creates a permission level.";

        [Permission("perms.create")]
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 4)
            {
                response = "Invalid usage! perms_createlevel <name> <points> <nodes> <isOverride>";
                return false;
            }

            var name = arguments.At(0);
            
            if (!int.TryParse(arguments.At(1), out var points))
            {
                response = "The <points> argument has to be a valid integer.";
                return false;
            }

            var nodes = arguments.At(2).Split(',');

            if (!bool.TryParse(arguments.At(3), out var isOverride))
            {
                response = "The <isOverride> argument has to be a valid boolean.";
                return false;
            }

            if (PermissionFile.Levels.Any(x => x.Name == name))
            {
                PermissionFile.Levels.Remove(PermissionFile.Levels.First(x => x.Name == name));
            }

            PermissionFile.Levels.Add(new PermissionLevel
            {
                AllowedPermissionNodes = nodes,
                IsOverride = isOverride,
                Name = name,
                Points = points
            });

            PermissionFile.Save();

            response = "Permission level saved.";
            return true;
        }
    }
}