using System.ComponentModel;

namespace Permissions
{
    public class Config
    {
        [Description("Whether or not to use global configuration file.")]
        public bool UseGlobal { get; set; } = true;

        [Description("Whether or not to show debug messages.")]
        public bool ShowDebug { get; set; }

        [Description("The response to send when the sender has insufficient permissions.")]
        public string MissingPermissionsResponse { get; set; } = "Your permission level does not have the required permissions to run this command! (Required: %missing%)";
    }
}