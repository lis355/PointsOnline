using System;
using System.Collections.Generic;

using PointsOnlineProject.Structure;

namespace PointsOnlineProject
{
    class Game
    {
        public GameInformation Info;
        GameMatrix Matrix;

        public Game(bool IsLimitedField,//ограниченное поле или нет
                    int BaseFieldSize,//размер поля
                    int BaseMatrixSize//размер поля
                    )
        {
            Info = new GameInformation();
            Info.IsLimited = IsLimitedField;
            Info.FieldDimension = BaseFieldSize;
            Info.MatrixDimension = BaseMatrixSize;

            Matrix = new GameMatrix(Info.MatrixDimension);
 

        }
    }
}
