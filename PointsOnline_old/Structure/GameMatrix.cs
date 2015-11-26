using System;
using System.Collections.Generic;
using System.Text;

namespace PointsOnlineProject
{
    class GameMatrix
    {
        GameField[,] Fields;

        public GameMatrix(int dim = 10)
        {
            Fields = new GameField[dim, dim];
        }
    }
}
