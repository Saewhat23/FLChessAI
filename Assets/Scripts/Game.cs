using System.Collections.Generic;
using System.Threading.Tasks;
using AIModule;
using Board;
using Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Game
 *
 * Main game controller. Referenced by many objects for information on the state of the game.
 * For example, finding the current player.
 */
public class Game : MonoBehaviour
{
    public static Game Controller;
    
    // Movement objects
    public GameObject movePlate;
    public GameObject movePlateParent;
    public UIManager uiManager;
    public RollDie die;

    // Game information
    public int turnCounter = 0;
    public int totalTurns;
    
    // Player information
    public Player Player1, Player2;
    private Player _currentPlayer;
    public Transform player1Parent, player2Parent;
    public CommandDelegation commandDelegation;
    
    // Used for AI moves
    public float counter = 1.0f;

    public float subCounter = 1.0f;
    public bool lastMoveFailed;
    public bool movingPiece;
    public static ChessMove[] AIMoves;
    public static ChessMove nextMove;

    public bool subMovingPiece;
    public static bool subLastMoveFailed;
    public static ChessMove[] SubAIMoves;
    public static ChessMove subAInextMove;
    
    private void Awake()
    {
        Controller = this;
    }

    // Initialize the game
    private void Start()
    { 
        ChessGrid.Pieces.Clear();
        Player1 = new Player("player1", false);
        Player2 = new Player("player2", false);
        //Player1.isSubAI = true;
        _currentPlayer = Player1;

        // Initialize pov and opponent
        Player pov = StartScene.Player1 ? Player1 : Player2;
        Player opp = pov.Name == "player1" ? Player2 : Player1;
        opp.IsAI = StartScene.VsAi;
        PovManager.Instance.Init(pov, opp);
        ChessGrid.Instance.Init();  // Initialize board
        
        totalTurns = 1;
        uiManager.InsertMoveRecord(totalTurns + ". Player 1", true, Color.white);
        RequestAI();
        Player2.Disable();
    }

    // Runs every frame
    private void Update()
    {
        if (AIMoves != null && _currentPlayer.IsAI && !movingPiece)
        {
            // wait for countdown for timer in between AI's moves
            if (counter > 0)
            {
                counter -= Time.deltaTime;
            }
            else
            { // make the next move for AI
                if (nextMove == null)
                {
                    uiManager.InsertMoveRecord("Skipped", false, Color.white);
                    IncrementTurnCounter();
                }
                else
                {
                    movingPiece = true;
                    StartCoroutine(MovementUtil.Instance.MovePiece(nextMove, nextMove.AttackOnly));
                    counter = 1; // reset counter
                }
            }
        }

    }
    
    //Swaps the current turn of the game
    public void IncrementTurnCounter()
    {
        movingPiece = false;
        subMovingPiece = false;
        ModifyEvalBar();

        // player reaches move limit
        if (turnCounter >= _currentPlayer.RemainingMoves)
        {
            // change for current player before switching
            MovementUtil.DestroyMovePlates();
            _currentPlayer.Disable();
            
            AIMoves = null;
            SubAIMoves = null;

            _currentPlayer = _currentPlayer == Player1 ? Player2 : Player1;
            turnCounter = 0;
            totalTurns++;
            uiManager.InsertMoveRecord(totalTurns + ". " + (_currentPlayer.Name == "player1" ? "Player 1" : "Player 2"), true, Color.white);
            Text moveText = _currentPlayer == Player1 ? uiManager.whiteMoves : uiManager.blackMoves;
            moveText.text = "1/" + (_currentPlayer.RemainingMoves + 1);

            //reset all bools used to keep track of squadron selection
            _currentPlayer.Reset(); 
            commandDelegation.Reset();
            commandDelegation.gameObject.SetActive(false);
        }
        // increment turn counter and set ui
        else
        {
            turnCounter++;
            Text moveText = _currentPlayer == Player1 ? uiManager.whiteMoves : uiManager.blackMoves;
            moveText.text = (turnCounter + 1) + "/" + (_currentPlayer.RemainingMoves + 1);
        }
      //  RequestSubAI();
        RequestAI();
       
    }
    
    // Request that the AI make a move
    private void RequestAI()
    {
        if (!_currentPlayer.IsAI) return;   // if not the Ai's turn, do nothing
        if (lastMoveFailed || turnCounter == 0)
        {
            lastMoveFailed = false;
            // convert board to thread accessible version
            Dictionary<Square, AiChessPiece> board = new Dictionary<Square, AiChessPiece>();
            foreach (Square s in ChessGrid.Pieces.Keys)
            {
                ChessPiece p = ChessGrid.Pieces[s];
                board.Add(s, new AiChessPiece(p.name, p.Owner, p.type, p.Position));
            }

            AIMoves = null;
            nextMove = null;
            
            // start in thread so AI calculation doesn't freeze game
            Task thread = new Task(() => AI.AiMoves(board, _currentPlayer));
            thread.Start();
        }
        else
        {
            
            nextMove = AIMoves[turnCounter];
            movingPiece = false;
        }
    }

    //private void RequestSubAI()
    //{
    //    if (!_currentPlayer.isSubAI) return;   // if not the SubAi's turn, do nothing
    //    if (subLastMoveFailed || turnCounter == 0)
    //    {
    //        subLastMoveFailed = false;
    //        convert board to thread accessible version
    //        Dictionary<Square, AiChessPiece> board = new Dictionary<Square, AiChessPiece>();
    //        foreach (Square s in ChessGrid.Pieces.Keys)
    //        {
    //            ChessPiece p = ChessGrid.Pieces[s];
    //            board.Add(s, new AiChessPiece(p.name, p.Owner, p.type, p.Position));
    //        }

    //        SubAIMoves = null;
    //        subAInextMove = null;

    //        start in thread so AI calculation doesn't freeze game
    //        Task thread = new Task(() => AI.AiMoves(board, _currentPlayer));
    //        thread.Start();
    //    }
    //    else
    //    {
    //        subAInextMove = SubAIMoves[turnCounter];
    //        subMovingPiece = false;
    //    }
    //}

    private void ModifyEvalBar()
    {
        // Get total strength of board
        float[,] strength = EvaluationValues.InitStrength(ChessGrid.Pieces);
        float totalStrength = EvaluationValues.BoardEvaluate(ChessGrid.Pieces, strength, true) - 2 * EvaluationValues.PieceValues["king"]; // subtract kings from equation
        
        // Get the net strength of board (player 2 is negative, player 1 is positive, sum the strength)
        float eval = EvaluationValues.BoardEvaluate(ChessGrid.Pieces, strength, false);
        
        // Find the percentage of strength distribution (0 completely favors player2, 1 completely favors player1)
        eval += totalStrength / 2;
        float bar = eval / totalStrength;
        uiManager.evalBar.value = bar;  // update eval bar
    }
    
    // Declare winner and load end scene
    public static void Winner(string playerWinner)
    {
        EndScene.Winner = playerWinner == "player1" ? "Player 1" : "Player 2";
        SceneManager.LoadScene("End");
    }
    
    public Player GetCurrentPlayer()
    {
        return _currentPlayer;
    }

    // End a player's turn before making max possible moves (determined by number of commanders).
    public void EndTurn()
    {
        if (_currentPlayer != PovManager.Instance.Pov) return;
        for (int i = 0; i < 3 - turnCounter; i++)
        {
            uiManager.InsertMoveRecord("Skipped", false, Color.white);
        }
        turnCounter = 2;
        IncrementTurnCounter();
    }
}
