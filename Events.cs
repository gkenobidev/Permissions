using PluginAPI.Enums;
using PluginAPI.Core.Attributes;

namespace Permissions
{
    public class Events
    {
        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void OnWaitingForPlayers()
        {
            PermissionFile.Load();
        }
    }
}