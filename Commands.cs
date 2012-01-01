using TShockAPI;

namespace SignControl
{
	internal class Commands
	{
		public static void Load ()
		{
			TShockAPI.Commands.ChatCommands.Add (new Command (Permissions.protectsign, Set, "sset", "setsign") { DoLog = false });
			TShockAPI.Commands.ChatCommands.Add (new Command (Permissions.protectsign, UnSet, "sunset", "unsetsign") { DoLog = false });
			TShockAPI.Commands.ChatCommands.Add (new Command (Permissions.protectsign, CancelSet, "scset", "scunset", "cancelsetsign", "cancelunsetsign"));
			TShockAPI.Commands.ChatCommands.Add (new Command (Permissions.warpsign, WarpSet, "swarpset", "ssetwarp", "swset"));
			TShockAPI.Commands.ChatCommands.Add (new Command (Permissions.warpsign, WarpUnSet, "swarpunset", "sunsetwarp", "swunset"));

			//every registered pony can unlock - changeable with the permission
			TShockAPI.Commands.ChatCommands.Add (new Command (Permissions.canunlocksign, UnLock, "sunlock", "unlocksign", "signunlock") { DoLog = false });
		}

		private static void Set (CommandArgs args)
		{
			if (SignControl.Players [args.Player.Index].GetState () == SettingState.Setting) {
				SignControl.Players [args.Player.Index].PasswordForSign = "";
				SignControl.Players [args.Player.Index].SetState (SettingState.None);
				args.Player.SendMessage (Messages.stopSelecting, Color.BlueViolet);
			} else {
				if (args.Parameters.Count < 1) {
					args.Player.SendMessage (Messages.enterPassword, Color.Red);
					return;
				}
				
				if (args.Parameters.Count == 3 && args.Parameters [1].ToUpper () == "REGION") { //sets password for all signs in region
					if (!args.Player.Group.HasPermission (Permissions.editallsigns) || !args.Player.Group.HasPermission (TShockAPI.Permissions.manageregion)) {
						args.Player.SendMessage (Messages.noPermissionRegion, Color.Red);
						return;
					}
					
					var region = TShock.Regions.GetRegionByName (args.Parameters [2]);
					
					if (region == null) {
						args.Player.SendMessage (Messages.noRegion, Color.Red);
						return;
					}
					
					for (int l = 0; l < Terraria.Sign.maxSigns; l++) {
						if (Terraria.Main.sign [l] != null) {
							if (region.InArea (new Rectangle (Terraria.Main.sign [l].x, Terraria.Main.sign [l].y, 0, 0))) {
								var sign = SignManager.GetSign (l);
								
								var vasnull = false;
								if(sign == null){
									sign = new Sign();
									vasnull = true;
								}
								
								sign.SetID(l);
								sign.SetPassword (args.Parameters [0]);
								sign.SetPosition (Terraria.Main.sign [l].x, Terraria.Main.sign [l].y);
								
								if(vasnull)
									SignManager.SetSign(l, sign);
								
								SignControl.Players [args.Player.Index].AddSignAccess (l);
							}
						}
					}
					
					args.Player.SendMessage (string.Format (Messages.regionLocked, region.Name), Color.BlueViolet);
					
				} else if (args.Parameters.Count > 3) {
					args.Player.SendMessage (Messages.tooManyParams, Color.Red);
					return;
				} else { //normal selecting
					SignControl.Players [args.Player.Index].PasswordForSign = args.Parameters [0];
					SignControl.Players [args.Player.Index].SetState (SettingState.Setting);
					args.Player.SendMessage (string.Format (Messages.openSignTo, Messages.protect), Color.BlueViolet);
				}
			}
		}

		private static void UnLock (CommandArgs args)
		{
			if (SignControl.Players [args.Player.Index].GetState () == SettingState.UnLocking) {
				SignControl.Players [args.Player.Index].PasswordForSign = "";
				SignControl.Players [args.Player.Index].SetState (SettingState.None);
				args.Player.SendMessage (Messages.stopSelecting, Color.BlueViolet);
			} else {
				if (args.Player.Group.HasPermission (Permissions.editallsigns)) {
					args.Player.SendMessage (Messages.notNeedUnlock, Color.BlueViolet);
					return;
				}

				if (args.Parameters.Count != 1) {
					args.Player.SendMessage (Messages.enterPassword, Color.Red);
					return;
				}

				SignControl.Players [args.Player.Index].PasswordForSign = args.Parameters [0];
				SignControl.Players [args.Player.Index].SetState (SettingState.UnLocking);
				args.Player.SendMessage (string.Format (Messages.openSignTo, Messages.unlock), Color.BlueViolet);
			}
		}

		private static void UnSet (CommandArgs args)
		{
			if (SignControl.Players [args.Player.Index].GetState () == SettingState.Deleting) {
				SignControl.Players [args.Player.Index].PasswordForSign = "";
				SignControl.Players [args.Player.Index].SetState (SettingState.None);
				args.Player.SendMessage (Messages.stopSelecting, Color.BlueViolet);
			} else {
				if (args.Player.Group.HasPermission (Permissions.removesignprotection)) {
					SignControl.Players [args.Player.Index].SetState (SettingState.Deleting);
					args.Player.SendMessage (string.Format (Messages.openSignTo, Messages.delete), Color.BlueViolet);
				} else {
					if (args.Parameters.Count < 1) {
						args.Player.SendMessage (Messages.enterPassword, Color.Red);
						return;
					}
					
					if (args.Parameters.Count == 3 && args.Parameters [1].ToUpper () == "REGION") { //removes protection off all signs in region
						if (!args.Player.Group.HasPermission (Permissions.removesignprotection) || !args.Player.Group.HasPermission (TShockAPI.Permissions.manageregion)) {
							args.Player.SendMessage (Messages.noPermissionRegion, Color.Red);
							return;
						}
					
						var region = TShock.Regions.GetRegionByName (args.Parameters [2]);
					
						if (region == null) {
							args.Player.SendMessage (Messages.noRegion, Color.Red);
							return;
						}
					
						for (int l = 0; l < Terraria.Sign.maxSigns; l++) {
							if (Terraria.Main.sign [l] != null) {
								if (region.InArea (new Rectangle (Terraria.Main.sign [l].x, Terraria.Main.sign [l].y, 0, 0))) {
									var sign = SignManager.GetSign (l);
									if(sign != null && sign.IsLocked()){
										sign.SetPassword ("");
										SPlayer.RemoveSignAccessFromAll (l);
									}
								}
							}
						}
					
						args.Player.SendMessage (string.Format (Messages.regionUnLocked, region.Name), Color.BlueViolet);
					
					} else if (args.Parameters.Count > 3) {
						args.Player.SendMessage (Messages.tooManyParams, Color.Red);
						return;
					} else { //normal selecting
						SignControl.Players [args.Player.Index].PasswordForSign = args.Parameters [0];
						SignControl.Players [args.Player.Index].SetState (SettingState.Deleting);
						args.Player.SendMessage (string.Format (Messages.openSignTo, Messages.delete), Color.BlueViolet);
					}
				}
			}
		}

		private static void CancelSet (CommandArgs args)
		{
			SignControl.Players [args.Player.Index].PasswordForSign = "";
			SignControl.Players [args.Player.Index].WarpForSign = "";
			SignControl.Players [args.Player.Index].SetState (SettingState.None);
			args.Player.SendMessage (Messages.stopSelecting, Color.BlueViolet);
		}

		private static void WarpSet (CommandArgs args)
		{
			if (SignControl.Players [args.Player.Index].GetState () == SettingState.WarpSetting) {
				SignControl.Players [args.Player.Index].WarpForSign = "";
				SignControl.Players [args.Player.Index].SetState (SettingState.None);
				args.Player.SendMessage (Messages.stopSelecting, Color.BlueViolet);
			} else {
				if (args.Parameters.Count != 1) {
					args.Player.SendMessage (Messages.enterWarp, Color.Red);
					return;
				}

				var warp = TShock.Warps.FindWarp (args.Parameters [0]);
				if (warp.WarpName == null) {
					args.Player.SendMessage (Messages.invalidWarp, Color.Red);
					return;
				}
				SignControl.Players [args.Player.Index].WarpForSign = warp.WarpName;
				SignControl.Players [args.Player.Index].SetState (SettingState.WarpSetting);
				args.Player.SendMessage (string.Format (Messages.openSignTo, Messages.warpable), Color.BlueViolet);
			}
		}

		private static void WarpUnSet (CommandArgs args)
		{
			if (SignControl.Players [args.Player.Index].GetState () == SettingState.DeletingWarp) {
				SignControl.Players [args.Player.Index].SetState (SettingState.None);
				args.Player.SendMessage (Messages.stopSelecting, Color.BlueViolet);
			} else {
				SignControl.Players [args.Player.Index].SetState (SettingState.DeletingWarp);
				args.Player.SendMessage (string.Format (Messages.openSignTo, Messages.unwarpable), Color.BlueViolet);
			}
		}
	}
}