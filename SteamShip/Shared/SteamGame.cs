﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SteamShip.Shared
{
    public class SteamGame
    {
        public string name { get; set; }

        public int appid { get; set; }

        //in minutes
        public int playtime_forever { get; set; }

        public int hours_played => (int)(playtime_forever / 60);

    }

// Custom comparer for SteamGame object
public class SteamGameComparer : IEqualityComparer<SteamGame>, IComparer<SteamGame>
{
    // Games are equal if their app_id's are equal.
    public bool Equals(SteamGame x, SteamGame y)
    {

        if(x.appid == y.appid) {
            //add up playtime for averaging at the end. I know its hacky to have a compare method make changes to the object but I challenge you to do better
            y.playtime_forever += x.playtime_forever;
            return true;
        }

        return false;
    }

    // If Equals() returns true for a pair of objects
    // then GetHashCode() must return the same value for these objects.

    public int GetHashCode(SteamGame game)
    {
        if (Object.ReferenceEquals(game, null)) return 0;

        int hashGameId = game.appid.GetHashCode();

        int hashGameName = game.name.GetHashCode();

        return hashGameId ^ hashGameName;
    }

    public int Compare(SteamGame x, SteamGame y)
    {
        return -1 * x.playtime_forever.CompareTo(y.playtime_forever);
    }

}

}

