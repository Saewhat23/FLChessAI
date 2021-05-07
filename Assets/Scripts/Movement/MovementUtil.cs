using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Board;
using UnityEngine;

namespace Movement
{
    /**
     * MovementUtil
     *
     * Defines which pieces can move where.
     * Includes implementation for performing a move on the board.
     */
    public class MovementUtil : MonoBehaviour
    {
        // direction of player : player1 = 1, player2 = -1
        private static int _direction;

        // Graveyard objects
        public Graveyard player1Graveyard;
        public Graveyard player2Graveyard;

        public bool[] hasMoved = new bool[3];
    
        public static MovementUtil Instance;

        private void Awake()
        {
            Instance = this;
        }

        // Move a piece on the board
        public IEnumerator MovePiece(ChessMove chessMove, bool attackOnly)
        {
            ChessPiece initialPiece = ChessGrid.GetPosition(chessMove.InitialSquare);
            string notation = UIManager.BuildNotation(initialPiece.type, chessMove.InitialSquare, chessMove.TargetSquare, chessMove.Attack);
            Color color = Color.white;

            // Assume it will succeed if not an attack
            bool success = true;
            if (chessMove.Attack)
            {
                // Update attacking text
                ChessPiece targetPiece = ChessGrid.GetPosition(chessMove.TargetSquare);
                int minNeeded = CaptureMatrix.GetMin(initialPiece.type, targetPiece.type, chessMove.AddOne);
                Game.Controller.uiManager.UpdateAttack(UIManager.BuildNotation(initialPiece.type, 
                    chessMove.InitialSquare, chessMove.TargetSquare, true) + " - " + minNeeded + "+ needed.");
            
                // Only do the die roll if it is not a king attacking a pawn (this move is automatic)
                if (!(initialPiece.type == "king" && targetPiece.type == "pawn"))
                {
                    yield return Game.Controller.die.Roll();
                    int num = Game.Controller.die.GetResult();
                    success = num >= minNeeded;
                }
            
                // Set color of move history text
                color = success ? Color.green : Color.red;
            }

            // set commander as having moved
            initialPiece.GetCommander().SetMoved();
        
            // Add move history record
            Game.Controller.uiManager.InsertMoveRecord(notation, false, color);
        
            if (success)
            {
                if (chessMove.Attack)
                {
                    // Play sounds
                    SoundManger.PlayAttackSuccessSound();
                    yield return new WaitForSeconds(0.7f);
                    SoundManger.PlayCaptureSound();

                    ChessPiece targetPiece = ChessGrid.GetPosition(chessMove.TargetSquare);
                    // if king was taken, a player has won the game
                    if (targetPiece.type == "king")
                        Game.Winner(targetPiece.Owner.Name == "player1" ? "player2" : "player1");
                    targetPiece.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;   // Disable commander icon on this piece

                    // transition command authority to king if bishop was captured
                    if (targetPiece.type == "bishop")
                    {
                        Player player = targetPiece.Owner;
                        player.RemainingMoves--;
                        if (targetPiece.Owner == PovManager.Instance.Pov)
                            CommandDelegation.Instance.DisableButton(targetPiece.GetDivision());
                        player.TransitionCommand(targetPiece.GetDivision());
                    }
                
                    // move piece to graveyard
                    Graveyard grave = targetPiece.Owner.Name == "player1" ? player1Graveyard : player2Graveyard;
                    targetPiece.moveable = false;
                    targetPiece.dragging = true;
                    ChessGrid.Captured.Add(targetPiece);
                    grave.AddToGrave(targetPiece.gameObject.transform);
                }

                // If piece moves to the square it attacked (not an archer)
                if (!attackOnly)
                {
                    SoundManger.PlayMoveSound();
                    ChessGrid.SetPositionEmpty(initialPiece.Position);
                    ChessGrid.SetPosition(chessMove.TargetSquare, initialPiece);
                    initialPiece.SpriteRenderer.sortingOrder = 1;
                    initialPiece.Position = chessMove.TargetSquare;
                }
                else
                {
                    ChessGrid.SetPositionEmpty(chessMove.TargetSquare);
                }
            }
            else
            { // Attack Failed
                Game.Controller.lastMoveFailed = true;
                SoundManger.PlayAtackFailedSound();
            }

            // Increment turn regardless of fail/success
            DestroyMovePlates();
            Game.Controller.IncrementTurnCounter();
        }
    
        // Get all possible moves for a player based on the current state of the board
        public static List<Square> GetPossibleMoves(Dictionary<Square, ChessPiece> board, ChessPiece piece)
        {
            // first check if piece's division has already moved - if so, return an empty list
            if (piece.GetCommander().Moved)
            {
                piece.moveable = false;
                return new List<Square>();
            }
        
            _direction = piece.Owner.Name == "player1" ? 1 : -1;
            List<Square> possibleMoves = null;
        
            // Find moves based on type of piece
            switch (piece.type)
            {
                case "pawn": case "bishop": {
                    possibleMoves = InfantryMoves(piece.Position, _direction);
                    break;
                }
                case "king": case "queen": {
                    possibleMoves = RoyaltyMoves(board, piece.Position, 3, piece.Owner.Name);
                    break;
                }
                case "knight": {
                    possibleMoves = RoyaltyMoves(board, piece.Position, 4, piece.Owner.Name);
                    break;
                }
                case "rook": {
                    possibleMoves = ArcherMoves(board, piece.Position, piece.Owner.Name);
                    break;
                }
            }

            // Clean the squares based on if the positions are valid on the board + other rules
            List<Square> cleaned = new List<Square>();
            List<Square> surrounding = SurroundingSquares(piece.Position);

            foreach (Square s in possibleMoves.Where(s => ChessGrid.ValidPosition(s)))
            {
                if (board.ContainsKey(s))
                {
                    if (piece.type != "knight" && !s.AttackOnly && !surrounding.Contains(s))
                    {
                        // remove this if statement if we want pieces to be able to attack any squares they can reach
                        continue;
                    }
                    ChessPiece target = board[s];
                    if (target.Owner == piece.Owner)
                    {
                        continue;
                    }
                }
                cleaned.Add(s);
            }

            piece.moveable = cleaned.Count > 0;
            return cleaned;
        }

        // Bishop and pawn attacks - forward direction only
        private static List<Square> InfantryMoves(Square start, int direction)
        {
            List<Square> moves = new List<Square>
            {
                new Square(start.X - 0, start.Y + direction),
                new Square(start.X - 1, start.Y + direction),
                new Square(start.X + 1, start.Y + direction)
            };
            return moves;
        }

        // Movement for the Rook and its Attack range
        private static List<Square> ArcherMoves(Dictionary<Square, ChessPiece> board, Square start, string owner)
        {
            // Add squares directly next to it able to move there
            List<Square> moves = SurroundingSquares(start);
            foreach (Square s in from s in moves where board.ContainsKey(s) let target = board[s] where target.Owner.Name != owner select s)
            {
                s.AttackOnly = true;
            }

            // Find squares in the archer attack range with an enemy piece in them
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    if (i == 0 && j == 0) continue;
                    Square s = new Square(start.X + i, start.Y + j);
                    if (!board.ContainsKey(s)) continue;
                    ChessPiece target = board[s];
                    if (target.Owner.Name != owner)
                        moves.Add(new Square(s.X, s.Y, true));
                }
            }
            return moves;
        }

        //Movement for the Knights, King, and Queen
        private static List<Square> RoyaltyMoves(Dictionary<Square, ChessPiece> board, Square start, int depth, string owner)
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

            // Iterate over squares in the radius and make sure it is possible to move there (empty square or enemy piece)
            // Cache these in the valid hashset - O(1) access to check if a square is valid
            // This cache allows us to check the path from start to finish until we hit an invalid square (invalid) or the final square (valid)
            // The cache is necessary to avoid iterating over every surrounding square n times resulting in O(8^n) - instead it is O(n) recursion
            foreach (Square s in moves)
            {
                if (ValidateSquare(board, valid, invalid, s, owner))
                    valid.Add(s);
                else
                    invalid.Add(s);
            }

            // Validate that each target has a valid path - if it does, add it to validPaths list. This is what determines the final moves
            return valid.Where(s => ValidatePath(board, valid, start, s, 0, depth)).ToList();
        }

        // Validate that a square can be moved to, and then add it to the corresponding cache list
        private static bool ValidateSquare(Dictionary<Square, ChessPiece> board, ISet<Square> validMoves, ISet<Square> invalidMoves, Square current, string owner)
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

        /*
         * Unwrap surrounding squares into a list
         * ex: a b c
         *     h   d    ->    a b c d e f g h
         *     g f e
         *     
         * This allows us to get the two closest squares of a square using index -1/+1, ie if the target is directly above, then:
         * b will be checked first
         * a, c are checked next
         * h, d are checked next
         * g, e are checked next
         * f is checked last - most cases will not reach the last check
         */
        public static List<Square> SurroundingSquares(Square start)
        {
            List<Square> surroundings = new List<Square>
            {
                new Square(start.X - 1, start.Y + 1),
                new Square(start.X, start.Y + 1),
                new Square(start.X + 1, start.Y + 1),
                new Square(start.X + 1, start.Y),
                new Square(start.X + 1, start.Y - 1),
                new Square(start.X, start.Y - 1),
                new Square(start.X - 1, start.Y - 1),
                new Square(start.X - 1, start.Y)
            };
            return surroundings;
        }

        // Get the pieces that exist in the eight squares around a piece
        public static List<ChessPiece> SurroundingPieces(Dictionary<Square, ChessPiece> board, Square start)
        {
            List<ChessPiece> list = new List<ChessPiece>();
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

        // Validate that a path exists to a square for a royal piece.
        // Solves the logic of "n squares in any direction, does not have to move in a straight line"
        private static bool ValidatePath(Dictionary<Square, ChessPiece> board, HashSet<Square> validMoves, Square current, Square final, int index, int depth)
        {
            // NOTE: putting a debug statement in here will lag it a lot and crash unity, especially for knight movement
            if (Equals(current, final)) return true;

            // If not the last, but we run into a piece on the way
            if (index >= depth || (index > 0 && board.ContainsKey(current)))
                return false;

            if (index > 0 && !validMoves.Contains(current))
                return false;

            // get all surrounding squares
            List<Square> surroundings = SurroundingSquares(current);
            int start = surroundings.IndexOf(new Square(current.X + Math.Sign(final.X - current.X), current.Y + Math.Sign(final.Y - current.Y)));

            // Iterate over all of the surrounding squares. if one of them succeeds, the path is valid
            for (int i = 0; i < 5; i++)
            {
                if (i < 4)
                {
                    if (ValidatePath(board, validMoves, surroundings[WrapAround(start - i, surroundings.Count)], final, index + 1, depth))
                        return true;
                }
                if (ValidatePath(board, validMoves, surroundings[WrapAround(start + i, surroundings.Count)], final, index + 1, depth))
                    return true;
            }
       
            // If every path failed, target is unreachable
            return false;
        }

        // Wrap index value around the length of a list. Used to wraparound the surrounding squares list.
        public static int WrapAround(int i, int l)
        {
            if (i >= l)
                return Math.Abs(l - i);
            if (i < 0)
                return i + l;
            return i;
        }

        //Destroys all Move plate objects
        public static void DestroyMovePlates()
        {
            Transform movePlates = Game.Controller.movePlateParent.transform;
            for (int i = 0; i < movePlates.childCount; i++)
            {
                Destroy(movePlates.GetChild(i).gameObject);
            }
        }
    }
}
