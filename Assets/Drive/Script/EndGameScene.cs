using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EndGameTrigger : MonoBehaviour
{
    private List<GameObject> playersInScene;
    private HashSet<GameObject> playersInside = new HashSet<GameObject>();

    public string endSceneName = "EndGame"; // Change this to your actual end scene name

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        playersInScene = new List<GameObject>(players);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playersInScene.Contains(other.gameObject))
        {
            playersInside.Add(other.gameObject);
            CheckAllPlayersInside();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playersInside.Contains(other.gameObject))
        {
            playersInside.Remove(other.gameObject);
        }
    }

    void CheckAllPlayersInside()
    {
        if (playersInside.Count == playersInScene.Count)
        {
            // All players are in the zone
            SceneManager.LoadScene(endSceneName);
        }
    }
}