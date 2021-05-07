using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Board
{
    /**
     * ChessGrid
     * 
     * Stores the state of the board, objects, and sprites.
     */
    public class ChessGrid : MonoBehaviour
    {
        public static ChessGrid Instance;
        public GameObject Piece;                // prefab object for a new piece
        public Image board;                     // board background object
        private const float SquareSize = 0.3f; // grid size for board

        
        // Stores different types of sprites so the player can toggle between
        [Serializable]
        public struct PieceSprite
        {
            public string name;
            public Sprite reg_player1, reg_player2;
            public Sprite pix_player1, pix_player2;
        }
        public PieceSprite[] sprites;
        public Dictionary<string, PieceSprite> spriteDictionary = new Dictionary<string, PieceSprite>();
        public Sprite[] boards;
        public bool usingRegSprites;
        public bool usingRegBoard;

        // Stores the commander icons that show which group each piece belongs to
        public GameObject CommanderOverlay;
        public List<GameObject> CommanderOverlays = new List<GameObject>();
    
        // Stores pieces on the board
        public static readonly Dictionary<Square, ChessPiece> Pieces = new Dictionary<Square, ChessPiece>();
        public static readonly List<ChessPiece> Captured = new List<ChessPiece>();

        private void Awake()
        {
            Instance = this;
        }

        public void Init()
        {
            foreach (PieceSprite pieceSprite in sprites)
            {
                spriteDictionary.Add(pieceSprite.name, pieceSprite);
            }
            InitPieces();
            foreach (ChessPiece piece in  Pieces.Values)
            {
                piece.gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        //Creation method that instantiates a new chess piece at the location and saves the necessary information
        private void Create(string pieceName, int x, int y, Sprite sprite)
        {
            Player player = pieceName.Contains("player1") ? Game.Controller.Player1 : Game.Controller.Player2;
            GameObject obj = Instantiate(Piece, new Vector3(0, 0, -4.0f), Quaternion.identity);
            GameObject overlay = Instantiate(CommanderOverlay, obj.transform);
            overlay.transform.localPosition = new Vector3(0, 0, -5);
            overlay.transform.localScale = new Vector3(.14f, .14f, 1);
            CommanderOverlays.Add(overlay);
            ChessPiece piece = obj.GetComponent<ChessPiece>();
            piece.Init(pieceName, player, x, y, sprite);
            SetPosition(piece.Position, piece);
        }

        // Switch between pixel and regular graphics
        public void ToggleGraphics()
        {
            usingRegSprites = !usingRegSprites;
            List<ChessPiece> all = new List<ChessPiece>();
            all.AddRange(Pieces.Values);
            all.AddRange(Captured);
            foreach (ChessPiece piece in all)
            {
                SpriteRenderer spriteRenderer = piece.gameObject.GetComponent<SpriteRenderer>();
                Sprite sprite;
                if (piece.Owner.Name == "player1")
                {
                    sprite = usingRegSprites ? spriteDictionary[piece.type].reg_player1 : spriteDictionary[piece.type].pix_player1;
                }
                else
                {
                    sprite = usingRegSprites ? spriteDictionary[piece.type].reg_player2 : spriteDictionary[piece.type].pix_player2;
                }
                spriteRenderer.sprite = sprite;
            }
        }

        // Switch between forest background and regular background
        public void ToggleBoard()
        {
            usingRegBoard = !usingRegBoard;
            board.sprite = usingRegBoard ? boards[0] : boards[1];
        }

        // Toggle hide/show commander icons
        public void ToggleCommanderOverlays()
        {
            foreach (GameObject obj in CommanderOverlays)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    
        // Create all the game pieces
        private void InitPieces()
        {
            Create("M player1_king", 4, 0, spriteDictionary["king"].reg_player1);
            Create("L player1_bishop", 2, 0, spriteDictionary["bishop"].reg_player1);
            Create("R player1_bishop", 5, 0, spriteDictionary["bishop"].reg_player1);
            Create("M player1_rook", 0, 0, spriteDictionary["rook"].reg_player1);
            Create("M player1_rook", 7, 0, spriteDictionary["rook"].reg_player1);
            Create("L player1_knight", 1, 0, spriteDictionary["knight"].reg_player1);
            Create("R player1_knight", 6, 0, spriteDictionary["knight"].reg_player1);
            Create("M player1_queen", 3, 0, spriteDictionary["queen"].reg_player1);
            Create("L player1_pawn", 0, 1, spriteDictionary["pawn"].reg_player1);
            Create("L player1_pawn", 1, 1, spriteDictionary["pawn"].reg_player1);
            Create("L player1_pawn", 2, 1, spriteDictionary["pawn"].reg_player1);
            Create("M player1_pawn", 3, 1, spriteDictionary["pawn"].reg_player1);
            Create("M player1_pawn", 4, 1, spriteDictionary["pawn"].reg_player1);
            Create("R player1_pawn", 5, 1, spriteDictionary["pawn"].reg_player1);
            Create("R player1_pawn", 6, 1, spriteDictionary["pawn"].reg_player1);
            Create("R player1_pawn", 7, 1, spriteDictionary["pawn"].reg_player1);

            Create("M player2_king", 4, 7, spriteDictionary["king"].reg_player2);
            Create("L player2_bishop", 2, 7, spriteDictionary["bishop"].reg_player2);
            Create("R player2_bishop", 5, 7, spriteDictionary["bishop"].reg_player2);
            Create("M player2_rook", 0, 7, spriteDictionary["rook"].reg_player2);
            Create("M player2_rook", 7, 7, spriteDictionary["rook"].reg_player2);
            Create("L player2_knight", 1, 7, spriteDictionary["knight"].reg_player2);
            Create("R player2_knight", 6, 7, spriteDictionary["knight"].reg_player2);
            Create("M player2_queen", 3, 7, spriteDictionary["queen"].reg_player2);
            Create("L player2_pawn", 0, 6, spriteDictionary["pawn"].reg_player2);
            Create("L player2_pawn", 1, 6, spriteDictionary["pawn"].reg_player2);
            Create("L player2_pawn", 2, 6, spriteDictionary["pawn"].reg_player2);
            Create("M player2_pawn", 3, 6, spriteDictionary["pawn"].reg_player2);
            Create("M player2_pawn", 4, 6, spriteDictionary["pawn"].reg_player2);
            Create("R player2_pawn", 5, 6, spriteDictionary["pawn"].reg_player2);
            Create("R player2_pawn", 6, 6, spriteDictionary["pawn"].reg_player2);
            Create("R player2_pawn", 7, 6, spriteDictionary["pawn"].reg_player2);
        }

        public static void SetPositionEmpty(Square square)
        {
            Pieces.Remove(square);
        }

        public static void SetPosition(Square square, ChessPiece piece)
        {
            if (Pieces.ContainsKey(square))
                Pieces.Remove(square);
            Pieces.Add(square, piece);
        }

        public static ChessPiece GetPosition(Square position)
        {
            if (Pieces[position] == null)
                return null;
            return Pieces[position];
        }

        // Checks if a square is actually inside of the chess board
        public static bool ValidPosition(Square square)
        {
            return !(square.X < 0 || square.Y < 0 || square.X >= 8 || square.Y >= 8);
        }
    
        // Fixes the coordinates so it sticks to the board.
        public static float[] GetRealPos(Square position)
        {
            float xVal = position.X;
            float yVal = position.Y;
            if (PovManager.Instance.Pov.Name == "player2")
            {
                // invert position if POV is of player2
                xVal = 7 - xVal;
                yVal = 7 - yVal;
            }
            xVal *= SquareSize;
            yVal *= SquareSize;
            xVal -= SquareSize * 3.5f;
            yVal -= SquareSize * 3.5f;
            return new[] { xVal, yVal };
        }
    }
}