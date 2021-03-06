﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using Hooks;
using TShockAPI;
using Terraria;

namespace SignControl
{
    [APIVersion(1, 11)]
    public class SignControl : TerrariaPlugin
    {
        private static string updateUrl = "https://raw.github.com/natrim/Sign-Control/master/release.txt";
        public static DateTime updateLastcheck = DateTime.MinValue;
        private static readonly int updateCheckXMinutes = 120;

        public static SPlayer[] Players;

        public SignControl(Main game)
            : base(game)
        {
        }

        public override string Name
        {
            get { object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false); return attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override string Author
        {
            get { object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false); return attributes.Length == 0 ? "" : ((AssemblyCompanyAttribute)attributes[0]).Company; }
        }

        public override string Description
        {
            get { object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false); return attributes.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attributes[0]).Description; }
        }

        public override void Initialize()
        {
            NetHooks.GetData += NetHooks_GetData;
            ServerHooks.Leave += ServerHooks_Leave;
            GameHooks.Initialize += OnInitialize;
            GameHooks.PostInitialize += OnPostInitialize;
            GameHooks.Update += OnUpdate;
            WorldHooks.SaveWorld += OnSaveWorld;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NetHooks.GetData -= NetHooks_GetData;
                ServerHooks.Leave -= ServerHooks_Leave;
                GameHooks.Initialize -= OnInitialize;
                GameHooks.PostInitialize -= OnPostInitialize;
                GameHooks.Update -= OnUpdate;
                WorldHooks.SaveWorld -= OnSaveWorld;
            }

            base.Dispose(disposing);
        }

        private void OnUpdate()
        {
            if ((DateTime.Now - updateLastcheck).TotalMinutes >= updateCheckXMinutes)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(UpdateChecker));
                updateLastcheck = DateTime.Now;
            }
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

        private void OnInitialize()
        {
            Permissions.Load();
            Commands.Load();
        }

        private void OnPostInitialize()
        {
            //TODO: full support for reloadplugins command ( DeInitialize and better Dispose )
            SignManager.Load();

            //TODO: lazy loading
            Players = new SPlayer[Main.maxNetPlayers];
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new SPlayer();
            }

            Console.WriteLine(string.Format(Messages.loading, Name, Version, Author, Description));
        }

        private void ServerHooks_Leave(int obj)
        {
            Players[obj] = new SPlayer();
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
                            SPlayer splayer = Players[e.Msg.whoAmI];
                            TSPlayer tplayer = TShock.Players[e.Msg.whoAmI];

                            if (id != -1)
                            {
                                Sign sign = SignManager.GetSign(id);
                                bool messageSent = false;

                                switch (splayer.GetState())
                                {
                                    case SettingState.Setting:
                                        if (sign.IsLocked())
                                        {
                                            tplayer.SendMessage(Messages.alreadyProtected, Color.Red);
                                            messageSent = true;
                                        }
                                        else
                                        {
                                            sign.SetID(id);
                                            sign.SetPosition(x, y);
                                            sign.SetPassword(splayer.PasswordForSign);
                                            splayer.AddSignAccess(id); //unlock this sign for him

                                            tplayer.SendMessage(Messages.nowProtected, Color.Red);
                                            messageSent = true;
                                        }

                                        splayer.SetState(SettingState.None);
                                        break;
                                    case SettingState.Deleting:
                                        if (sign.IsLocked())
                                        {
                                            if (tplayer.Group.HasPermission(Permissions.removesignprotection) ||
                                                sign.CheckPassword(splayer.PasswordForSign))
                                            {
                                                sign.SetPassword("");
                                                SPlayer.RemoveSignAccessFromAll(id); //remove access to this sign

                                                tplayer.SendMessage(Messages.removed, Color.Red);
                                                messageSent = true;
                                            }
                                            else
                                            {
                                                tplayer.SendMessage(Messages.wrongPassword, Color.Red);
                                                messageSent = true;
                                            }
                                        }
                                        else
                                        {
                                            tplayer.SendMessage(Messages.notProtected, Color.Red);
                                            messageSent = true;
                                        }

                                        splayer.SetState(SettingState.None);
                                        break;
                                    case SettingState.UnLocking:
                                        if (sign.IsLocked())
                                        {
                                            if (sign.CheckPassword(splayer.PasswordForSign))
                                            {
                                                splayer.AddSignAccess(id); //unlock this sign for him

                                                tplayer.SendMessage(Messages.unlocked, Color.Red);
                                                messageSent = true;
                                            }
                                            else
                                            {
                                                tplayer.SendMessage(Messages.wrongPassword, Color.Red);
                                                messageSent = true;
                                            }
                                        }
                                        else
                                        {
                                            tplayer.SendMessage(Messages.notProtected, Color.Red);
                                            messageSent = true;
                                        }

                                        splayer.SetState(SettingState.None);
                                        break;
                                    case SettingState.WarpSetting:
                                        if ((sign.IsLocked() &&
                                             (tplayer.Group.HasPermission(Permissions.editallsigns) || splayer.CanEditSign(id)))
                                            || !sign.IsLocked())
                                        {
                                            sign.SetWarp(splayer.WarpForSign);
                                            tplayer.SendMessage(string.Format(Messages.warpTo, splayer.WarpForSign), Color.Plum);
                                            messageSent = true;
                                        }
                                        else
                                        {
                                            tplayer.SendMessage(Messages.isProtected, Color.Red);
                                            messageSent = true;
                                        }

                                        splayer.SetState(SettingState.None);
                                        break;
                                    case SettingState.DeletingWarp:
                                        if (sign.IsWarping())
                                        {
                                            if ((sign.IsLocked() &&
                                                 (tplayer.Group.HasPermission(Permissions.editallsigns) || splayer.CanEditSign(id)))
                                                || !sign.IsLocked())
                                            {
                                                sign.SetWarp("");
                                                tplayer.SendMessage(Messages.notWarping, Color.Red);
                                                messageSent = true;
                                            }
                                            else
                                            {
                                                tplayer.SendMessage(Messages.noEdit, Color.Red);
                                                messageSent = true;
                                            }
                                        }
                                        else
                                        {
                                            tplayer.SendMessage(Messages.noWarp, Color.Red);
                                            messageSent = true;
                                        }

                                        splayer.SetState(SettingState.None);
                                        break;
                                }

                                if (!messageSent)
                                {
                                    if (sign.IsLocked())
                                    {
                                        if (tplayer.Group.HasPermission(Permissions.editallsigns) || splayer.CanEditSign(id))
                                        {
                                            tplayer.SendMessage(Messages.editable, Color.YellowGreen);
                                        }
                                        else
                                        {
                                            tplayer.SendMessage(Messages.notEditable, Color.Yellow);
                                            if (tplayer.Group.HasPermission(Permissions.canunlocksign))
                                            {
                                                tplayer.SendMessage(Messages.password, Color.Yellow);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        tplayer.SendMessage(Messages.notProtected, Color.Yellow);
                                    }
                                }

                                if (sign.IsWarping())
                                {
                                    var warp = TShock.Warps.FindWarp(sign.GetWarp());
                                    if (warp.WarpName != null)
                                    {
                                        tplayer.Teleport((int)warp.WarpPos.X, (int)warp.WarpPos.Y);
                                        tplayer.SendMessage(string.Format(Messages.teleported, warp.WarpName), Color.Blue);
                                    }
                                    else
                                    {
                                        tplayer.SendMessage(Messages.wrongWarp, Color.Red);
                                    }
                                }
                            }

                            if (splayer.GetState() != SettingState.None)
                                //if player is still setting something - end his setting
                                splayer.SetState(SettingState.None);
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
                                var splayer = Players[e.Msg.whoAmI];
                                var tplayer = TShock.Players[e.Msg.whoAmI];

                                if (id != -1)
                                {
                                    //FIXME: get sign by x and y to prevent that bug
                                    var sign = SignManager.GetSign(id);

                                    if (sign.IsLocked())
                                    {
                                        if (!tplayer.Group.HasPermission(Permissions.editallsigns) && !splayer.CanEditSign(id))
                                        //if player doesnt have permission, then break and message
                                        {
                                            tplayer.SendMessage(Messages.stopEdit, Color.IndianRed);
                                            if (tplayer.Group.HasPermission(Permissions.canunlocksign))
                                            {
                                                tplayer.SendMessage(Messages.password, Color.IndianRed);
                                            }

                                            e.Handled = true;
                                            tplayer.SendData(PacketTypes.SignNew, "", id);
                                            return;
                                        }
                                    }
                                }
                                else 
                                { 
                                    //there is no sign so stop editing - little fixy .)
                                    tplayer.SendMessage(Messages.wrongtile, Color.IndianRed);
                                    tplayer.SendData(PacketTypes.SignNew, "", id);
                                    e.Handled = true;
                                    return;
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
                                //FIXME: get sign by x and y
                                var sign = SignManager.GetSign(id);

                                if (sign.IsLocked()) //if locked stop removing
                                {
                                    var tplayer = TShock.Players[e.Msg.whoAmI];

                                    tplayer.SendMessage(Messages.isProtected, Color.Red);

                                    //display more verbose info to player who has permission to remove protection on this chest
                                    if (tplayer.Group.HasPermission(Permissions.removesignprotection) ||
                                            tplayer.Group.HasPermission(Permissions.protectsign))
                                    {
                                        tplayer.SendMessage(Messages.removeSign, Color.Red);
                                    }

                                    //and stop
                                    tplayer.SendTileSquare(x, y);
                                    e.Handled = true;
                                    return;
                                }


                                //reset sign to remove all ponys in it cause the sign will get removed and we dont want that another sign get protected if placed in same place
                                sign.Reset();
                            }

                            //TODO: protect the 2 tiles on which is locked sign placed to prevent auto remove (checking x+-1 and y+-1 of tile for sign)
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected void UpdateChecker(Object stateInfo = null)
        {
            string raw;
            try
            {
                raw = new System.Net.WebClient().DownloadString(updateUrl);
            }
            catch (Exception)
            {
                return;
            }
            var list = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Version version;
            if (!Version.TryParse(list[0], out version)) return;
            if (Version.CompareTo(version) >= 0) return;

            Console.WriteLine(string.Format(Messages.version, Name, version));

            foreach (TSPlayer tplayer in TShock.Players)
            {
                if (tplayer != null && tplayer.Active && tplayer.Group.HasPermission(TShockAPI.Permissions.maintenance))
                {
                    tplayer.SendMessage(string.Format(Messages.version, Name, version), Color.Yellow);
                    if (list.Length > 1)
                        for (var i = 1; i < list.Length; i++)
                            tplayer.SendMessage(list[i], Color.Yellow);
                }
            }
        }
    }
}