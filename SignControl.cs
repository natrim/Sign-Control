using System;
using System.ComponentModel;
using System.IO;
using Hooks;
using TShockAPI;
using Terraria;

namespace SignControl
{
    [APIVersion(1, 10)]
    public class SignControl : TerrariaPlugin
    {
        public static SPlayer[] Players = new SPlayer[Main.maxNetPlayers];

        public SignControl(Main game)
            : base(game)
        {
        }

        public override string Name
        {
            get { return "Sign Control"; }
        }

        public override Version Version
        {
            get { return new Version(1, 1, 2); } //TODO: copy Deathmax's auto version check
        }

        public override string Author
        {
            get { return "Natrim & Inspired by Deathmax's Chest-Control"; }
        }

        public override string Description
        {
            get { return "Gives you control over signs."; }
        }

        public override void Initialize()
        {
            NetHooks.GetData += NetHooks_GetData;
            ServerHooks.Leave += ServerHooks_Leave;
			GameHooks.PostInitialize += OnPostInitialize;
            WorldHooks.SaveWorld += OnSaveWorld;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
			{
				NetHooks.GetData -= NetHooks_GetData;
            	ServerHooks.Leave -= ServerHooks_Leave;
				GameHooks.PostInitialize -= OnPostInitialize;
            	WorldHooks.SaveWorld -= OnSaveWorld;
			}
			
            base.Dispose(disposing);
        }

        private void OnSaveWorld(bool resettime, HandledEventArgs e)
        {
            try
            {
                SignManager.Save(); //save signs
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnPostInitialize()
        {
			Console.WriteLine("Initiating Sign-Control...");
			
			//TODO: full support for reloadplugins command ( DeInitialize and better Dispose )
			SignManager.Load();
			Permissions.Load();
			Commands.Load();
			
			//TODO: lazy loading
			for (int i = 0; i < Players.Length; i++)
			{
				Players[i] = new SPlayer(i);
			}
        }

        private void ServerHooks_Leave(int obj)
        {
            Players[obj] = new SPlayer(obj);
        }

        private void NetHooks_GetData(GetDataEventArgs e)
        {
            try
            {
                switch (e.MsgID)
                {
                    case PacketTypes.SignRead: //on open sign set/unset protection
                        using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        {
                            var reader = new BinaryReader(data);
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            reader.Close();

                            int id = Terraria.Sign.ReadSign(x, y);
                            SPlayer player = Players[e.Msg.whoAmI];
                            TSPlayer tplayer = TShock.Players[e.Msg.whoAmI];

                            if (id != -1)
                            {
                                Sign sign = SignManager.GetSign(id);
                                bool messageSent = false;

                                switch (player.GetState())
                                {
                                    case SettingState.Setting:
                                        if (sign.IsLocked())
                                        {
                                            player.SendMessage("This sign is already protected!", Color.Red);
                                            messageSent = true;
                                        }
                                        else
                                        {
                                            sign.SetID(id);
                                            sign.SetPosition(x, y);
                                            sign.SetPassword(player.PasswordForSign);
                                            player.AddSignAccess(id); //unlock this sign for him

                                            player.SendMessage("This sign is now protected.", Color.Red);
                                            messageSent = true;
                                        }

                                        player.SetState(SettingState.None);
                                        break;
                                    case SettingState.Deleting:
                                        if (sign.IsLocked())
                                        {
                                            if (tplayer.Group.HasPermission(Permissions.removesignprotection) ||
                                                sign.CheckPassword(player.PasswordForSign))
                                            {
                                                sign.SetPassword("");
                                                SPlayer.RemoveSignAccessFromAll(id); //remove access to this sign

                                                player.SendMessage("Sign protection removed!", Color.Red);
                                                messageSent = true;
                                            }
                                            else
                                            {
                                                player.SendMessage("Wrong password!", Color.Red);
                                                messageSent = true;
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This sign is not protected!", Color.Red);
                                            messageSent = true;
                                        }

                                        player.SetState(SettingState.None);
                                        break;
                                    case SettingState.UnLocking:
                                        if (sign.IsLocked())
                                        {
                                            if (sign.CheckPassword(player.PasswordForSign))
                                            {
                                                player.AddSignAccess(id); //unlock this sign for him

                                                player.SendMessage("Sign editing unlocked!", Color.Red);
                                                messageSent = true;
                                            }
                                            else
                                            {
                                                player.SendMessage("Wrong password!", Color.Red);
                                                messageSent = true;
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This sign is not protected!", Color.Red);
                                            messageSent = true;
                                        }

                                        player.SetState(SettingState.None);
                                        break;
                                    case SettingState.WarpSetting:
                                        if ((sign.IsLocked() &&
                                             (tplayer.Group.HasPermission(Permissions.editallsigns) || player.CanEditSign(id)))
                                            || !sign.IsLocked())
                                        {
                                            sign.SetWarp(player.WarpForSign);
                                            player.SendMessage("This sign will now warp to : " + player.WarpForSign,
                                                               Color.Plum);
                                            messageSent = true;
                                        }
                                        else
                                        {
                                            player.SendMessage("This sign is protected!", Color.Red);
                                            messageSent = true;
                                        }
                                        player.SetState(SettingState.None);
                                        break;
                                    case SettingState.DeletingWarp:
                                        if (sign.IsWarping())
                                        {
                                            if ((sign.IsLocked() &&
                                                 (tplayer.Group.HasPermission(Permissions.editallsigns) || player.CanEditSign(id)))
                                                || !sign.IsLocked())
                                            {
                                                sign.SetWarp("");
                                                player.SendMessage("This sign is no longer warping.", Color.Red);
                                                messageSent = true;
                                            }
                                            else
                                            {
                                                player.SendMessage("You don't have permission to edit this sign!",
                                                                   Color.Red);
                                                messageSent = true;
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This sign is not warp-enabled.", Color.Red);
                                            messageSent = true;
                                        }
                                        player.SetState(SettingState.None);
                                        break;
                                }

                                if (!messageSent)
                                {
                                    if (sign.IsLocked())
                                    {
                                        if (tplayer.Group.HasPermission(Permissions.editallsigns) || player.CanEditSign(id))
										{
                                            player.SendMessage("This sign is protected. You are able to edit it.", Color.Yellow);
										}
                                        else
										{
                                            player.SendMessage(
                                                "This sign is protected. You are not able to edit it.",
                                                Color.Yellow);
									        player.SendMessage(
                                                "( If you know the password, you can unlock it using \"/sunlock PASSWORD\" command. )",
                                                Color.Yellow);
										}
                                    }
                                    else
                                        player.SendMessage("This sign is not protected.", Color.Yellow);
                                }

                                if (sign.IsWarping())
                                {
                                    var warp = TShock.Warps.FindWarp(sign.GetWarp());
                                    if (warp.WarpName != null)
                                    {
                                        player.Teleport((int) warp.WarpPos.X, (int) warp.WarpPos.Y);
                                        player.SendMessage("You have been teleported to " + warp.WarpName, Color.Blue);
                                    }
                                    else
                                        player.SendMessage("Warp no longer exists!", Color.Red);
                                }
                            }

                            if (player.GetState() != SettingState.None)
                                //if player is still setting something - end his setting
                                player.SetState(SettingState.None);
                        }
                        break;

                    case PacketTypes.SignNew: //editing sign
                        if (!e.Handled)
                        {
                            using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                            {
                                var reader = new BinaryReader(data);
                                var signId = reader.ReadInt16();
                                var x = reader.ReadInt32();
                                var y = reader.ReadInt32();
                                reader.Close();
							
								//FIXME: hacked clients can theoreticaly bypass it ( seems like it checks by id only ) - needs testing
							
                                var id = Terraria.Sign.ReadSign(x, y);
                                var player = Players[e.Msg.whoAmI];
                                var tplayer = TShock.Players[e.Msg.whoAmI];

                                if (id != -1)
                                {
                                    var sign = SignManager.GetSign(id);

                                    if (sign.IsLocked())
                                    {
                                        if (!tplayer.Group.HasPermission(Permissions.editallsigns) && !player.CanEditSign(id))
                                            //if player doesnt have permission, then break and message
                                        {
                                            e.Handled = true;
                                            tplayer.SendMessage(
                                                "This sign is protected with password. Your changes would be not visible to other players.",
                                                Color.IndianRed);
                                            tplayer.SendMessage(
                                                "( To edit this sign unlock it using \"/sunlock PASSWORD\" command. )",
                                                Color.IndianRed);
                                            tplayer.SendData(PacketTypes.SignNew, "", id);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case PacketTypes.TileKill:
                    case PacketTypes.Tile:
                        using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        {
                            var reader = new BinaryReader(data);
                            if (e.MsgID == PacketTypes.Tile)
                            {
                                var type = reader.ReadByte();
                                if (!(type == 0 || type == 4))
                                    return;
                            }
                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            reader.Close();

                            var id = Terraria.Sign.ReadSign(x, y);

                            if (id != -1) //if have found sign
                            {
                            	var sign = SignManager.GetSign(id);
                            	if (sign.IsLocked()) //if locked stop removing
                            	{
									var tplayer = TShock.Players[e.Msg.whoAmI];
								
									if (tplayer.Group.HasPermission(Permissions.removesignprotection) ||
                                            tplayer.Group.HasPermission(Permissions.protectsign))
                                            //display more verbose info to player who has permission to remove protection on this chest
                                    {
                                    	tplayer.SendMessage(
                                                "This sign is protected. To remove it, first remove protection using \"/sunset\" command.",
                                                Color.Red);
                                    }
                                    else
                                    {
                                        tplayer.SendMessage("This sign is protected!", Color.Red);
                                    }

                                    tplayer.SendTileSquare(x, y);
                                 	e.Handled = true;
                                 }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}