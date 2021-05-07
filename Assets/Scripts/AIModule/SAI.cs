using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using AIModule;
using Movement;
using Board;

public class SAI : MonoBehaviour
{
    public static SAI instance;
    private const int INITIAL_DEPTH = 2;
    private const int TIMEOUT_MILISECONDS =3000;

    private static int currentDepth;
    private static ChessMove besteWhiteMove;
    private static ChessMove bestBlackMove;
    private static ChessMove bestMove;
    private static ChessMove globalBestMove;
    private static long start;
    private static bool timeout;

    public bool[] hasMoved = new bool[3];

    public void Awake()
    {
        instance = this;
    }
    public static ChessMove DecideAIBestMove()
    {
        timeout = false;
        start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        for (int d = 0; ; d++)
        {
            if (d > 0)
            {
                globalBestMove = bestMove;
                // Debug.Log("Completed search with depth " + currentDepth + ". Best move so far: " + globalBestMove  );
            }
            currentDepth = INITIAL_DEPTH + d;
            maximizer(currentDepth, int.MinValue, int.MaxValue);

            if (timeout)
            {
                Console.WriteLine();
                return globalBestMove;
            }
        }
    }

    //AI ==> Maximizer, Alpha player 
    private static int maximizer(int depth, int alpha, int beta)
    {
        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start > TIMEOUT_MILISECONDS)
        {
            timeout = true;
            return alpha;
        }

        if (depth == 0)
        {
            //------------------Human Player ---------------------///
            //Calculate over all rating
            //Evaluate KingSafety 
            //Evaluate Position 
            //Activity of Pieces 
            //.....

            //------------------------------------------------------//
            return calculateRating("player1");
        }

        List<ChessMove> possibleMoves = GetAllPosibleMoves();
        foreach (ChessMove move in possibleMoves)
        {

        
           makeMove(move);
           // Game.Controller.SetCurrentPlayer(Game.Controller.GetEnemyPlayer());
            int rating = minimizer(depth - 1, alpha, beta);
            //Game.Controller.SetCurrentPlayer(Game.Controller.GetEnemyPlayer());
           undoMove(move);

            if (rating > alpha)
            {
                alpha = rating;

                if (depth == currentDepth)
                {
                    bestMove = move;

                }
            }

            if (alpha >= beta)
            {
                return alpha;
            }

        }
        return alpha;
    }


    //Human ==> Minimizer, Beta player
    private static int minimizer(int depth, int alpha, int beta)
    {

        if (depth == 0)
        {
            //------------------Human Player ---------------------///
            //Evaluate KingSafety 
            //Evaluate Position 
            //Activity of Pieces 
            //......
            //------------------------------------------------------//
            return calculateRating("player2");
        }



        List<ChessMove> possibleMoves = GetAllPosibleMoves();



        //Go throu all possible moves 
        foreach (ChessMove move in possibleMoves)
        {


            //if(!ChessGrid.Pieces.ContainsKey(move.InitialSquare))
            //{
            //    //p = ChessGrid.GetPosition(move.InitialSquare);
            //    continue;
            //}
            //AI pretends to be the enemy player and make the best move for the enemy player,  to respond to it
            makeMove(move);
             //Game.Controller.SetCurrentPlayer(Game.Controller.GetEnemyPlayer());
            int rating = maximizer(depth - 1, alpha, beta);
            //Game.Controller.SetCurrentPlayer(Game.Controller.GetEnemyPlayer());


            //Undo the move that was excuted 
           undoMove(move);

            if (rating <= beta)
            {
                beta = rating;
            }

            if (alpha >= beta)
            {
                return beta;
            }


        }
        return beta;
    }

    //Get All poossible moves for all the pieces on the baord 
    public static List<ChessMove> GetAllPosibleMoves()
    {
        //var peices = new List<ChessPiece>(ChessGrid.Pieces.Values);
        List<ChessMove> possibleMoves = new List<ChessMove>();

        // List<List<Square>> sq = new List<List<Square>>();
        foreach (ChessPiece p in ChessGrid.Pieces.Values)
        {
            if (p != null)
            {

                foreach (Square ss in MovementUtil.GetPossibleMoves(ChessGrid.Pieces, p))
                {
                    //if(p.moveable)
                    possibleMoves.Add(new ChessMove(p.Position, ss));
                }
            }
        }
        return possibleMoves;
    }

    //Get all PosibbleMoves for the player with the white Pieces 
    public static List<ChessMove> GetAllPosibleMovesForWhite()
    {
        //var peices = new List<ChessPiece>(ChessGrid.Pieces.Values);
        List<ChessMove> possibleMoves = new List<ChessMove>();

        // List<List<Square>> sq = new List<List<Square>>();
        foreach (ChessPiece p in ChessGrid.Pieces.Values)
        {
            if (p != null)
            {


                foreach (Square ss in MovementUtil.GetPossibleMoves(ChessGrid.Pieces, p))
                {
                    if (p.Owner.Name=="player1")
                    {
                      //  if(p.moveable)
                        possibleMoves.Add(new ChessMove(p.Position, ss));
                    }

                }
            }
        }
        return possibleMoves;
    }

    //Get All Possible Moves for the player with the black peices 
    public static List<ChessMove> GetAllPosibleMovesForBlack()
    {
        Commander com = null;

        List<ChessMove> possibleMoves = new List<ChessMove>();


        foreach (ChessPiece p in ChessGrid.Pieces.Values)
        {
            if (p != null)
            {
               
               
                List<Square> moves = MovementUtil.GetPossibleMoves(ChessGrid.Pieces, p);

                foreach (Square ss in moves)
                {
                    if (p.Owner.Name == "player2" )
                    {
                       // if(p.moveable)
                        possibleMoves.Add(new ChessMove(p.Position, ss));
                    }
                       

                }
            }
        }
        return possibleMoves;
    }


    // make a move on the baord without showing it on the (UI)board 
    //AI pretends to be the enemy player and make the best move for the enemy player,  to respond to it

    public static void makeMove(ChessMove move)
    { 
        if(ChessGrid.Pieces.ContainsKey(move.TargetSquare))
        {
            ChessGrid.SetPositionEmpty(move.TargetSquare);
        }
        ChessPiece movingPiece = ChessGrid.GetPosition(move.InitialSquare);
        movingPiece.Position= move.TargetSquare;
        ChessGrid.Pieces[move.TargetSquare] = movingPiece;
        ChessGrid.Pieces[move.InitialSquare] = null;

        //Square initialSquare = move.InitialSquare;
        //Square targetSquare = move.TargetSquare;
        //ChessPiece targetPiece = null;
        //if (ChessGrid.GetPosition(move.TargetSquare) != null)
        //{
        //     targetPiece = ChessGrid.GetPosition(move.TargetSquare);
        //}
        //ChessPiece movingPiece = ChessGrid.GetPosition(targetSquare);

        //movingPiece.Position = initialSquare;
        //initialSquare.
        

        //ChessPiece current = ChessGrid.GetPosition(move.InitialSquare);
        //Square holdSquare = move.TargetSquare;



        //if (current != null)
        //{
        //    ChessGrid.SetPositionEmpty(move.InitialSquare);
        //    ChessGrid.SetPosition(move.InitialSquare, current);

        //}
        //else
        //{
        //    ChessGrid.SetPositionEmpty(holdSquare);
        //}

    }


    public static void undoMove(ChessMove move)
    {


        Square initialSquare = move.InitialSquare;
        Square targetSquare = move.TargetSquare;

        ChessPiece targetPiece = null;
        targetPiece = ChessGrid.GetPosition(move.TargetSquare);
        
        ChessPiece movingPiece = ChessGrid.GetPosition(move.InitialSquare);
        movingPiece.Position = initialSquare;
        ChessGrid.Pieces[initialSquare] = movingPiece;
        ChessGrid.Pieces[targetSquare] = null;

        if (targetPiece != null)
        {
            targetPiece.Position = targetSquare;
            ChessGrid.Pieces[targetSquare] = targetPiece;
            ChessGrid.Pieces.Add(targetSquare,targetPiece);
            // int[] pos = current.getMatrixPos();
            //ChessGrid.SetPositionEmpty(current.Position);
            //ChessGrid.SetPosition(targetSquare, targetPiece);
        }
        //else
        //{
        //    if (ChessGrid.Pieces.ContainsKey(move.TargetSquare))
        //    {
        //        ChessGrid.SetPositionEmpty(targetPiece.Position);
        //    }
        //}
    }

    public static int calculateRating(string owner)
    {
        int whiteScore = 1;
        int blackScore = 0;

        foreach (ChessPiece p in ChessGrid.Pieces.Values)
        {
            if (p != null && p.Owner.Name == "player1")
            {
                whiteScore += AIModule.EvaluationValues.PieceValues[p.type] ;
                // Debug.Log("WhiteScore: " + whiteScore);
            }
            else if (p != null && p.Owner.Name == "player2")
            {
                blackScore += AIModule.EvaluationValues.PieceValues[p.type];
                
            }
            else
            {
                Debug.Log("ComputerRating failed ");
            }
        }

        return owner == "black" ? blackScore - whiteScore : whiteScore - blackScore;


    }
}
