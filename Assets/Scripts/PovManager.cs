using Board;
using UnityEngine;

/*
 * POVManager
 *
 * Holds Player objects for the POV of the player and the opponent.
 * Allows for the user to play as player 1 or 2, and against AI or human.
 */
public class PovManager : MonoBehaviour
{
    public Player Pov;
    public Player Opp;

    public GameObject player1Objects;
    public GameObject player2Objects;

    public static PovManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(Player player, Player opponent)
    {
        Pov = player;
        Opp = opponent;
        GameObject active = Pov.Name == "player1" ? player1Objects : player2Objects;
        active.SetActive(true);
    }
}
