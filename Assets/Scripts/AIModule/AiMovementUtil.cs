using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Movement;
using UnityEngine;

namespace AIModule
{
    /**
     * AiMovementUtil
     *
     * A wrapper around MovementUtil that allows for the AI to make better decisions.
     * For explanations on movement, see MovementUtil.cs
     */
    public class AiMovementUtil : MonoBehaviour
    {
        public static List<Square> GetPossibleMoves(Dictionary<Square, AiChessPiece> board, AiChessPiece piece, bool updateStrength, float[,] strength)
        {
            string owner = piece.Owner.Name;
            int direction = owner == "player1" ? 1 : -1;
            List<Square> possibleMoves = null;
            switch (piece.Type)
            {
                case "pawn": case "bishop": {
                    possibleMoves = InfantryMoves(piece.Position, direction, updateStrength, strength);
                    break;
                }
                case "king": case "queen": {
                    possibleMoves = RoyaltyMoves(board, piece.Position, 3, owner, updateStrength, strength);
                    break;
                }
                case "knight": {
                    possibleMoves = RoyaltyMoves(board, piece.Position, 4, owner, updateStrength, strength);
                    break;
                }
                case "rook": {
                    possibleMoves = ArcherMoves(board, piece.Position, owner, updateStrength, strength);
                    break;
                }
            }

            List<Square> cleaned = new List<Square>();
            List<Square> surrounding = MovementUtil.SurroundingSquares(piece.Position);
            foreach (Square s in possibleMoves.Where(s => ChessGrid.ValidPosition(s)))
            {
                if (board.ContainsKey(s))
                {
                    if (piece.Type != "knight" && !s.AttackOnly && !surrounding.Contains(s))
                    {
                        // remove this if statement if we want pieces to be able to attack any squares they can reach
                        continue;
                    }
                    AiChessPiece target = board[s];
                    if (target.Owner == piece.Owner)
                    {
                        continue;
                    }
                }
                cleaned.Add(s);
            }
            return cleaned;
        }
    
        // Forward direction only
        private static List<Square> InfantryMoves(Square start, int direction, bool updateStrength, float[,] strength)
        {
            List<Square> moves = new List<Square>
            {
                new Square(start.X - 0, start.Y + direction),
                new Square(start.X - 1, start.Y + direction),
                new Square(start.X + 1, start.Y + direction)
            };

            if (!updateStrength) return moves;
            foreach (Square s in moves.Where(ChessGrid.ValidPosition))
            {
                strength[s.X, s.Y] += direction * EvaluationValues.PieceStrength["pawn"];
            }

            return moves;
        }

        // Movement for the Rook and its Attack range
        private static List<Square> ArcherMoves(Dictionary<Square, AiChessPiece> board, Square start, string owner, bool updateStrength, float[,] strength)
        {
            int direction = owner == "player1" ? 1 : -1;
            List<Square> moves = MovementUtil.SurroundingSquares(start);
            foreach (Square s in moves)
            {
                if (!board.ContainsKey(s)) continue;
                AiChessPiece target = board[s];
                if (target.Owner.Name != owner)
                    s.AttackOnly = true;
            }

            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    if (i == 0 && j == 0) continue;
                    Square s = new Square(start.X + i, start.Y + j);
                    if (updateStrength && ChessGrid.ValidPosition(s))
                        strength[s.X, s.Y] += direction * EvaluationValues.PieceStrength["rook"];
                    if (!board.ContainsKey(s)) continue;
                    AiChessPiece target = board[s];
                    if (target.Owner.Name != owner)
                        moves.Add(new Square(s.X, s.Y, true));
                }
            }
            return moves;
        }

        //Movement for the Knights, King, and Queen
        private static List<Square> RoyaltyMoves(Dictionary<Square, AiChessPiece> board, Square start, int depth, string owner, bool updateStrength, float[,] strength)
        {
            List<Square> moves = new List<Square>();
            for (int i = -depth; i <= depth; i++)
            {
                for (int j = -depth; j <= depth; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        moves.Add(new Square(start.X + i, start.Y + j));
                    }
                }
            }

            moves.Sort((a, b) => -(Math.Abs(a.X) + Math.Abs(a.Y)).CompareTo(Math.Abs(b.X) + Math.Abs(b.Y)));
            HashSet<Square> valid = new HashSet<Square>();
            HashSet<Square> invalid = new HashSet<Square>();

            // iterate over squares in the radius and make sure it is possible to move there (empty square or enemy piece)
            // cache these in the valid hashset - O(1) access to check if a square is valid
            // this cache allows us to check the path from start to finish until we hit an invalid square (invalid) or the final square (valid)
            // the cache is necessary to avoid iterating over every surrounding square n times resulting in O(8^n) - instead it is O(n) recursion
            foreach (Square s in moves)
            {
                if (ValidateSquare(board, valid, invalid, s, owner))
                    valid.Add(s);
                else
                    invalid.Add(s);
            }

            // validate that each target has a valid path - if it does, add it to validPaths list. This is what determines the final moves
            List<Square> validPaths = new List<Square>();
            int direction = owner == "player1" ? 1 : -1;
            foreach (Square s in valid.Where(s => ValidatePath(board, valid, start, s, 0, depth, updateStrength, strength)))
            {
                if (updateStrength)
                    strength[s.X, s.Y] += direction * (depth == 3 ? EvaluationValues.PieceStrength["queen"] : EvaluationValues.PieceStrength["knight"]);
                validPaths.Add(s);
            }

            if (!updateStrength) return validPaths;
            List<AiChessPiece> surroundings = SurroundingPieces(board, start);
            foreach (Square s in surroundings.Select(piece => piece.Position))
            {
                strength[s.X, s.Y] += direction * (depth == 3 ? EvaluationValues.PieceStrength["queen"] : EvaluationValues.PieceStrength["knight"]);
            }
            return validPaths;
        }

        private static bool ValidateSquare(Dictionary<Square, AiChessPiece> board, ISet<Square> validMoves, ISet<Square> invalidMoves, Square current, string owner)
        {
            if (!ChessGrid.ValidPosition(current)) return false;
            if (board.ContainsKey(current))
            {
                if (board[current].Owner.Name == owner)
                {
                    invalidMoves.Add(current);
                    return false;
                }
                validMoves.Add(current);
                return true;
            }
            validMoves.Add(current);
            return true;
        }

        public static List<AiChessPiece> SurroundingPieces(Dictionary<Square, AiChessPiece> board, Square start)
        {
            List<AiChessPiece> list = new List<AiChessPiece>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    Square s = new Square(start.X + i, start.Y + j);
                    if (board.ContainsKey(s))
                        list.Add(board[s]);
                }
            }
            return list;
        }

        private static bool ValidatePath(Dictionary<Square, AiChessPiece> board, HashSet<Square> validMoves, Square current, Square final, int index, int depth, bool updateStrength, float[,] strength)
        {
            // NOTE: putting a debug statement in here will lag it a lot and crash unity, especially for knight movement
            if (Equals(current, final)) return true;

            // if not the last, but we run into a piece on the way
            if (index >= depth || (index > 0 && board.ContainsKey(current)))
                return false;

            if (index > 0 && !validMoves.Contains(current))
                return false;

            // get all surrounding squares
            List<Square> surroundings = MovementUtil.SurroundingSquares(current);
            int start = surroundings.IndexOf(new Square(current.X + Math.Sign(final.X - current.X), current.Y + Math.Sign(final.Y - current.Y)));

            // iterate over all of the surrounding squares. if one of them succeeds, the path is valid
            for (int i = 0; i < 5; i++)
            {
                if (i < 4)
                {
                    if (ValidatePath(board, validMoves, surroundings[MovementUtil.WrapAround(start - i, surroundings.Count)], final, index + 1, depth, updateStrength, strength))
                        return true;
                }
                if (ValidatePath(board, validMoves, surroundings[MovementUtil.WrapAround(start + i, surroundings.Count)], final, index + 1, depth, updateStrength, strength))
                    return true;
            }
       
            // if every path failed, target is unreachable
            return false;
        }
    }
}
