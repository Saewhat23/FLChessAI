using System;
using System.Collections.Generic;
using Board;

namespace AIModule
{
    /**
     * EvaluationValues
     *
     * Determines how the AI views piece values, strength, position, and overall board evaluation.
     */
    public static class EvaluationValues
    {
        private const float StrengthModifier = .5f;
        private const float ValueModifier = 1;
        
        // Strength of each piece, based on capture matrix
        public static readonly Dictionary<string, int> PieceStrength = new Dictionary<string, int> 
            {{ "pawn", 1 }, { "knight", 3}, { "bishop", 2}, { "rook", 4}, { "queen", 5}, { "king", 6}};

        // Value of each piece, based on piece importance and strength
        public static readonly Dictionary<string, int> PieceValues = new Dictionary<string, int> 
            {{ "pawn", 20 }, { "knight", 40}, { "bishop", 200}, { "rook", 60}, { "queen", 100}, { "king", 1000}};

        // Generally, center of the board is a better place to be. Position is a very little weight on the AI's decision, but could break a tie between two moves.
        public static readonly float[,] PositionValues = {
            { -5, -5, -5,  -5,  -5, -5, -5, -5 },
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            {  9,  9,  9,  9,  9,  9,  9,  9 },
            {  9,  9,  9,  9,  9,  9,  9,  9 },
            {  9,  9,  9,  9,  9,  9,  9,  9 },
            {  9,  9,  9,  9,  9,  9,  9,  9 },
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { -5, -5, -5,  -5,  -5, -5, -5, -5 },
        };
        
        // Method to total the strength of the pieces on the board. On an even board, this will be zero.
        // Player 1 is positive, Player 2 is negative.
        private static float TotalStrength(float[,] strength, bool abs)
        {
            float sum = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (abs)
                        sum += Math.Abs(strength[x, y]);
                    else
                        sum += strength[x, y];
                }
            }
            return sum;
        }
        
        // Methods to do a full evaluation of the board
        public static float BoardEvaluate(Dictionary<Square, ChessPiece> board, float[,] strength, bool abs)
        {
            float sum = StrengthModifier * TotalStrength(strength, abs);
            float pieceSum = 0;
            foreach (ChessPiece piece in board.Values)
            {
                if (!abs)
                {
                    int direction = piece.Owner.Name == "player1" ? 1 : -1;
                    pieceSum += direction * PieceValues[piece.type];
                }
                else
                {
                    pieceSum += PieceValues[piece.type];
                }
            }
            pieceSum = ValueModifier * pieceSum;
            return sum + pieceSum;
        }
        public static float BoardEvaluate(Dictionary<Square, AiChessPiece> board, float[,] strength, bool abs)
        {
            float sum = StrengthModifier * TotalStrength(strength, abs);
            float pieceSum = 0;
            foreach (AiChessPiece piece in board.Values)
            {
                if (!abs)
                {
                    int direction = piece.Owner.Name == "player1" ? 1 : -1;
                    pieceSum += direction * PieceValues[piece.Type];
                }
                else
                {
                    pieceSum += PieceValues[piece.Type];
                }
            }
            pieceSum = ValueModifier * pieceSum;
            return sum + pieceSum;
        }
        
        // Methods to generate strength array from all pieces on the board
        public static float[,] InitStrength(Dictionary<Square, AiChessPiece> board)
        {
            float[,] strength = new float[8, 8];
            foreach (AiChessPiece piece in board.Values)
            {
                AiMovementUtil.GetPossibleMoves(board, piece, true, strength);
            }
            return strength;
        }
        public static float[,] InitStrength(Dictionary<Square, ChessPiece> board)
        {
            Dictionary<Square, AiChessPiece> newBoard = new Dictionary<Square, AiChessPiece>();
            foreach (Square s in board.Keys)
            {
                ChessPiece piece = board[s];
                newBoard.Add(s, new AiChessPiece(piece.name, piece.Owner, piece.type, piece.Position));
            }

            return InitStrength(newBoard);
        }
    }
}