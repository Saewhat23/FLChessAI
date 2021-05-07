using Board;
using UnityEngine;
using UnityEngine.UI;

/**
 * UIManager
 *
 * Manages UI elements on screen such as move history,
 * attacking text, turn count text, etc.
 */
public class UIManager : MonoBehaviour
{
    public Text attackingText;
    public Text whiteMoves;
    public Text blackMoves;
    public GameObject historyRecord;
    public GameObject history;
    
    public Slider evalBar;
    
    private static string[] xLetters = { "a", "b", "c", "d", "e", "f", "g", "h" };

    public void Start()
    {
        whiteMoves.text = "1/3";
        blackMoves.text = "1/3";
    }

    // Attack text updates
    public void UpdateAttack(string text)
    {
        attackingText.text = text;
        attackingText.gameObject.SetActive(true);
    }

    public void DisableAttack()
    {
        attackingText.gameObject.SetActive(false);
    }

    public void InsertMoveRecord(string text, bool newTurn, Color color)
    {
        string record;
        if (newTurn)
        {
            record = text; // display turn number and player
        } else
        {
            record = "     - " + text; // display a move by the player, indented under current turn
        }

        GameObject recordObj = Instantiate(historyRecord, history.transform);
        Text t = recordObj.GetComponent<Text>();
        t.text = record;
        t.color = color;
    }

    private static string GetPieceAbbr(string type, Square square)
    {
        string notation = "";
        if (type.Equals("knight"))
        {
            notation = "N";
        } else if (!type.Equals("pawn"))
        {
            notation = type.Substring(0, 1).ToUpper();
        }

        notation += GetPosNotation(square);
        return notation;
    }

    private static string GetPosNotation(Square square)
    {
        return xLetters[square.X] + (square.Y + 1);
    }

    public static string BuildNotation(string type, Square initial, Square target, bool capture)
    {
        string notation = GetPieceAbbr(type, initial);
        if (capture)
        {
            ChessPiece targetPiece = ChessGrid.GetPosition(target);
            notation += " x " + GetPieceAbbr(targetPiece.type, target);
        }
        else
        {
            notation += " -> " + GetPosNotation(target);
        }

        return notation;
    }
}
