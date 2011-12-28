using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TShockAPI;
using Terraria;

namespace SignControl
{
    internal class SignManager
    {
        private static readonly Sign[] Signs = new Sign[Terraria.Sign.maxSigns];
        private static readonly string ControlDirectory = Path.Combine(TShock.SavePath, "signcontrol");
        private static readonly string SavePath = Path.Combine(ControlDirectory, Main.worldID + ".txt");

        public static Sign GetSign(int id)
        {
            return Signs[id];
        }

        public static void Load()
        {
            if (!Directory.Exists(ControlDirectory))
                Directory.CreateDirectory(ControlDirectory);

            if (!File.Exists(SavePath))
                File.Create(SavePath).Close();

            for (var i = 0; i < Signs.Length; i++)
                Signs[i] = new Sign();

            var error = false;
            foreach (
                var args in File.ReadAllLines(SavePath).Select(line => line.Split('|')).Where(args => args.Length >= 4))
                try
                {
                    var sign = new Sign();

					sign.SetID(int.Parse(args[0]));
                    sign.SetPosition(new Vector2(int.Parse(args[1]), int.Parse(args[2])));
                    sign.SetPassword(args[3], args[3] != "");
                    
                    if (args.Length == 5)
                        sign.SetWarp(args[4]);

                    //check if sign still exists in world
					var id = Terraria.Sign.ReadSign((int) sign.GetPosition().X, (int) sign.GetPosition().Y);
                    if (id != 1)
					{	
						//the id of sign changed
						if(id != sign.GetID())
							sign.SetID(id);
						
						//add to array
						if (Signs.Length > sign.GetID())
							Signs[sign.GetID()] = sign;
					}
                }
                catch
                {
                    error = true;
                }

            if (error)
                Console.WriteLine("Failed to load some sign data, corresponding signs will be left unprotected.");
        }

        public static void Save()
        {
            File.WriteAllLines(SavePath, (from sign in Signs
                                          where sign != null
                                          where sign.IsLocked() || sign.IsWarping()
                                          select string.Format("{0}|{1}|{2}|{3}|{4}", sign.GetID(), sign.GetPosition().X, sign.GetPosition().Y, sign.GetPassword(), sign.GetWarp())).ToArray());
        }
    }
}