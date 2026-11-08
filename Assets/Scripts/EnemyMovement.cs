using UnityEngine;

/// <summary>
/// Patrols the enemy back and forth between two world points.
/// Touching the player removes one life.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    [Header("Patrol Points")]
    [Tooltip("First patrol position in world space.")]
    [SerializeField] private Transform pointA;

    [Tooltip("Second patrol position in world space.")]
    [SerializeField] private Transform pointB;

    [Header("Movement")]
    [Tooltip("How fast the enemy moves between points.")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("How close the enemy must get before reversing.")]
    [SerializeField] private float arriveDistance = 0.05f;

    private Transform currentTarget;

    private void Start()
    {
        // Start by moving toward point B (or A if B is missing).
        if (pointB != null)
        {
            currentTarget = pointB;
        }
        else if (pointA != null)
        {
            currentTarget = pointA;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (currentTarget == null || pointA == null || pointB == null)
        {
            return;
        }

        // Move toward the current target.
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            moveSpeed * Time.deltaTime);

        // Reverse when close enough to the target.
        if (Vector3.Distance(transform.position, currentTarget.position) <= arriveDistance)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solid contact removes a life and respawns the player.
        if (collision.collider.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamagePlayer(collision.collider.GetComponentInParent<PlayerMovement>());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Also support trigger-based enemies if preferred.
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamagePlayer(other.GetComponentInParent<PlayerMovement>());
            }
        }
    }
}

