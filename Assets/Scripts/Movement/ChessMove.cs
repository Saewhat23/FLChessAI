using System.Collections.Generic;
using AIModule;
using Board;

namespace Movement
{
    /**
     * ChessMove
     *
     * Represents a move from one square to another.
     */
    public class ChessMove
    {
        public readonly Square InitialSquare;   // Square that holds the piece that is moving
        public readonly Square TargetSquare;    // Square that the piece is moving to
        public readonly float Rating;           // Rating of the move - determined in EvaluateMove.cs
        public readonly bool Attack;            // If the move is an attack on another piece
        public bool AttackOnly;                 // If the move is a ranged attack, or an attack that does not move the initial piece
        public readonly bool AddOne;            // If the move is done by a knight from a distance - subtract 1 from die result - or add 1 to the minimum value required

        // AI constructor
        public ChessMove(Dictionary<Square, AiChessPiece> board, Square initialSquare, Square targetSquare, float rating)
        {
            InitialSquare = initialSquare;
            TargetSquare = targetSquare;
            if (board.ContainsKey(targetSquare))
                Attack = (board[targetSquare] != null);
            Rating = rating;
        
            if (!Attack) return;
            AiChessPiece piece = board[initialSquare];
            if (piece.Type == "knight")
            {
                AiChessPiece target = board[targetSquare];
                List<AiChessPiece> surroundings = AiMovementUtil.SurroundingPieces(board, initialSquare);
                if (!surroundings.Contains(target))
                {
                    AddOne = true;
                }
            }
        }
    
        // Player constructor
        public ChessMove(Square initialSquare, Square targetSquare, float rating)
        {
            InitialSquare = initialSquare;
            TargetSquare = targetSquare;
            if (ChessGrid.Pieces.ContainsKey(targetSquare))
                Attack = (ChessGrid.GetPosition(targetSquare) != null);
            Rating = rating;
        
            if (!Attack) return;
            ChessPiece piece = ChessGrid.GetPosition(initialSquare);
        
            if (piece.type == "knight")
            {
                ChessPiece target = ChessGrid.GetPosition(targetSquare);
                List<ChessPiece> surroundings = MovementUtil.SurroundingPieces(ChessGrid.Pieces, initialSquare);
                if (!surroundings.Contains(target))
                {
                    AddOne = true;
                }
            }
        }

        public ChessMove(Square initialSquare, Square targetSquare)
        {
            InitialSquare = initialSquare;
            TargetSquare = targetSquare;
            if (ChessGrid.Pieces.ContainsKey(targetSquare))
                Attack = (ChessGrid.GetPosition(targetSquare) != null);
            //Rating = rating;

            if (!Attack) return;
            ChessPiece piece = ChessGrid.GetPosition(initialSquare);

            if (piece.type == "knight")
            {
                ChessPiece target = ChessGrid.GetPosition(targetSquare);
                List<ChessPiece> surroundings = MovementUtil.SurroundingPieces(ChessGrid.Pieces, initialSquare);
                if (!surroundings.Contains(target))
                {
                    AddOne = true;
                }
            }
        }


    }
}
