using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Movement;
using UnityEngine;
using Random = System.Random;

namespace AIModule
{
    /**
     * EvaluateMove
     *
     * Determine the rating of a ChessMove based on multiple factors.
     */
    public class EvaluateMove : MonoBehaviour
    {
        public static EvaluateMove Instance;

        private void Awake()
        {
            Instance = this;
        }

        /* ------------------------------------- CRITERIA FOR AI DECISIONS ------------------------------------- */
        [Header("AI Decision Weights")]
        [Tooltip("How the AI weighs decisions involving position of the target square on the board.")]
        public float weightEvalPosition = 1.0f;
        [Tooltip("How the AI weighs decisions involving the number of enemy attackers/defenders near a target square.")]
        public float weightEvalStrength = 1.0f;
        [Tooltip("How the AI weighs decisions involving the number of open squares directly next to the target.")]
        public float weightEvalOpen = 1.0f;
        [Tooltip("How the AI weighs decisions involving the number of open squares directly next to the target.")]
        public float weightEvalBlocked = 1.0f;
        [Tooltip("How the AI weighs decisions involving the priority of nearby enemy pieces.")]
        public float weightEvalPriority = 1.0f;
        [Tooltip("How the AI weighs decisions involving the chance to win against a nearby enemy piece.")]
        public float weightEvalWinChance = 1.0f;
        [Tooltip("How the AI weighs decisions involving safety of the current piece.")]
        public float weightEvalSafety = 1.0f;
        [Tooltip("How the AI weighs decisions involving safety of the king.")]
        public float weightEvalKingSafety = 1.0f;

        public float EvaluateChessMove(Dictionary<Square, AiChessPiece> board, float[,] strength,  Square initialSquare, Square targetSquare, string owner)
        {
            AiChessPiece initialPiece = board[initialSquare];
            List<AiChessPiece> initialSurroundings = AiMovementUtil.SurroundingPieces(board, initialSquare);
            List<AiChessPiece> targetSurroundings = AiMovementUtil.SurroundingPieces(board, targetSquare);
            float initial = EvaluateChessSquare(strength, initialSquare, owner, initialPiece.Type, initialSurroundings);
            float target = EvaluateChessSquare(strength, targetSquare, owner, initialPiece.Type, targetSurroundings);
            var score = target - initial; // take into account if the target square is better than initial square
            if (targetSurroundings.Any(p => p.Type == "king" && p.Owner.Name != owner))
            {
                score += 30;
            }
            if (board.ContainsKey(targetSquare))
            {
                score += 20; // be more aggressive
                AiChessPiece targetPiece = board[targetSquare];
                score += weightEvalPriority * EvaluatePiecePriority(targetPiece);
                score += weightEvalWinChance * EvaluateWinChance(board, strength, initialPiece, targetPiece);
            }
            score += weightEvalKingSafety * EvaluateKingSafety(board, initialSquare, targetSquare, owner);
            return score * (owner == "player1" ? 1 : -1);
        }
    
        // Assigns a score to each valid move
        private float EvaluateChessSquare(float[,] strength, Square square, string owner, string type, List<AiChessPiece> surroundings)
        {
            float score = 0f;
            score += weightEvalPosition * EvaluateBoardPosition(square);
            score += weightEvalStrength * EvaluateStrength(strength, square, owner);
            score += weightEvalOpen * EvaluateOpenSquares(surroundings);
            score += weightEvalBlocked * EvaluateBlockedPieces(owner, surroundings);
            score += weightEvalSafety * EvaluatePieceSafety(square, owner, type, strength);
            return score;
        }
    
        /*
         * Generally, center of the board is better. It's not good for pieces to be stuck on the top and bottom rank.
         */
        private static float EvaluateBoardPosition(Square target)
        {
            return EvaluationValues.PositionValues[target.X, target.Y];
        }
    
        /*
         * Check how many open squares are around a piece. Does not matter too much, but shows how a piece can move freely.
         */
        private static float EvaluateOpenSquares(List<AiChessPiece> pieces)
        {
            return 8 - pieces.Count;
        }
    
        /*
         * Check how many pieces are blocked by this piece. Prevents AI from grouping pieces together without purpose.
         */
        private static float EvaluateBlockedPieces(string owner, List<AiChessPiece> pieces)
        {
            float blockedSquares = pieces.Where(piece => piece.Owner.Name == owner).Sum(piece => 1f);
            return -blockedSquares;
        }
    
        /*
         * Check if this move puts the king in a less secure position.
         * Returns 0 if does not involve king, 1000 if move protects king, and -1000 if move leaves open spot next to king.
         */
        private static float EvaluateKingSafety(Dictionary<Square, AiChessPiece> board, Square initial, Square target, string owner)
        {
            AiChessPiece initialPiece = board[initial];
            if (initialPiece.Type == "king" && initialPiece.Owner.Name == owner)
            {
                return 0;
            }
        
            List<AiChessPiece> initialSurroundings = AiMovementUtil.SurroundingPieces(board, initial);
            List<AiChessPiece> targetSurroundings = AiMovementUtil.SurroundingPieces(board, target);

            float score = 0;
            if (initialSurroundings.Any(piece => piece.Type == "king" && piece.Owner.Name == owner))
            {
                score -= 1000;
            }
            if (targetSurroundings.Any(piece => piece.Type == "king" && piece.Owner.Name == owner))
            {
                score += 1000;
            }

            return score;
        }
    
        /*
         * Evaluate the number of attackers and defenders on a square.
         */
        private static float EvaluateStrength(float[,] strength,  Square initial, string owner)
        {
            return strength[initial.X, initial.Y] * (owner == "player1" ? 1 : -1);
        }
    
        /**
         * Evaluate the priority of an enemy piece.
         */
        private static float EvaluatePiecePriority(AiChessPiece target)
        {
            return EvaluationValues.PieceValues[target.Type];
        }
    
        /**
         * Evaluate the win change against an enemy piece.
         */
        private static float EvaluateWinChance(Dictionary<Square, AiChessPiece> board, float[,] strength, AiChessPiece initial, AiChessPiece target)
        {
            List<AiChessPiece> surroundings = AiMovementUtil.SurroundingPieces(board, initial.Position);
            bool addOne = false;
            switch (initial.Type)
            {
                case "knight":
                {
                    if (!surroundings.Contains(target))
                    {
                        addOne = true;
                    }

                    break;
                }
                case "rook":
                    return 10 * EvaluationValues.PieceValues[target.Type];
            }
        
            float minRoll = CaptureMatrix.GetMin(initial.Type, target.Type, addOne);
            if (minRoll > 6) return -10000;
            return minRoll;
        }
    
        /**
         * Evaluate how aggressive the piece should be in attacks. If target is valued higher, be more aggressive. Otherwise,
         * depend on strength values to make move.
         */
        private static float EvaluatePieceSafety(Square square, string owner, string type, float[,] strength)
        {
            int direction = owner == "player1" ? 1 : -1;
            float str = direction * strength[square.X, square.Y];
            if (type == "rook")
            {
                str = Math.Abs(str);
            }
            float priority = EvaluationValues.PieceValues[type];
            if (type == "king")
                return -priority;
            return priority / 2 + str;
        }

        
    }
}
