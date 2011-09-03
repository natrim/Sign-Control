using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using System;
using System.IO;
using System.Drawing;

namespace SignControl
{
    [APIVersion(1, 8)]
    public class SignControl : TerrariaPlugin
    {
        public static bool Init = false;
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
            get { return new Version(1, 0); }
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
            NetHooks.GetData += new NetHooks.GetDataD(NetHooks_GetData);
            ServerHooks.Leave += ServerHooks_Leave;
            GameHooks.Update += OnUpdate;
            WorldHooks.SaveWorld += OnSaveWorld;
        }

        public override void DeInitialize()
        {
            NetHooks.GetData -= NetHooks_GetData;
            ServerHooks.Leave -= ServerHooks_Leave;
            GameHooks.Update -= OnUpdate;
            WorldHooks.SaveWorld -= OnSaveWorld;
        }

        void OnSaveWorld(bool resettime, System.ComponentModel.HandledEventArgs e)
        {
            SignManager.Save(); //save signs
        }

        void OnUpdate()
        {
            if (!Init && Main.worldID > 0)
            {
                Console.WriteLine("Initiating Sign-Control...");
                SignManager.Load();
                Commands.Load();
                for (int i = 0; i < Players.Length; i++)
                {
                    Players[i] = new SPlayer(i);
                }
                Init = true;
            }
        }

        void ServerHooks_Leave(int obj)
        {
            Players[obj] = new SPlayer(obj);
        }

        void NetHooks_GetData(GetDataEventArgs e)
        {
            switch (e.MsgID)
            {
                case PacketTypes.SignRead: //on open sign set/unset protection
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                    {
                        BinaryReader reader = new BinaryReader(data);
                        var x = reader.ReadInt32();
                        var y = reader.ReadInt32();
                        reader.Close();

                        var id = Terraria.Sign.ReadSign(x, y);
                        var player = SignControl.Players[e.Msg.whoAmI];
                        var tplayer = TShock.Players[e.Msg.whoAmI];

                        if (id != -1)
                        {
                            var sign = SignManager.getSign(id);
                            bool messageSent = false;

                            switch (player.getState())
                            {
                                case SettingState.Setting:
                                    if (sign.isLocked())
                                    {
                                        player.SendMessage("This sign is already protected!", Color.Red);
                                        messageSent = true;
                                    }
                                    else
                                    {
                                        sign.setID(id);
                                        sign.setPosition(x, y);
                                        sign.setPassword(player.PasswordForSign);
                                        player.addSignAccess(id); //unlock this sign for him

                                        player.SendMessage("This sign is now protected.", Color.Red);
                                        messageSent = true;
                                    }

                                    player.setState(SettingState.None);
                                    break;
                                case SettingState.Deleting:
                                    if (sign.isLocked())
                                    {
                                        if (tplayer.Group.HasPermission("removesignprotection") || sign.checkPassword(player.PasswordForSign))
                                        {
                                            sign.reset();
                                            player.removeSignAccess(id); //remove access to this sign

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

                                    player.setState(SettingState.None);
                                    break;

                                case SettingState.UnLocking:
                                    if (sign.isLocked())
                                    {
                                        if (sign.checkPassword(player.PasswordForSign))
                                        {
                                            player.addSignAccess(id); //unlock this sign for him

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

                                    player.setState(SettingState.None);
                                    break;
                            }

                            if (!messageSent)
                            {
                                if (sign.isLocked())
                                {
                                    if (tplayer.Group.HasPermission("editallsigns") || player.canEditSign(id))
                                    {
                                        player.SendMessage("This sign is locked. You can edit it.", Color.Yellow);
                                    }
                                    else
                                    {
                                        player.SendMessage("This sign is locked. You can't edit it. You can unlock it using \"/sunlock PASSWORD\" command.", Color.Yellow);
                                    }
                                }
                                else
                                {
                                    player.SendMessage("This sign is not locked.", Color.Yellow);
                                }
                            }
                        }

                        if (player.getState() != SettingState.None) //if player is still setting something - end his setting
                            player.setState(SettingState.None);

                    }
                    break;

                case PacketTypes.SignNew: //editing sign
                    if (!e.Handled)
                    {
                        using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        {
                            BinaryReader reader = new BinaryReader(data);
                            short signId = reader.ReadInt16();
                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            reader.Close();

                            var id = Terraria.Sign.ReadSign(x, y);
                            var player = SignControl.Players[e.Msg.whoAmI];
                            var tplayer = TShock.Players[e.Msg.whoAmI];

                            if (id != -1)
                            {
                                var sign = SignManager.getSign(id);

                                if (sign.isLocked())
                                {
                                    if (!tplayer.Group.HasPermission("editallsigns") && !player.canEditSign(id)) //if player doesnt have permission, then break and message
                                    {
                                        e.Handled = true;
                                        tplayer.SendMessage("This sign is locked with password. Your changes would be not visible to other players.", Color.IndianRed);
                                        tplayer.SendMessage("( To edit this sign unlock it using \"/sunlock PASSWORD\" command. )", Color.IndianRed);
                                        tplayer.SendData(PacketTypes.SignNew, Terraria.Main.sign[id].text, id);
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
                        BinaryReader reader = new BinaryReader(data);
                        int x;
                        int y;
                        if (e.MsgID == PacketTypes.Tile)
                        {
                            var type = reader.ReadByte();
                            if (!(type == 0 || type == 4))
                                return;
                        }
                        x = reader.ReadInt32();
                        y = reader.ReadInt32();
                        reader.Close();

                        if (Sign.TileIsSign(x, y)) //if is sign OR tombstone
                        {
                            var id = Terraria.Sign.ReadSign(x, y);
                            var tplayer = TShock.Players[e.Msg.whoAmI];

                            if (id != -1) //if have found sign
                            {
                                var sign = SignManager.getSign(id);
                                if (sign.isLocked())//if locked stop removing
                                {
                                    if (tplayer.Group.HasPermission("removesignprotection") || tplayer.Group.HasPermission("protectsign")) //display more verbose info to player who has permission to remove protection on this chest
                                    {
                                        tplayer.SendMessage("This sign is protected. To remove it, first remove protection using \"/sunset\" command.", Color.Red);
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
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
