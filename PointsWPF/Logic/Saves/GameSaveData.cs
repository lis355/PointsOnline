﻿using GJson;
using System.Collections.Generic;

namespace PointsOnline
{
    class GameSaveData
    {
        public enum PlayerSide
        {
            Red,
            Blue
        }

        public class PlayerStats
        {
            [Name("Time")]
            public int TimeInSeconds;

            [Name("Score")]
            public int Score;

            [Name("ActivePoints")]
            public List<IntPoint> ActivePoints;

            [Name("CapturedPoints")]
            public List<IntPoint> CapturedPoints;

            [Name("Regions")]
            public List<Region> Regions;
        }

        [Name("RedPlayer")]
        public PlayerStats RedPlayer;

        [Name("BluePlayer")]
        public PlayerStats BluePlayer;

        [Name("IsGameStarted")]
        public bool IsGameStarted;

        [Name("CurrentMovingPlayer")]
        public PlayerSide CurrentMovingPlayer;
    }
}
