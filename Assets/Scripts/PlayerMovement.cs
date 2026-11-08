using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves the player with WASD / arrow keys using the new Input System
/// and Rigidbody2D.MovePosition so walls block movement.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("How fast the player moves (units per second).")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 spawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = rb.position;
    }

    public void Respawn()
    {
        // Keep coins, score and the key; only move the player back to safety.
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = spawnPosition;
        transform.position = new Vector3(spawnPosition.x, spawnPosition.y, transform.position.z);
    }

    /// <summary>
    /// Called automatically by the PlayerInput component
    /// when Behavior is set to "Send Messages".
    /// </summary>
    public void OnMove(InputValue value)
    {
        // Read the Vector2 from WASD / arrow keys / gamepad stick.
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        // Stop moving after Win or Game Over.
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        // MovePosition respects 2D physics collisions (walls).
        Vector2 newPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}
