using TShockAPI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SignControl
{
    class Commands
    {
        public static void Load()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("protectsign", Set, "sset", "setsign"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectsign", UnSet, "sunset", "unsetsign"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectsign", CancelSet, "scset", "scunset", "cancelsetsign", "cancelunsetsign"));

            //everyone can unlock
            TShockAPI.Commands.ChatCommands.Add(new Command(UnLock, "sunlock", "unlocksign", "signunlock"));

            //add permissions to db if not exists
            bool perm = false;
            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("protectsign"))
                    {
                        perm = true;
                    }
                }
            }
            if (!perm)
            {
                List<string> permissions = new List<string>();
                permissions.Add("protectsign");
                permissions.Add("editallsigns");
                permissions.Add("removesignprotection");
                TShock.Groups.AddPermissions("trustedadmin", permissions);
            }
        }

        private static void Set(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].getState() == SettingState.Setting)
            {
                SignControl.Players[args.Player.Index].PasswordForSign = "";
                SignControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage("You must enter password to protect sign!", Color.Red);
                    return;
                }

                SignControl.Players[args.Player.Index].PasswordForSign = args.Parameters[0];
                SignControl.Players[args.Player.Index].setState(SettingState.Setting);
                args.Player.SendMessage("Open a sign to protect it.", Color.BlueViolet);
            }
        }

        private static void UnLock(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].getState() == SettingState.UnLocking)
            {
                SignControl.Players[args.Player.Index].PasswordForSign = "";
                SignControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                if (args.Player.Group.HasPermission("editallsigns"))
                {
                    args.Player.SendMessage("You dont need to unlock signs because you have permission to edit all signs!", Color.BlueViolet);
                    return;
                }

                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage("You must enter password to unlock sign!", Color.Red);
                    return;
                }

                SignControl.Players[args.Player.Index].PasswordForSign = args.Parameters[0];
                SignControl.Players[args.Player.Index].setState(SettingState.UnLocking);
                args.Player.SendMessage("Open a sign to unlock it.", Color.BlueViolet);
            }
        }

        private static void UnSet(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].getState() == SettingState.Deleting)
            {
                SignControl.Players[args.Player.Index].PasswordForSign = "";
                SignControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                if (args.Player.Group.HasPermission("removesignprotection"))
                {
                    SignControl.Players[args.Player.Index].setState(SettingState.Deleting);
                    args.Player.SendMessage("Open a sign to delete it's protection.", Color.BlueViolet);
                }
                else
                {
                    if (args.Parameters.Count != 1)
                    {
                        args.Player.SendMessage("You must enter password to remove sign protection!", Color.Red);
                        return;
                    }

                    SignControl.Players[args.Player.Index].PasswordForSign = args.Parameters[0];
                    SignControl.Players[args.Player.Index].setState(SettingState.Deleting);
                    args.Player.SendMessage("Open a sign to delete it's protection.", Color.BlueViolet);
                }
            }
        }

        private static void CancelSet(CommandArgs args)
        {
            SignControl.Players[args.Player.Index].PasswordForSign = "";
            SignControl.Players[args.Player.Index].setState(SettingState.None);
            args.Player.SendMessage("Selection of sign canceled.", Color.BlueViolet);
        }
    }
}
