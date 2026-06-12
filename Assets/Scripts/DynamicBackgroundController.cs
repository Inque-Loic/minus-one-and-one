using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicBackgroundController : MonoBehaviour
{
    [System.Serializable]
    public class TitleLayerMotion
    {
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Vector2 drift = Vector2.zero;
        public float scaleAmplitude = 0.01f;
        public float alphaAmplitude = 0f;
        public float rotationAmplitude = 0f;
        public float phase;

        [HideInInspector] public Vector2 basePosition;
        [HideInInspector] public Vector3 baseScale;
        [HideInInspector] public float baseRotation;
        [HideInInspector] public float baseAlpha = 1f;
    }

    [Header("Layers")]
    public Image baseImage;
    public RectTransform patternA;
    public RectTransform patternB;
    public Image colorOverlay;
    public List<DynamicBackgroundElement> ornaments = new List<DynamicBackgroundElement>();
    public List<TitleLayerMotion> titleLayers = new List<TitleLayerMotion>();

    [Header("Colors")]
    public Color defaultOverlayColor = new Color(0.78f, 0.88f, 0.78f, 0.04f);
    public Color selectingOverlayColor = new Color(0.58f, 0.82f, 0.76f, 0.06f);
    public Color respondingOverlayColor = new Color(0.92f, 0.58f, 0.42f, 0.08f);
    public Color discussionOverlayColor = new Color(0.56f, 0.66f, 0.92f, 0.08f);
    public Color scoreGuessingOverlayColor = new Color(0.98f, 0.78f, 0.34f, 0.07f);
    public Color goodResultOverlayColor = new Color(0.52f, 0.82f, 0.42f, 0.08f);
    public Color villainResultOverlayColor = new Color(0.72f, 0.22f, 0.22f, 0.10f);
    public Color drawResultOverlayColor = new Color(0.60f, 0.68f, 0.78f, 0.08f);

    [Header("Motion")]
    public float patternScrollSpeedA = 7f;
    public float patternScrollSpeedB = 4f;
    public float overlayLerpSpeed = 2.4f;
    public float titleLoopDuration = 10f;
    public float titleMotionScale = 1f;

    Color targetOverlayColor;
    float targetIntensity = 0.35f;
    Vector2 patternABase;
    Vector2 patternBBase;

    void Awake()
    {
        CacheBasePositions();
        targetOverlayColor = defaultOverlayColor;
        ApplyMotionIntensity(targetIntensity);
        DisableRaycasts(transform);
    }

    void Update()
    {
        UpdateTitleLayerMotion();
        UpdatePatternMotion();
        UpdateOverlayColor();
    }

    public void RegisterTitleLayer(RectTransform rectTransform, CanvasGroup canvasGroup, Vector2 drift, float scaleAmplitude, float alphaAmplitude, float rotationAmplitude, float phase)
    {
        if (rectTransform == null) return;

        TitleLayerMotion motion = new TitleLayerMotion
        {
            rectTransform = rectTransform,
            canvasGroup = canvasGroup,
            drift = drift,
            scaleAmplitude = scaleAmplitude,
            alphaAmplitude = alphaAmplitude,
            rotationAmplitude = rotationAmplitude,
            phase = phase
        };

        CaptureTitleLayerBase(motion);
        titleLayers.Add(motion);
    }

    public void ClearTitleLayers()
    {
        titleLayers.Clear();
    }

    public void SetPhase(RoundPhase phase)
    {
        switch (phase)
        {
            case RoundPhase.Selecting:
                targetOverlayColor = selectingOverlayColor;
                ApplyMotionIntensity(0.45f);
                break;
            case RoundPhase.Responding:
                targetOverlayColor = respondingOverlayColor;
                ApplyMotionIntensity(0.60f);
                break;
            case RoundPhase.Discussion:
                targetOverlayColor = discussionOverlayColor;
                ApplyMotionIntensity(0.75f);
                break;
            case RoundPhase.RoundEnd:
                targetOverlayColor = new Color(0.68f, 0.76f, 0.88f, 0.08f);
                ApplyMotionIntensity(0.65f);
                break;
            case RoundPhase.ScoreGuessing:
                targetOverlayColor = scoreGuessingOverlayColor;
                ApplyMotionIntensity(0.50f);
                break;
            case RoundPhase.GameOver:
                targetOverlayColor = defaultOverlayColor;
                ApplyMotionIntensity(0.40f);
                break;
            default:
                SetMainMenuTone();
                break;
        }
    }

    public void SetMainMenuTone()
    {
        targetOverlayColor = defaultOverlayColor;
        ApplyMotionIntensity(0.35f);
    }

    public void SetIdentityTone(bool isVillain)
    {
        targetOverlayColor = isVillain
            ? new Color(0.78f, 0.28f, 0.24f, 0.10f)
            : new Color(0.55f, 0.82f, 0.45f, 0.08f);
        ApplyMotionIntensity(isVillain ? 0.55f : 0.42f);
    }

    public void SetResultTone(bool villainWin, bool isDraw)
    {
        targetOverlayColor = isDraw
            ? drawResultOverlayColor
            : villainWin ? villainResultOverlayColor : goodResultOverlayColor;
        ApplyMotionIntensity(0.40f);
    }

    public void ApplyMotionIntensity(float intensity)
    {
        targetIntensity = Mathf.Clamp01(intensity);
        for (int i = 0; i < ornaments.Count; i++)
        {
            if (ornaments[i] != null)
                ornaments[i].SetIntensity(targetIntensity);
        }
    }

    void UpdateTitleLayerMotion()
    {
        if (titleLayers.Count == 0) return;

        float duration = Mathf.Max(0.1f, titleLoopDuration);
        float normalizedTime = Mathf.Repeat(Time.unscaledTime, duration) / duration;
        float angleBase = normalizedTime * Mathf.PI * 2f;
        float scale = titleMotionScale * targetIntensity;

        for (int i = 0; i < titleLayers.Count; i++)
        {
            TitleLayerMotion layer = titleLayers[i];
            if (layer == null || layer.rectTransform == null) continue;

            if (layer.baseScale == Vector3.zero)
                CaptureTitleLayerBase(layer);

            float angle = angleBase + layer.phase;
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            layer.rectTransform.anchoredPosition = layer.basePosition + new Vector2(layer.drift.x * x, layer.drift.y * y) * scale;

            float pulse = Mathf.Sin(angle + Mathf.PI * 0.5f);
            float scalePulse = 1f + layer.scaleAmplitude * pulse * scale;
            layer.rectTransform.localScale = layer.baseScale * scalePulse;

            float rotation = layer.baseRotation + layer.rotationAmplitude * Mathf.Sin(angle + Mathf.PI * 0.25f) * scale;
            layer.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);

            if (layer.canvasGroup != null)
                layer.canvasGroup.alpha = Mathf.Clamp01(layer.baseAlpha + layer.alphaAmplitude * pulse * scale);
        }
    }

    void UpdatePatternMotion()
    {
        if (patternA != null)
        {
            float offset = Time.unscaledTime * patternScrollSpeedA * targetIntensity;
            patternA.anchoredPosition = patternABase + new Vector2(offset % 96f, 0f);
        }

        if (patternB != null)
        {
            float offset = Time.unscaledTime * patternScrollSpeedB * targetIntensity;
            patternB.anchoredPosition = patternBBase + new Vector2(0f, -(offset % 96f));
        }
    }

    void UpdateOverlayColor()
    {
        if (colorOverlay == null) return;
        colorOverlay.color = Color.Lerp(colorOverlay.color, targetOverlayColor, Time.unscaledDeltaTime * overlayLerpSpeed);
    }

    void CacheBasePositions()
    {
        if (patternA != null)
            patternABase = patternA.anchoredPosition;
        if (patternB != null)
            patternBBase = patternB.anchoredPosition;

        for (int i = 0; i < titleLayers.Count; i++)
            CaptureTitleLayerBase(titleLayers[i]);
    }

    void CaptureTitleLayerBase(TitleLayerMotion layer)
    {
        if (layer == null || layer.rectTransform == null) return;

        layer.basePosition = layer.rectTransform.anchoredPosition;
        layer.baseScale = layer.rectTransform.localScale;
        layer.baseRotation = layer.rectTransform.localEulerAngles.z;
        layer.baseAlpha = layer.canvasGroup != null ? layer.canvasGroup.alpha : 1f;
    }

    void DisableRaycasts(Transform root)
    {
        if (root == null) return;

        Graphic graphic = root.GetComponent<Graphic>();
        if (graphic != null)
            graphic.raycastTarget = false;

        CanvasGroup group = root.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        foreach (Transform child in root)
            DisableRaycasts(child);
    }
}
