using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * StartScene
 *
 * Basic setter for the initial game load.
 * Allows the user to set state using on screen buttons that are linked to these methods.
 */
public class StartScene : MonoBehaviour
{
    public static bool VsAi = true;
    public static bool Player1 = true;
    public static bool AIVsAI = true;

    public void SetPlayer(bool isPlayer1)
    {
        Player1 = isPlayer1;
    }

    public void SetVsAI(bool isVsAI)
    {
        VsAi = isVsAI;
    }
    public void SetAIVsAI(bool isAIVsAI)
    {
        AIVsAI = isAIVsAI;
    }
    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
