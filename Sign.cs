using System;
using System.Text;
using System.Security.Cryptography;
using TShockAPI;

namespace SignControl
{
    class Sign
    {
        protected int ID;
        protected int WorldID;
        protected string HashedPassword;
        protected Vector2 Position;
        protected string Warp;

        public Sign()
        {
            ID = -1;
            WorldID = Terraria.Main.worldID;
            HashedPassword = "";
            Position = new Vector2(0, 0);
            Warp = "";
        }

        public void reset()
        {
            HashedPassword = "";
        }

        public void setID(int id)
        {
            ID = id;
        }

        public int getID()
        {
            return ID;
        }

        public void setPosition(Vector2 position)
        {
            Position = position;
        }

        public void setPosition(int x, int y)
        {
            Position = new Vector2(x, y);
        }

        public void setWarp(string warp)
        {
            Warp = warp;
        }

        public string getWarp()
        {
            return Warp;
        }

        public Vector2 getPosition()
        {
            return Position;
        }

        public bool isLocked()
        {
            return HashedPassword != "";
        }

        public bool isWarping()
        {
            return Warp != "";
        }

        public bool checkPassword(string password)
        {
            return HashedPassword.Equals(SHA1(password));
        }

        public void setPassword(string password)
        {
            if (password == "")
                HashedPassword = "";
            else
                HashedPassword = SHA1(password);
        }

        public void setPassword(string password, bool checkForHash)
        {
            if (checkForHash)
            {
                string pattern = @"^[0-9a-fA-F]{40}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(password, pattern)) //is SHA1 string
                    HashedPassword = password;
            }
            else
                setPassword(password);
        }

        public string getPassword()
        {
            return HashedPassword;
        }

        private static string SHA1(string input)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(input);
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }

        public static bool TileIsSign(Terraria.Tile tile)
        {
            return (tile.type == 0x37 || tile.type == 0x55);
        }

        public static bool TileIsSign(Vector2 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            return TileIsSign(x, y);
        }

        public static bool TileIsSign(int x, int y)
        {
            return TileIsSign(Terraria.Main.tile[x, y]);
        }
    }
}
