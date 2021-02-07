﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SteamShip.Shared
{
    public class SteamOwnedGames
    {
        public int game_count { get; set; }

        public SteamGame[] games { get; set; }
    }
}