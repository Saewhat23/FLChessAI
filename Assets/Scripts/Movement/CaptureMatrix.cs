using System.Collections.Generic;

namespace Movement
{
    /**
     * CaptureMatrix
     *
     * Stores minimum die roll needed for an attacking piece vs a defending piece.
     */
    public static class CaptureMatrix
    {
        // A matrix modeled on the capture matrix provided
        private static readonly Dictionary<string, int> Matrix = new Dictionary<string, int>()
        {
            { "king,king", 4 },
            { "king,queen", 4 },
            { "king,knight", 4 },
            { "king,bishop", 4 },
            { "king,rook", 5 },
            { "king,pawn", 1 },

            { "queen,king", 4 },
            { "queen,queen", 4 },
            { "queen,knight", 4 },
            { "queen,bishop", 4 },
            { "queen,rook", 5 },
            { "queen,pawn", 2 },

            { "knight,king", 6 },
            { "knight,queen", 6 },
            { "knight,knight", 4 },
            { "knight,bishop", 4 },
            { "knight,rook", 5 },
            { "knight,pawn", 2 },

            { "bishop,king", 5 },
            { "bishop,queen", 5 },
            { "bishop,knight", 5 },
            { "bishop,bishop", 4 },
            { "bishop,rook", 5 },
            { "bishop,pawn", 3 },

            { "rook,king", 4 },
            { "rook,queen", 4 },
            { "rook,knight", 5 },
            { "rook,bishop", 5 },
            { "rook,rook", 6 },
            { "rook,pawn", 5 },

            { "pawn,king", 6 },
            { "pawn,queen", 6 },
            { "pawn,knight", 6 },
            { "pawn,bishop", 5 },
            { "pawn,rook", 6 },
            { "pawn,pawn", 4 }
        };

        // Get the value needed for an attack
        public static int GetMin(string attacker, string defender, bool addOne)
        {
            int min = Matrix[attacker + "," + defender];
            if (addOne) min++; // knight attacking from distance, add one to min roll
            return min;
        }
    }
}
