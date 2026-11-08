using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// UI motion helpers that keep working when Time.timeScale is 0.
/// </summary>
public class UiAnimator : MonoBehaviour
{
    public static UiAnimator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void PunchScale(Transform target, float punch = 0.25f, float duration = 0.25f)
    {
        if (target == null)
        {
            return;
        }

        StopCoroutine(nameof(PunchRoutine));
        StartCoroutine(PunchRoutine(target, punch, duration));
    }

    public void ShowPanelAnimated(GameObject panel, float duration = 0.35f)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(true);
        StartCoroutine(PanelInRoutine(panel.transform, duration));
    }

    public void FlashMessage(TextMeshProUGUI label, string message, float punch = 0.12f)
    {
        if (label == null)
        {
            return;
        }

        label.text = message;
        PunchScale(label.transform, punch, 0.28f);
        StartCoroutine(FlashColorRoutine(label));
    }

    private IEnumerator PunchRoutine(Transform target, float punch, float duration)
    {
        Vector3 original = target.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            // Ease out bounce-ish curve.
            float wave = Mathf.Sin(n * Mathf.PI) * (1f - n);
            target.localScale = original * (1f + wave * punch * 4f);
            yield return null;
        }

        target.localScale = original;
    }

    private IEnumerator PanelInRoutine(Transform panel, float duration)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = panel.gameObject.AddComponent<CanvasGroup>();
        }

        Vector3 endScale = Vector3.one;
        panel.localScale = Vector3.one * 0.7f;
        group.alpha = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            panel.localScale = Vector3.Lerp(Vector3.one * 0.7f, endScale, n);
            group.alpha = n;
            yield return null;
        }

        panel.localScale = endScale;
        group.alpha = 1f;
    }

    private IEnumerator FlashColorRoutine(TextMeshProUGUI label)
    {
        Color original = label.color;
        label.color = Color.white;
        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.unscaledDeltaTime;
            label.color = Color.Lerp(Color.yellow, original, t / 0.35f);
            yield return null;
        }

        label.color = original;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
