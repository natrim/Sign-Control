using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using System.ComponentModel;

namespace SignControl
{
    public static class Permissions
    {
        [Description("Allows the setting / unsetting of protection")]
        public static readonly string protectsign;
        [Description("User can edit all protected signs")]
        public static readonly string editallsigns;
        [Description("User can remove protection of all protected signs")]
        public static readonly string removesignprotection;
        [Description("User can set / unset sign warpable")]
        public static readonly string warpsign;
        [Description("User can unlock signs if he knows the right password for the sign")]
        public static readonly string canunlocksign;
        [Description("Users can protect / unprotect all signs in region they have build rights to")]
        public static readonly string protectallsigns;


        static Permissions()
        {
            foreach (var field in typeof(Permissions).GetFields())
            {
                field.SetValue(null, field.Name);
            }
        }

        public static void Load()
        {
            var permissions = new List<string>();

            if (!TShock.Groups.groups.Where(@group => @group.Name != "superadmin").Any(@group => group.HasPermission(protectsign)))
            {
                permissions.Add(protectsign);
                permissions.Add(editallsigns);
                permissions.Add(removesignprotection);
                permissions.Add(protectallsigns);
            }

            if (!TShock.Groups.groups.Where(@group => @group.Name != "superadmin").Any(@group => group.HasPermission(warpsign)))
            {
                permissions.Add(warpsign);
            }

            //if we hoowe some permissions and group exists
            if (permissions.Count > 0 && TShock.Groups.GroupExists("trustedadmin"))
                TShock.Groups.AddPermissions("trustedadmin", permissions);

            //add the permission of unlocking to default group (taken from config), if not somepony already hoowes it
            if (!TShock.Groups.groups.Where(@group => @group.Name != "superadmin").Any(@group => group.HasPermission(canunlocksign)))
            {
                if (TShock.Groups.GroupExists(TShock.Config.DefaultRegistrationGroupName))
                {
                    TShock.Groups.AddPermissions(TShock.Config.DefaultRegistrationGroupName, new List<string> { canunlocksign });
                }
                else if (TShock.Groups.GroupExists("default"))
                { //else try default
                    TShock.Groups.AddPermissions("default", new List<string> { canunlocksign });
                }
            }
        }
    }
}