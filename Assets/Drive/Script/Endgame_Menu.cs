using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    // Load the game scene (set scene index or name in Build Settings)
    public void Retry()
    {
        Time.timeScale = 1f; // Just in case you're resuming from pause
        SceneManager.LoadScene("MainGame"); // Replace with your actual scene name
        // OR use SceneManager.LoadScene(1); if you're using scene index
    }

    // Quit the game
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        SceneManager.LoadScene("MainMenu");
    }
}
