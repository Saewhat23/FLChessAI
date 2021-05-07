using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    /*
     * Commander
     *
     * Represents a King or Bishop on the board for a certain player.
     */
    public class Commander
    {
        private const float Alpha = 0.85f;                                                      // Transparency of all colors
        public static Color MiddleCommander = new Color(0, 1, 0, Alpha);                // Color for middle/king group
        public static Color LeftCommander = new Color(1, 0, 0, Alpha);                  // Color for left/bishop group
        public static Color RightCommander = new Color(0, 0, 1, Alpha);                 // Color for right/bishop group
        public static Color MovedCommander = new Color(0.5f, 0.5f,0.5f, Alpha);         // Color for disabled group upon move
        
        public bool Moved;
        public readonly List<ChessPiece> Pieces = new List<ChessPiece>();

        public void AddPiece(ChessPiece piece)
        {
            Pieces.Add(piece);
        }

        public void RemovePiece(ChessPiece piece)
        {
            Pieces.Remove(piece);
        }

        // Transition all pieces to disabled
        public void SetMoved()
        {
            Moved = true;
            foreach (ChessPiece piece in Pieces)
            {
                piece.SetColor("moved");
            }
        }

        // Reset all pieces on new turn
        public void Reset()
        {
            Moved = false;
            foreach (ChessPiece piece in Pieces)
            {
                piece.SetColor(piece.GetDivision());
            }
        }
    }
}