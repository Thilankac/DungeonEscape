using UnityEngine;

/// <summary>Optional bonus points. Coins do not unlock the exit.</summary>
[RequireComponent(typeof(Collider2D))]
public class CoinCollect : MonoBehaviour
{
    [SerializeField, Min(1)] private int scoreValue = 5;
    private bool collected;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        GameManager manager = GameManager.Instance;
        if (manager == null || !manager.IsPlaying) return;

        // Set the flag first so multiple colliders cannot award points twice.
        collected = true;
        manager.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
