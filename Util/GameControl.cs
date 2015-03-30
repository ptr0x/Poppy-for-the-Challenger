using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFTC.Util
{
    class GameControl
    {
        public static Obj_AI_Hero MyHero = ObjectManager.Player;

        public static void LoadPlugin()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            var plugin = Type.GetType("PFTC.Plugin." + ObjectManager.Player.ChampionName);

            if (plugin == null)
            {
                Game.PrintChat(MiscControl.stringColor(ObjectManager.Player.ChampionName, MiscControl.TableColor.Red) + " not found. Loading OrbWalker");
                MiscControl.LoadOrbwalker();
                return;
            }
            else
            {
                Game.PrintChat(MiscControl.stringColor(ObjectManager.Player.ChampionName, MiscControl.TableColor.RoyalBlue) + " loaded, thanks for using PFTC.");
            }

            Activator.CreateInstance(plugin);
        }

        private static void CurrentDomainOnUnhandledException(object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Console.WriteLine(((Exception)unhandledExceptionEventArgs.ExceptionObject).Message);
            Game.PrintChat("Fatal Error please report on forum / Erro critico por favor avise no fórum");
        }

        public class EnemyInfo
        {
            public Obj_AI_Hero Player;
            public int LastSeen;
            public int LastPinged;

            public EnemyInfo(Obj_AI_Hero player)
            {
                Player = player;
            }
        }

    }
}
