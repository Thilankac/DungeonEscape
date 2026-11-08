using UnityEngine;

/// <summary>
/// Lightweight world animation: bob, spin, and/or scale pulse.
/// Uses unscaled time so it can keep moving after the game pauses.
/// </summary>
public class SimplePulse : MonoBehaviour
{
    public enum PulseMode
    {
        Bob,
        Spin,
        ScalePulse,
        BobAndSpin
    }

    [SerializeField] private PulseMode mode = PulseMode.Bob;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float amount = 0.15f;
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private bool onlyWhilePlaying = true;

    private Vector3 startLocalPos;
    private Vector3 startLocalScale;
    private float phase;

    private void Awake()
    {
        startLocalPos = transform.localPosition;
        startLocalScale = transform.localScale;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (onlyWhilePlaying && GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        phase += dt * speed;

        switch (mode)
        {
            case PulseMode.Bob:
                transform.localPosition = startLocalPos + Vector3.up * Mathf.Sin(phase) * amount;
                break;

            case PulseMode.Spin:
                transform.Rotate(0f, 0f, speed * 40f * dt);
                break;

            case PulseMode.ScalePulse:
            {
                float s = 1f + Mathf.Sin(phase) * amount;
                transform.localScale = startLocalScale * s;
                break;
            }

            case PulseMode.BobAndSpin:
                transform.localPosition = startLocalPos + Vector3.up * Mathf.Sin(phase) * amount;
                transform.Rotate(0f, 0f, speed * 35f * dt);
                break;
        }
    }
}
