using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicBackgroundController : MonoBehaviour
{
    [Header("Layers")]
    public Image baseImage;
    public RectTransform patternA;
    public RectTransform patternB;
    public Image colorOverlay;
    public List<DynamicBackgroundElement> ornaments = new List<DynamicBackgroundElement>();

    [Header("Colors")]
    public Color defaultOverlayColor = new Color(0.03f, 0.06f, 0.10f, 0.14f);
    public Color selectingOverlayColor = new Color(0.04f, 0.10f, 0.14f, 0.16f);
    public Color respondingOverlayColor = new Color(0.18f, 0.05f, 0.05f, 0.18f);
    public Color discussionOverlayColor = new Color(0.07f, 0.06f, 0.20f, 0.18f);
    public Color scoreGuessingOverlayColor = new Color(0.18f, 0.14f, 0.05f, 0.16f);
    public Color goodResultOverlayColor = new Color(0.10f, 0.18f, 0.08f, 0.18f);
    public Color villainResultOverlayColor = new Color(0.22f, 0.04f, 0.04f, 0.20f);
    public Color drawResultOverlayColor = new Color(0.08f, 0.10f, 0.14f, 0.18f);

    [Header("Motion")]
    public float patternScrollSpeedA = 7f;
    public float patternScrollSpeedB = 4f;
    public float overlayLerpSpeed = 2.4f;

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
        UpdatePatternMotion();
        UpdateOverlayColor();
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
                targetOverlayColor = new Color(0.05f, 0.08f, 0.13f, 0.22f);
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
            ? new Color(0.22f, 0.04f, 0.04f, 0.20f)
            : new Color(0.11f, 0.17f, 0.08f, 0.16f);
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
