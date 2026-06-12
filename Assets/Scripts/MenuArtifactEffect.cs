using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MenuArtifactEffect : MonoBehaviour
{
    public enum EffectKind
    {
        Card,
        Pawn,
        Chip,
        Aura
    }

    public EffectKind kind = EffectKind.Aura;
    public RectTransform rectTransform;
    public Image image;
    public CanvasGroup canvasGroup;

    public float phase;
    public float spinSpeed = 0.45f;
    public float flipAmount = 0.18f;
    public float shimmerAmount = 0.12f;
    public Color colorA = Color.white;
    public Color colorB = Color.white;

    Vector3 baseScale = Vector3.one;
    float baseRotation;
    bool initialized;

    public void Initialize(EffectKind effectKind, float phaseOffset, float speed, float amount, float shimmer, Color from, Color to)
    {
        CacheReferences();
        CaptureBaseTransform();
        kind = effectKind;
        phase = phaseOffset;
        spinSpeed = speed;
        flipAmount = amount;
        shimmerAmount = shimmer;
        colorA = from;
        colorB = to;
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
        {
            CacheReferences();
            CaptureBaseTransform();
            initialized = true;
        }

        float t = Time.unscaledTime + phase;
        float wave = (Mathf.Sin(t * spinSpeed) + 1f) * 0.5f;
        float wave2 = (Mathf.Sin(t * spinSpeed * 1.73f + 1.1f) + 1f) * 0.5f;

        switch (kind)
        {
            case EffectKind.Card:
                ApplyCardFlip(t, wave);
                break;
            case EffectKind.Pawn:
                ApplyPawnPulse(wave, wave2);
                break;
            case EffectKind.Chip:
                ApplyChipSpin(t, wave);
                break;
            default:
                ApplyAuraPulse(wave);
                break;
        }
    }

    void ApplyCardFlip(float t, float wave)
    {
        float xScale = 1f - Mathf.Pow(Mathf.Sin(t * spinSpeed), 2f) * flipAmount;
        float yScale = 1f + Mathf.Sin(t * spinSpeed * 0.5f + 0.4f) * flipAmount * 0.18f;
        rectTransform.localScale = new Vector3(baseScale.x * xScale, baseScale.y * yScale, baseScale.z);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, baseRotation + Mathf.Sin(t * spinSpeed * 0.42f) * 7f);
        ApplyColor(wave);
    }

    void ApplyPawnPulse(float wave, float wave2)
    {
        float scale = 1f + Mathf.Lerp(-shimmerAmount * 0.35f, shimmerAmount, wave);
        rectTransform.localScale = baseScale * scale;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, baseRotation + Mathf.Lerp(-2.5f, 2.5f, wave2));
        ApplyColor(wave);
    }

    void ApplyChipSpin(float t, float wave)
    {
        float scale = 1f + Mathf.Sin(t * spinSpeed * 0.8f) * flipAmount * 0.28f;
        rectTransform.localScale = baseScale * scale;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, baseRotation + t * spinSpeed * 24f);
        ApplyColor(wave);
    }

    void ApplyAuraPulse(float wave)
    {
        float scale = 1f + Mathf.Lerp(-flipAmount * 0.25f, flipAmount, wave);
        rectTransform.localScale = baseScale * scale;
        ApplyColor(wave);
        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.Lerp(colorA.a, colorB.a, wave);
    }

    void ApplyColor(float wave)
    {
        if (image == null) return;
        image.color = Color.Lerp(colorA, colorB, wave);
    }

    void CacheReferences()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;
        if (image == null)
            image = GetComponent<Image>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void CaptureBaseTransform()
    {
        if (rectTransform == null) return;
        baseScale = rectTransform.localScale;
        baseRotation = rectTransform.localEulerAngles.z;
    }
}
