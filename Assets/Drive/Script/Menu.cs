using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject gameUI;
    public Button[] menuButtons; // Assign in order: Resume, Restart, Quit

    private int currentIndex = 0;
    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) PauseGame();
            else ResumeGame();
        }

        if (isPaused)
        {
            HandleMenuNavigation();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);

        currentIndex = 0;
        HighlightButton(currentIndex);
    }

    public void ResumeGame()
    {
        UnpauseGame();
    }

    public void RestartGame()
    {
        UnpauseGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        UnpauseGame();
        SceneManager.LoadScene("MenuScene");
    }

    void UnpauseGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
    }

    void HandleMenuNavigation()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuButtons.Length;
            HighlightButton(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            menuButtons[currentIndex].onClick.Invoke(); // Calls ResumeGame, RestartGame, or QuitToMenu
        }
    }

    void HighlightButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
    }
}
