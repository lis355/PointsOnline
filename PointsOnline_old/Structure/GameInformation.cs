using System;
using System.Collections.Generic;

namespace PointsOnlineProject.Structure
{
    static class GameState
    {
        public const int
            WaitingProgress = 1;         
    }

    class GameInformation
    {
        public bool IsLimited;
        public int FieldDimension;
        public int MatrixDimension;
        public int Condition;
    }
}
