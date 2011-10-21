using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace SignControl
{
    internal class Commands
    {
        public static void Load()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("protectsign", Set, "sset", "setsign"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectsign", UnSet, "sunset", "unsetsign"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectsign", CancelSet, "scset", "scunset",
                                                            "cancelsetsign", "cancelunsetsign"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpsign", WarpSet, "swarpset", "ssetwarp", "swset"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpsign", WarpUnSet, "swarpunset", "sunsetwarp", "swunset"));

            //everyone can unlock
            TShockAPI.Commands.ChatCommands.Add(new Command(UnLock, "sunlock", "unlocksign", "signunlock"));

            //add permissions to db if not exists
            var perm = TShock.Groups.groups.Where(@group => @group.Name != "superadmin").Any(@group => group.HasPermission("protectsign"));
            if (perm) return;
            var permissions = new List<string> {"protectsign", "editallsigns", "removesignprotection"};
            TShock.Groups.AddPermissions("trustedadmin", permissions);
        }

        private static void Set(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].GetState() == SettingState.Setting)
            {
                SignControl.Players[args.Player.Index].PasswordForSign = "";
                SignControl.Players[args.Player.Index].SetState(SettingState.None);
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
                SignControl.Players[args.Player.Index].SetState(SettingState.Setting);
                args.Player.SendMessage("Open a sign to protect it.", Color.BlueViolet);
            }
        }

        private static void UnLock(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].GetState() == SettingState.UnLocking)
            {
                SignControl.Players[args.Player.Index].PasswordForSign = "";
                SignControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                if (args.Player.Group.HasPermission("editallsigns"))
                {
                    args.Player.SendMessage(
                        "You dont need to unlock signs because you have permission to edit all signs!", Color.BlueViolet);
                    return;
                }

                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage("You must enter password to unlock sign!", Color.Red);
                    return;
                }

                SignControl.Players[args.Player.Index].PasswordForSign = args.Parameters[0];
                SignControl.Players[args.Player.Index].SetState(SettingState.UnLocking);
                args.Player.SendMessage("Open a sign to unlock it.", Color.BlueViolet);
            }
        }

        private static void UnSet(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].GetState() == SettingState.Deleting)
            {
                SignControl.Players[args.Player.Index].PasswordForSign = "";
                SignControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                if (args.Player.Group.HasPermission("removesignprotection"))
                {
                    SignControl.Players[args.Player.Index].SetState(SettingState.Deleting);
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
                    SignControl.Players[args.Player.Index].SetState(SettingState.Deleting);
                    args.Player.SendMessage("Open a sign to delete it's protection.", Color.BlueViolet);
                }
            }
        }

        private static void CancelSet(CommandArgs args)
        {
            SignControl.Players[args.Player.Index].PasswordForSign = "";
            SignControl.Players[args.Player.Index].SetState(SettingState.None);
            args.Player.SendMessage("Selection of sign canceled.", Color.BlueViolet);
        }

        private static void WarpSet(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].GetState() == SettingState.WarpSetting)
            {
                SignControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage("You must enter a warp the sign warps to!", Color.Red);
                    return;
                }

                var warp = TShock.Warps.FindWarp(args.Parameters[0]);
                if (warp.WarpName == null)
                {
                    args.Player.SendMessage("That is an invalid warp name!", Color.Red);
                    return;
                }
                SignControl.Players[args.Player.Index].WarpForSign = warp.WarpName;
                SignControl.Players[args.Player.Index].SetState(SettingState.WarpSetting);
                args.Player.SendMessage("Open a sign to make it warp-able.", Color.BlueViolet);
            }
        }

        private static void WarpUnSet(CommandArgs args)
        {
            if (SignControl.Players[args.Player.Index].GetState() == SettingState.DeletingWarp)
            {
                SignControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a sign.", Color.BlueViolet);
            }
            else
            {
                SignControl.Players[args.Player.Index].SetState(SettingState.DeletingWarp);
                args.Player.SendMessage("Open a sign to delete its warp.", Color.BlueViolet);
            }
        }
    }
}