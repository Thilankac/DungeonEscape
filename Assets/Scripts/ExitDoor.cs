using UnityEngine;

/// <summary>
/// Exit door. The player can win only after collecting the key.
/// Without the key, a hint message is shown.
/// Glows when unlocked.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ExitDoor : MonoBehaviour
{
    [Header("Messages")]
    [SerializeField] private string needKeyMessage = "Collect the key first!";

    [Header("Visuals")]
    [SerializeField] private Color lockedColor = new Color(0.25f, 0.55f, 0.3f, 1f);
    [SerializeField] private Color unlockedColor = new Color(0.45f, 1f, 0.55f, 1f);
    [SerializeField] private float glowSpeed = 4f;

    private SpriteRenderer spriteRenderer;
    private bool unlocked;
    private Vector3 baseScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = lockedColor;
        }
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        unlocked = GameManager.Instance != null && GameManager.Instance.HasKey;

        if (spriteRenderer == null)
        {
            return;
        }

        if (unlocked)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * glowSpeed) + 1f) * 0.5f;
            spriteRenderer.color = Color.Lerp(unlockedColor, Color.white, pulse * 0.35f);
            transform.localScale = baseScale * (1f + pulse * 0.08f);
        }
        else
        {
            spriteRenderer.color = lockedColor;
            transform.localScale = baseScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.HasKey)
        {
            GameManager.Instance.TriggerWin();
        }
        else
        {
            GameManager.Instance.ShowMessage(needKeyMessage);
        }
    }
}
