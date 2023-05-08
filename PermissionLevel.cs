namespace Permissions
{
    public class PermissionLevel
    {
        public int Points { get; set; } = 0;

        public string Name { get; set; } = "examplelevel";

        public string[] AllowedPermissionNodes { get; set; } = new string[]
        {
            "example.node"
        };

        public bool IsOverride { get; set; } 
    }
}