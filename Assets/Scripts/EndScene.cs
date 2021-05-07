using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * EndScene
 *
 * Displays the winner in a different scene.
 * Clicking will reload the start scene for gameplay loop.
 */
public class EndScene : MonoBehaviour
{
    public static string Winner;
    public Text winnerText;

    private void Start()
    {
        winnerText.text = Winner + " wins!";
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            SceneManager.LoadScene("Start");
    }
}
