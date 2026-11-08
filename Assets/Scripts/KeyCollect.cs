using UnityEngine;

/// <summary>
/// Collectible key. When the player enters the trigger:
/// adds score, plays a sound, unlocks the exit, then hides/destroys itself.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KeyCollect : MonoBehaviour
{
    [Header("Score")]
    [Tooltip("Points awarded when the key is collected.")]
    [SerializeField] private int scoreValue = 10;

    private bool collected;

    private void Reset()
    {
        // Ensure the collider is a trigger when the script is first added.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react once, and only to the Player.
        if (collected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        collected = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectKey(scoreValue);
        }

        // Remove the key from the level.
        Destroy(gameObject);
    }
}
