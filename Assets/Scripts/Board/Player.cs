using System.Collections.Generic;

namespace Board
{
    /**
     * Player
     *
     * Represents the two players of the game.
     */
    public class Player
    {
        public readonly string Name;    // player 1 or 2
        public bool IsAI;               // Is the player an AI
        public int RemainingMoves = 2;  // How many remaining moves based on number of commanders

        public bool isSubAI;

        // used to track the piece that was delegated and revert it on next turn
        public ChessPiece Selected;
        public string SelectedDivision;
        public List<ChessPiece> Delegated = new List<ChessPiece>();  // Pieces that were delegated this turn
        public List<string> DelegatedDivisions = new List<string>(); // Original division/corp of that piece
        
        public Dictionary<string, Commander> Commanders = new Dictionary<string, Commander>();

        // Full copy of a player object - used by AI to simulate new environment
        public Player(Player copy)
        {
            Name = copy.Name;
            IsAI = copy.IsAI;
            Commanders.Add("M", new Commander());
            Commanders.Add("L", new Commander());
            Commanders.Add("R", new Commander());
            Commanders["M"].Moved = copy.Commanders["M"].Moved;
            Commanders["R"].Moved = copy.Commanders["R"].Moved;
            Commanders["L"].Moved = copy.Commanders["L"].Moved;
            RemainingMoves = copy.RemainingMoves;
        }
    
        public Player(string name, bool isAI)
        {
            Name = name;
            IsAI = isAI;
            // Add default commanders
            Commanders.Add("M", new Commander());
            Commanders.Add("L", new Commander());
            Commanders.Add("R", new Commander());
        }

        public Player(bool isSubAI)
        {
            this.isSubAI = isSubAI;
            // Add default commanders
            Commanders.Add("M", new Commander());
            Commanders.Add("L", new Commander());
            Commanders.Add("R", new Commander());
        }

        public Commander GetCommander(string commander)
        {
            return Commanders[commander];
        }

        // When a bishop is captured, transition all of its pieces to the king
        public void TransitionCommand(string oldCommander)
        {
            Commander oldC = Commanders[oldCommander];
            foreach (ChessPiece piece in oldC.Pieces)
            {
                piece.TransitionTo("M");
            }
        }

        // Disable all moves for a commander when they have moved
        public void Disable()
        {
            foreach (Commander c in Commanders.Values)
            {
                c.SetMoved();
            }
        }
    
        // Reset all moves on new turn
        public void Reset()
        {
            foreach (Commander c in Commanders.Values)
            {
                c.Reset();
            }
        }
    }
}