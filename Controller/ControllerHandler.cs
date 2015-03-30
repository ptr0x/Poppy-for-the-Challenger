﻿using LeagueSharp;
using PFTC.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFTC.Controller
{
    class ControllerHandler
    {
        /*
         Inicializa o MAC
         */
        public static void GameStart()
        {
            Events.Game.OnGameStart += OnGameStart;
            Game.PrintChat(MiscControl.stringColor("PFTC , Assembly exclusivo LoL-Pro.net", MiscControl.TableColor.Gold));
        }

        /*
          Escreve os créditos
         */
        private static void Draw_Credits(EventArgs args)
        {
        }

        /*
          Metodo disparado ao iniciar o Jogo
         */
        private static void OnGameStart(EventArgs args)
        {
            try
            {
                Drawing.OnDraw += Draw_Credits;
            }
            catch (Exception)
            {
                Game.PrintChat("Failed to load credits.");
            }
        }
    }
}
