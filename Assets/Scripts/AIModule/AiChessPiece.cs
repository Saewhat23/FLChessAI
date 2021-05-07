using Board;

namespace AIModule
{
    /**
     * AiChessPiece
     *
     * A wrapper around ChessPiece
     * Allows for thread to use ChessPiece object, and represents AI view of board.
     * More lightweight as it does not have Unity instantiated object overhead.
     */
    public class AiChessPiece
    {
        public readonly string Name;
        public readonly Player Owner;
        public readonly string Type;
        public Square Position;
        public bool moveable;

        public AiChessPiece(string name, Player owner, string type, Square position)
        {
            Name = name;
            Owner = owner;
            Type = type;
            Position = position;
        }
        
        public override string ToString()
        {
            return Type + " - " + Owner  + " - " + Position;
        }

        public string GetDivision()
        {
            return Name.Substring(0, 1);
        }

        public Commander GetCommander()
        {
            return Owner.GetCommander(Name.Substring(0, 1));
        }
    }
}