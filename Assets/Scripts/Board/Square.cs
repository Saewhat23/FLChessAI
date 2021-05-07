namespace Board
{
    /**
     * Square
     *
     * Represents a square on the board.
     */
    public class Square
    {
        public readonly int X;
        public readonly int Y;
        public bool AttackOnly;

        public Square(int x, int y, bool attackOnly) : this(x, y)
        {
            X = x;
            Y = y;
            AttackOnly = attackOnly;
        }

        public Square(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Equivalence methods
        public override bool Equals(object obj)
        {
            Square other = obj as Square;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 710470020;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    
        public override string ToString()
        {
            return X + ", " + Y;
        }
    }
}
