using System;
using System.Collections.Generic;

namespace PointsOnlineProject
{
    class GamePoint
    {
        public int Color;
        public bool IsConnected; // если true то в ее 9-окрестности есть родственники
        public bool InRegion; // если true то в она находится внутри закрашенной области

        List<GamePoint> GetEnvironment()
        {
            List<GamePoint> S = new List<GamePoint>(8);



            return S;
        }
    }

}
