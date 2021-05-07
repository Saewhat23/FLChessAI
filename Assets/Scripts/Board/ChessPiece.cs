using System.Collections.Generic;
using Movement;
using UnityEngine;

namespace Board
{
    /**
     * ChessPiece
     * 
     * Represents a Chess piece object on the board.
     */
    public class ChessPiece : MonoBehaviour
    {
        public Player Owner;
        public string type;
        public Square Position;
        public SpriteRenderer SpriteRenderer;
        public bool moveable;
        private RectTransform _rectTransform;
        public bool dragging;

        public ChessPiece(Player owner, string type, Square position)
        {
            Owner = owner;
            this.type = type;
            Position = position;
        }
    
        public void Init(string pieceName, Player player, int x, int y, Sprite sprite)
        {
            // Set sprite
            GetComponent<SpriteRenderer>().sprite = sprite;

            // Piece information
            name = pieceName;
            Owner = player;
            type = name.Substring(10);
            gameObject.transform.SetParent(Owner.Name == "player1" ? Game.Controller.player1Parent : Game.Controller.player2Parent);
            Position = new Square(x, y);
            GetCommander().AddPiece(this);

            // Set position in world
            SetColor(GetDivision());
            SetRealPos();
            _rectTransform = GetComponent<RectTransform>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Delegate the piece temporarily to a new commander
        public void DelegateTo(string division)
        {
            Commander oldC = GetCommander();
            oldC.RemovePiece(this);
            name = division + name.Substring(1);
            Commander newC = GetCommander();
            newC.AddPiece(this);
            SetColor(division);
        }
    
        // Transition the piece permanently to a new commander
        public void TransitionTo(string division)
        {
            name = division + name.Substring(1);
            Commander newC = GetCommander();
            newC.AddPiece(this);
            SetColor(division);
        }

        // Change the color of commander icon depending on division and move status
        public void SetColor(string division)
        {
            SpriteRenderer spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            switch (division)
            {
                case "M": spriteRenderer.color = Commander.MiddleCommander; break;
                case "L": spriteRenderer.color = Commander.LeftCommander; break;
                case "R": spriteRenderer.color = Commander.RightCommander; break;
                case "moved": spriteRenderer.color = Commander.MovedCommander; break;
            }
        }

        public string GetDivision()
        {
            return name.Substring(0, 1);
        }

        public Commander GetCommander()
        {
            return Owner.GetCommander(name.Substring(0, 1));
        }
    
        private void Update()
        {
            // Always interpolate position over time back to where it is set at
            float speed = 10 * Time.deltaTime;
            float[] coords = ChessGrid.GetRealPos(Position);
            Vector3 pos = new Vector3(coords[0], coords[1], 0);
            if (!dragging)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, pos, speed);
                if (Vector3.Distance(transform.localPosition, pos) < 0.01f)
                    SpriteRenderer.sortingOrder = 0;
            }
        }

        // Set real coordinate position
        private void SetRealPos()
        {
            float[] coords = ChessGrid.GetRealPos(Position);
            Vector3 pos = new Vector3(coords[0], coords[1]);
            transform.localPosition = pos;
        }

        // Create moveplates for possible moves
        private void InitiateMovePlates()
        {
            List<Square> moves = MovementUtil.GetPossibleMoves(ChessGrid.Pieces, this);
            foreach (Square m in moves)
            {
                MovePlate.Spawn(Position, m, m.AttackOnly, Game.Controller);
            }
        }

        public void OnMouseDown()
        {
            if (Game.Controller.die.rolling || Game.Controller.GetCurrentPlayer() != Owner || Game.Controller.GetCurrentPlayer().IsAI)
                return;
        
            // allow for a king to delegate one piece to a bishop each turn
            if (GetDivision() == "M" && !GetCommander().Moved && type != "king")
            {
                Owner.Selected = this;
                Owner.SelectedDivision = GetDivision();
                Game.Controller.commandDelegation.gameObject.SetActive(true);
            }
            else
                Game.Controller.commandDelegation.gameObject.SetActive(false);
            
            SpriteRenderer.sortingOrder = 1;
            dragging = true;
            Game.Controller.uiManager.DisableAttack();

            if (Game.Controller.GetCurrentPlayer() == Owner)
            {
                MovementUtil.DestroyMovePlates();
                InitiateMovePlates();
            }
        }

        private void OnMouseUp()
        {
            dragging = false;
        }

        // Allow for dragging pieces with mouse
        private void OnMouseDrag()
        {
            if (!moveable || !dragging) return;
            float distance = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 current = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
            _rectTransform.position = new Vector3(current.x, current.y, transform.position.z);
        }

        // Equivalence methods
        public override bool Equals(object obj)
        {
            ChessPiece other = obj as ChessPiece;
            return Equals(other);
        }

        private bool Equals(ChessPiece other)
        {
            return type == other.type && Owner == other.Owner && Equals(Position, other.Position);
        }

        public override int GetHashCode()
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ (type != null ? type.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Owner != null ? Owner.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Position != null ? Position.GetHashCode() : 0);
            return hashCode;
        }
    }
}
