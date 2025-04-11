using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    public float deathDelay = 1.5f; // Time before reloading scene
    public string enemyLayerName = "Enemy";
    public string pushPullLayerName = "PushPullOBJ";
    public float lethalSpeed = 8f; // Speed threshold for deadly push/pull objects
    public Animator animator;

    public bool isDead = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        GameObject other = collision.gameObject;
        int otherLayer = other.layer;

        if (otherLayer == LayerMask.NameToLayer(enemyLayerName))
        {
            // Enemy kills instantly
            StartCoroutine(HandleDeath());
        }
        else if (otherLayer == LayerMask.NameToLayer(pushPullLayerName))
        {
            // PushPullOBJ kills only if moving fast enough
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && rb.velocity.magnitude >= lethalSpeed)
            {
                StartCoroutine(HandleDeath());
            }
        }
    }

    IEnumerator HandleDeath()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Optional: disable movement scripts, etc.
        yield return new WaitForSeconds(deathDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
