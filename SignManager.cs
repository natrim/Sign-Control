using Terraria;
using TShockAPI;
using System.Collections.Generic;
using System.IO;

namespace SignControl
{
    class SignManager
    {

        private static Sign[] Signs = new Sign[Terraria.Sign.maxSigns];
        private static string ControlDirectory = Path.Combine(TShock.SavePath, "signcontrol");
        private static string SavePath = Path.Combine(ControlDirectory, Main.worldID + ".txt");

        public static Sign getSign(int id)
        {
            return Signs[id];
        }

        public static void Load()
        {
            if (!Directory.Exists(ControlDirectory))
            {
                Directory.CreateDirectory(ControlDirectory);
            }

            if (!File.Exists(SavePath))
            {
                File.Create(SavePath).Close();
            }

            for (int i = 0; i < Signs.Length; i++)
                Signs[i] = new Sign();

            var error = false;
            foreach (var line in File.ReadAllLines(SavePath))
            {
                var args = line.Split('|');
                if (args.Length < 4)
                {
                    continue;
                }
                try
                {
                    var sign = Signs[int.Parse(args[0])];

                    sign.setPosition(new Vector2(int.Parse(args[1]), int.Parse(args[2])));
                    sign.setPassword(args[3], true);
                    sign.setID(int.Parse(args[0]));

                    //check if sign still exists in world
                    if (!Sign.TileIsSign(sign.getPosition()))
                    {
                        //sign dont exists - so reset it
                        sign.reset();
                    }
                }
                catch
                {
                    error = true;
                }
            }

            if (error)
            {
                System.Console.WriteLine("Failed to load some sign data, corresponding signs will be left unprotected.");
            }
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (var sign in Signs)
            {
                if (Sign.TileIsSign(sign.getPosition())) // save only if sign exists
                {
                    if (sign.isLocked()) //save only protected signs
                    {
                        lines.Add(string.Format("{0}|{1}|{2}|{3}", sign.getID(), sign.getPosition().X, sign.getPosition().Y, sign.getPassword()));
                    }
                }
            }
            File.WriteAllLines(SavePath, lines.ToArray());
        }

    }
}
