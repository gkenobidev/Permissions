using System;

namespace Permissions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PermissionAttribute : Attribute
    {
        internal string _requiredPerms;

        public PermissionAttribute(int requiredPoints)
        {
            _requiredPerms = requiredPoints.ToString();
        }

        public PermissionAttribute(string requiredId)
        {
            _requiredPerms = requiredId;
        }
    }
}