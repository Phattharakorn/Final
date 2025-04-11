using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Load the game scene (set scene index or name in Build Settings)
    public void StartGame()
    {
        Time.timeScale = 1f; // Just in case you're resuming from pause
        SceneManager.LoadScene("P_Drive"); // Replace with your actual scene name
        // OR use SceneManager.LoadScene(1); if you're using scene index
    }

    // Quit the game
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}
