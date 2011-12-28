using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;

namespace SignControl
{
    internal class Sign
    {
        protected string HashedPassword;
        protected int ID;
        protected Vector2 Position;
        protected string Warp;
        protected int WorldID;

        public Sign()
        {
            ID = -1;
            WorldID = Main.worldID;
            HashedPassword = "";
            Position = new Vector2(0, 0);
            Warp = "";
        }

        public void Reset()
        {
            HashedPassword = "";
        }

        public void SetID(int id)
        {
            ID = id;
        }

        public int GetID()
        {
            return ID;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void SetPosition(int x, int y)
        {
            Position = new Vector2(x, y);
        }

        public void SetWarp(string warp)
        {
            Warp = warp;
        }

        public string GetWarp()
        {
            return Warp;
        }

        public Vector2 GetPosition()
        {
            return Position;
        }

        public bool IsLocked()
        {
            return HashedPassword != "";
        }

        public bool IsWarping()
        {
            return Warp != "";
        }

        public bool CheckPassword(string password)
        {
            return HashedPassword.Equals(SHA1(password));
        }

        public void SetPassword(string password)
        {
            HashedPassword = password == "" ? "" : SHA1(password);
        }

        public void SetPassword(string password, bool checkForHash)
        {
            if (checkForHash)
            {
                const string pattern = @"^[0-9a-fA-F]{40}$";
                if (password != null && Regex.IsMatch(password, pattern)) //is SHA1 string
                    HashedPassword = password;
            }
            else
                SetPassword(password);
        }

        public string GetPassword()
        {
            return HashedPassword;
        }

        private static string SHA1(string input)
        {
            var buffer = Encoding.ASCII.GetBytes(input);
            using (var cryptoTransformSHA1 = new SHA1CryptoServiceProvider())
                return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
        }
    }
}