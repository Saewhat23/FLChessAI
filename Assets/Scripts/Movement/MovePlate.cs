using System.Linq;
using Board;
using UnityEngine;

namespace Movement
{
    /**
     * MovePlate
     *
     * An object that represents a clickable box for a piece to move to.
     */
    public class MovePlate : MonoBehaviour
    {
        private ChessMove _chessMove;
        public bool attackOnly;
    
        // Properties for changing the color depending on mouse movement
        private SpriteRenderer _spriteRenderer;
        private readonly Color _initial = new Color(0.16f, 0.15f, 0.22f, 0.69f);
        private readonly Color _attacking = new Color(0.43f, 0f, 0.01f, 0.46f);
        private readonly Color _highlighted = new Color(0.46f, 0.72f, 0.34f, 0.61f);
    
        private void Init()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.color = _chessMove.Attack ? _attacking : _initial;
        }
    
        private void Update()
        {
            Color color;
        
            // Determine raycasts - if the mouse is hovering over the object and released, then the piece that is being dragged will be moved here
            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            bool hovering = false;
            if (hits.Any(ray => ray.collider.gameObject == gameObject))
            {
                if (Input.GetMouseButtonUp(0) && !Game.Controller.die.rolling)  // make sure die is not rolling
                    StartCoroutine(MovementUtil.Instance.MovePiece(_chessMove, attackOnly));
                hovering = true;

                if (_chessMove.Attack && !Game.Controller.die.rolling)
                {
                    ChessPiece initialPiece = ChessGrid.GetPosition(_chessMove.InitialSquare);
                    ChessPiece targetPiece = ChessGrid.GetPosition(_chessMove.TargetSquare);
                    Game.Controller.uiManager.UpdateAttack(UIManager.BuildNotation(initialPiece.type, _chessMove.InitialSquare, _chessMove.TargetSquare, true) + " - "
                        + CaptureMatrix.GetMin(initialPiece.type, targetPiece.type, _chessMove.AddOne) + "+ needed");
                }
            }

            // Interpolate color based on mouse input
            if (hovering)
                color = _highlighted;
            else
                color = _chessMove.Attack ? _attacking : _initial;
            _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, color, 20 * Time.deltaTime);
        }

        // Spawn a moveplate at a position
        public static void Spawn(Square initial, Square target, bool attackOnly, Game controller)
        {
            // In world coordinates of moveplate
            float[] coords = ChessGrid.GetRealPos(target);
            GameObject obj = Instantiate(controller.movePlate, controller.movePlateParent.transform);
            obj.name = "MovePlate";
            obj.transform.localPosition = new Vector3(coords[0], coords[1]);
            obj.transform.localScale = new Vector3(1, 1, 1);
            MovePlate movePlate = obj.GetComponent<MovePlate>();
            movePlate._chessMove = new ChessMove(initial, target, 0);
            movePlate.attackOnly = attackOnly;
            movePlate.Init();
        }
    }
}
