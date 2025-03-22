using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class Constants
    {
        public static readonly int Margin = 10;
        public static readonly int Square_Size = 50;

        public static readonly Position[] Directions =
        {
            new Position { row = -1, column= 0 },
            new Position { row = 1, column =0 },
            new Position { row = 0, column = -1 },
            new Position { row = 0, column = 1 },
            new Position { row = -1, column = -1 },
            new Position { row = -1, column = 1 },
            new Position { row = 1, column = -1 },
            new Position { row = 1, column = 1 }
        };
        public static readonly int[,][] SquaresToSide;
        static Constants()
        {
            SquaresToSide = new int[8, 8][];
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    int numTop = 7 - row;
                    int numBottom = row;
                    int numLeft = column;
                    int numRight = 7 - column;

                    SquaresToSide[row, column] = new int[]
                    {
                        numTop,
                        numBottom,
                        numLeft,
                        numRight,
                        Math.Min(numTop, numLeft),
                        Math.Min(numBottom, numRight),
                        Math.Min(numTop, numRight),
                        Math.Min(numBottom, numLeft)
                    };
                }
            }
        }
    }
}
