using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    public float deathDelay = 1.5f; // Time before reloading scene
    public string enemyLayerName = "Enemy";
    public Animator animator; // Reference to player's Animator

    public bool isDead = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer(enemyLayerName))
        {
            StartCoroutine(HandleDeath());
        }
    }

    IEnumerator HandleDeath()
    {
        isDead = true;

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            // Optional: Disable movement or other scripts her
            yield return new WaitForSeconds(deathDelay);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
