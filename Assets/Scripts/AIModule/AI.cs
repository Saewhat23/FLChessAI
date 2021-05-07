using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Board;
using Movement;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace AIModule
{
    /**
     * AI
     *
     * Main implementation for the decisions behind the AI.
     * Utilizes recursive minimax algorithm for getting moves by maximizing gain while minimizing opponent's gain.
     */
    public class AI : MonoBehaviour
    {
        // Store values for different turn combinations
        private static List<List<ChessMove[]>> _turns;
        private static List<List<float>> _ratings;
        private static List<Dictionary<Square, AiChessPiece>> _boards = new List<Dictionary<Square, AiChessPiece>>();
        private static int _totalMovesSearched;
        private static long _start;
        private const int TurnSearchDepth = 2;   // how many turns to search
        private const int PossibleMoveDepth = 3; // how many possible moves to check before continuing (list is sorted by best rated)
        private const int NumThreads = 4;        // Number of threads to launch
        private const int Randomness = 0;       // Degree of randomness, random number between -val to +val is added to each rating
        
        private static void EvaluateAiMove(Dictionary<Square, AiChessPiece> board, float[,] strength, Player currentPlayer)
        {
            List<ChessMove> moves = GetMovesForPlayer(board, strength, currentPlayer);
            List<Task<float>> threads = new List<Task<float>>();
            for (int i = 0; i < NumThreads; i++)
            {
                Player threadedPlayer = new Player(currentPlayer);
                
                List<ChessMove> threadedMoves = new List<ChessMove>();
                int threadIndex = _turns.Count;

                // split up best moves
                for (int j = 0; j < moves.Count; j += NumThreads)
                {
                    if (j+threadIndex >= moves.Count) continue;
                    threadedMoves.Add(moves[j + threadIndex]);
                }

                // store new board for thread
                Dictionary<Square, AiChessPiece> threadedBoard = new Dictionary<Square, AiChessPiece>();
                foreach (Square s in board.Keys)
                {
                    AiChessPiece p = board[s];
                    threadedBoard.Add(s, new AiChessPiece(p.Name, p.Owner, p.Type, p.Position));
                }
                
                // init new strength for thread
                float[,] threadedStrength = EvaluationValues.InitStrength(threadedBoard);

                _turns.Add(new List<ChessMove[]>());
                _ratings.Add(new List<float>());
                _boards.Add(threadedBoard);

                Task<float> thread = new Task<float>(() => Minimax(threadedMoves, threadedBoard, threadedStrength, new Stack<ChessMove>(),
                    threadedPlayer, TurnSearchDepth, 3, threadIndex, 0));
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to join
            foreach (Task<float> t in threads)
            {
                t.Wait();
            }
        }

        public static void AiMoves(Dictionary<Square, AiChessPiece> board, Player currentPlayer)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            // Generate new environment for AI to simulate moves in
            float[,] strength = EvaluationValues.InitStrength(board);
            Game.AIMoves = null;
            Game.SubAIMoves = null;
            _totalMovesSearched = 0;
            _turns = new List<List<ChessMove[]>>();
            _ratings = new List<List<float>>();
            Player player = new Player(currentPlayer);
            
            // Run evaluation
            EvaluateAiMove(board, strength, currentPlayer);
            
            // All combinations are now stored in _turns, and their corresponding ratings are in _ratings
            // Find the best depending on player
            
            float best;
            int index = 0;

            // Flatten lists
            Random random = new Random();
            List<ChessMove[]> Moves = new List<ChessMove[]>();
            List<float> Ratings = new List<float>();
            for (var i = 0; i < _turns.Count; i++)
            {
                List<ChessMove[]> moveList = _turns[i];
                List<float> ratingList = _ratings[i];
                for (int j = 0; j < moveList.Count; j++)
                {
                    Moves.Add(moveList[j]);
                    Ratings.Add(ratingList[j]);
                    Ratings[Ratings.Count - 1] += random.Next(-Randomness, Randomness);
                }
            }

            // add the ratings of the turns to the total rating
            for (int i = 0; i < Moves.Count; i++)
            {
                ChessMove[] moves = Moves[i];
                Ratings[i] = 0;
                foreach (ChessMove move in moves)
                {
                    if (move == null) continue;
                    Ratings[i] += move.Rating;
                    Ratings[i] += (currentPlayer.Name == "player1" ? 10000 : -10000);
                } 
            }

            // find the min or max
            if (player.Name == "player1")
            {
                best = float.MinValue;
                for (int i = 0; i < Ratings.Count; i++)
                {
                    if (!(Ratings[i] > best)) continue;
                    best = Ratings[i];
                    index = i;
                }
            }
            else
            {
                best = float.MaxValue;
                for (int i = 0; i < Ratings.Count; i++)
                {
                    if (!(Ratings[i] < best)) continue;
                    best = Ratings[i];
                    index = i;
                }
            }
          
                // Set the ai's moves in Game
                Game.AIMoves = Moves[index];
                Game.nextMove = Moves[index][0];
            
           
            
            //Debug.Log(watch.ElapsedMilliseconds + " ms - " + _totalMovesSearched +
            //          " moves searched. Best moves yielded eval of: " + Ratings[index]);
        }
        
        // Recursively find best moves
        private static float Minimax(List<ChessMove> allMoves, Dictionary<Square, AiChessPiece> board, float[,] strength, Stack<ChessMove> moveStack, Player currentPlayer, int turnDepth, int moveDepth, int threadIndex, int turnIndex)
        {
            // Base case
            if (turnDepth == 0)
            {
                return EvaluationValues.BoardEvaluate(board, strength, false);
            }

            // If new turn, switch players
            if (moveDepth == 0)
            {
                Player opposite = currentPlayer.Name == "player1" ? Game.Controller.Player2 : Game.Controller.Player1;
                opposite.Commanders["M"].Moved = false;
                opposite.Commanders["R"].Moved = false;
                opposite.Commanders["L"].Moved = false;
                _ratings[threadIndex][turnIndex] += Minimax(GetMovesForPlayer(board, strength, opposite), board, strength, moveStack,
                    opposite, turnDepth - 1, 3, threadIndex, turnIndex);
            }
            else
            {
                int movesSearched = 0;
                // Iterate over possible moves
                for (int i = 0; i < allMoves.Count; i++)
                {
                    if (i >= allMoves.Count || movesSearched >= PossibleMoveDepth)
                        break;

                    ChessMove move = allMoves[i];
                    if (!board.ContainsKey(move.InitialSquare) || move.Attack && !board.ContainsKey(move.TargetSquare))
                        continue;
                    
                    int parentNodes = 3 - moveDepth;
                    
                    // Store results in _turns
                    if (turnDepth == TurnSearchDepth)
                    {
                        turnIndex = GetIndex(_turns[threadIndex], _ratings[threadIndex]);
                        for (int j = 0; j < parentNodes; j++)
                        {
                            _turns[threadIndex][turnIndex][j] = moveStack.Skip(parentNodes - j - 1).First();
                            _ratings[threadIndex][turnIndex] += _ratings[threadIndex][turnIndex - (j + 1)];
                        }
                    }
                    
                    movesSearched++;
                    _totalMovesSearched++;
                    if (turnDepth == TurnSearchDepth)
                        _turns[threadIndex][turnIndex][parentNodes] = move;
                    
                    // Save state of piece/board before making a move
                    AiChessPiece holder = null;
                    float[,] holderStrength = null;
                    bool[] commanderState = new bool[3];

                    // Make a move
                    MakeMove(board, ref strength, moveStack, move, ref holder, ref holderStrength, currentPlayer, ref commanderState);
                    
                    // Determine next best move
                    _ratings[threadIndex][turnIndex] += Minimax(GetMovesForPlayer(board, strength, currentPlayer), board, strength,
                        moveStack, currentPlayer, turnDepth, moveDepth - 1, threadIndex, turnIndex);
                    
                    // Unmake move
                    UnmakeMove(board, ref strength, moveStack, ref holder, ref holderStrength, currentPlayer, ref commanderState);
                }
            }

            return EvaluationValues.BoardEvaluate(board, strength, false);
        }

        // Get index for storing results in _turns and _ratings
        private static int GetIndex(List<ChessMove[]> chessMoves, List<float> floats)
        {
            chessMoves.Add(new ChessMove[3]);
            floats.Add(0);
            return chessMoves.Count - 1;
        }

        // Get all possible moves for a player depending on number of commanders
        private static List<ChessMove> GetMovesForPlayer(Dictionary<Square, AiChessPiece> board, float[,] strength, Player player)
        {
            List<ChessMove> allMoves = new List<ChessMove>();
            foreach (AiChessPiece piece in board.Values.Where(piece => piece.Owner.Name == player.Name))
            {
                if (player.GetCommander(piece.GetDivision()).Moved) continue;
                allMoves.AddRange(GetMovesForPiece(board, strength, piece));
            }

            SortMoves(allMoves, player);
            return allMoves;
        }
        
        // Get all possible moves for a piece on the board
        private static List<ChessMove> GetMovesForPiece(Dictionary<Square, AiChessPiece> board, float[,] strength, AiChessPiece piece)
        {
            List<Square> possibleSquares = AiMovementUtil.GetPossibleMoves(board, piece, false, null);
            List<ChessMove> possibleMoves = new List<ChessMove>();
            foreach (Square s in possibleSquares)
            {
                if (piece.Position.Equals(s))
                    continue;
                ChessMove move = new ChessMove(board, piece.Position, s, 
                    EvaluateMove.Instance.EvaluateChessMove(board, strength, piece.Position, s, piece.Owner.Name));
                move.AttackOnly = s.AttackOnly;
                possibleMoves.Add(move);
            }
            return possibleMoves;
        }
        
        // Logic for handling a move being made in AI evaluation
        private static void MakeMove(Dictionary<Square, AiChessPiece> board, ref float[,] strength, Stack<ChessMove> moves, ChessMove move, ref AiChessPiece holder, ref float[,] holderStrength, Player player, ref bool[] commanderState)
        {
            holderStrength = strength.Clone() as float[,];
            moves.Push(move);
        
            AiChessPiece piece = board[move.InitialSquare];
            // make the next move
            if (move.Attack)
            {
                holder = board[move.TargetSquare];
                board.Remove(move.TargetSquare);
                if (!move.AttackOnly)
                {
                    board.Remove(move.InitialSquare);
                    board.Add(move.TargetSquare, piece);
                    piece.Position = move.TargetSquare;
                }
            }
            else
            {
                board.Remove(move.InitialSquare);
                board.Add(move.TargetSquare, piece);
                piece.Position = move.TargetSquare;
            }

            player.Commanders[piece.GetDivision()].Moved = true;
            //player.RemainingMoves--;
            commanderState[0] = player.Commanders["M"].Moved;
            commanderState[1] = player.Commanders["R"].Moved;
            commanderState[2] = player.Commanders["L"].Moved;
        }
        
        // Logic for handling a move being unmade in AI evaluation
        private static void UnmakeMove(Dictionary<Square, AiChessPiece> board, ref float[,] strength, Stack<ChessMove> moves, ref AiChessPiece holder, ref float[,] holderStrength, Player player, ref bool[] commanderState)
        {
            strength = holderStrength.Clone() as float[,];
            ChessMove move = moves.Pop();
            AiChessPiece piece;
            if (!move.Attack)
            {
                piece = board[move.TargetSquare];
                board.Remove(move.TargetSquare);
                board.Add(move.InitialSquare, piece);
                piece.Position = move.InitialSquare;
            }
            else
            {
                if (!move.AttackOnly)
                {
                    piece = board[move.TargetSquare];
                    board.Remove(move.TargetSquare);
                    board.Add(move.InitialSquare, piece);
                    piece.Position = move.InitialSquare;
                }
                board.Add(move.TargetSquare, holder);
                holder.Position = move.TargetSquare;
            }

            //player.RemainingMoves++;
            player.Commanders["M"].Moved = commanderState[0];
            player.Commanders["R"].Moved = commanderState[1];
            player.Commanders["L"].Moved = commanderState[2];
        }
        
        // Sort list of moves based on the current player and the rating of the moves
        private static void SortMoves(List<ChessMove> chessMoves, Player player)
        {
            if (player.Name == "player1")
            {
                // sort descending
                chessMoves.Sort((y,x) => x.Rating.CompareTo(y.Rating));
            }
            else
            {
                // sort ascending
                chessMoves.Sort((x,y) => x.Rating.CompareTo(y.Rating));
            }
        }
    }
}