using System;
using System.Text;
using System.Security.Cryptography;
using TShockAPI;
using System.Drawing;

namespace SignControl
{
    class Sign
    {
        protected int ID;
        protected int WorldID;
        protected string HashedPassword;
        protected PointF Position;

        public Sign()
        {
            ID = -1;
            WorldID = Terraria.Main.worldID;
            HashedPassword = "";
            Position = new PointF(0, 0);
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

        public void setPosition(PointF position)
        {
            Position = position;
        }

        public void setPosition(int x, int y)
        {
            Position = new PointF(x, y);
        }

        public PointF getPosition()
        {
            return Position;
        }

        public bool isLocked()
        {
            return HashedPassword != "" ? true : false;
        }

        public bool checkPassword(string password)
        {
            if (HashedPassword.Equals(SHA1(password)))
            {
                return true;
            }

            return false;
        }

        public void setPassword(string password)
        {
            if (password == "")
            {
                HashedPassword = "";
            }
            else
            {
                HashedPassword = SHA1(password);
            }
        }

        public void setPassword(string password, bool checkForHash)
        {
            if (checkForHash)
            {
                string pattern = @"^[0-9a-fA-F]{40}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(password, pattern)) //is SHA1 string
                {
                    HashedPassword = password;
                }
            }
            else
            {
                setPassword(password);
            }
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
            if (tile.type == 0x37 || tile.type == 0x55)
            {
                return true;
            }

            return false;
        }

        public static bool TileIsSign(PointF position)
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
