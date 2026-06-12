using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DynamicBackgroundElement : MonoBehaviour
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;

    public float floatAmplitude = 8f;
    public Vector2 floatAmplitudeOverride = Vector2.zero;
    public float floatSpeed = 0.35f;
    public float rotationAmplitude = 3f;
    public float rotationSpeed = 0.25f;
    public float scaleAmplitude = 0f;
    public float scaleSpeed = 0.2f;
    public float alphaMin = 0.08f;
    public float alphaMax = 0.18f;

    Vector2 basePosition;
    Vector3 baseScale = Vector3.one;
    float baseRotation;
    float phaseOffset;
    float intensity = 0.35f;
    bool initialized;

    void Awake()
    {
        CacheReferences();
        CaptureBaseTransform();
    }

    public void Initialize(float phase)
    {
        CacheReferences();
        CaptureBaseTransform();
        phaseOffset = phase;
        initialized = true;
    }

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }

    void Update()
    {
        if (!initialized)
        {
            CacheReferences();
            CaptureBaseTransform();
            initialized = true;
        }

        float t = Time.unscaledTime + phaseOffset;
        float currentFloat = floatAmplitude * intensity;
        Vector2 currentFloat2 = floatAmplitudeOverride == Vector2.zero
            ? new Vector2(currentFloat, currentFloat * 0.45f)
            : floatAmplitudeOverride * intensity;
        rectTransform.anchoredPosition = basePosition + new Vector2(
            Mathf.Sin(t * floatSpeed) * currentFloat2.x,
            Mathf.Cos(t * floatSpeed * 0.73f) * currentFloat2.y);

        float z = baseRotation + Mathf.Sin(t * rotationSpeed) * rotationAmplitude * intensity;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, z);

        if (scaleAmplitude > 0f)
        {
            float scale = 1f + Mathf.Sin(t * scaleSpeed) * scaleAmplitude * intensity;
            rectTransform.localScale = baseScale * scale;
        }

        if (canvasGroup != null)
        {
            float alpha = Mathf.Lerp(alphaMin, alphaMax, (Mathf.Sin(t * 0.42f) + 1f) * 0.5f);
            canvasGroup.alpha = Mathf.Lerp(alphaMin, alpha, intensity);
        }
    }

    void CacheReferences()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void CaptureBaseTransform()
    {
        if (rectTransform == null) return;
        basePosition = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
        baseRotation = rectTransform.localEulerAngles.z;
    }
}
