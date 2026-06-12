using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[ExecuteAlways]
public class MainMenuTextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public RectTransform target;
    public TextMeshProUGUI label;
    public TextMeshProUGUI indicator;

    public Color normalColor = new Color(0.94f, 0.95f, 0.88f, 1f);
    public Color hoverColor = new Color(1f, 0.86f, 0.34f, 1f);
    public Color indicatorColor = new Color(1f, 0.86f, 0.34f, 1f);
    public Vector2 hoverOffset = new Vector2(28f, 0f);
    public float hoverScale = 1.12f;
    public float speed = 16f;

    Vector2 basePosition;
    Vector3 baseScale;
    bool active;
    bool pointerInside;
    bool selected;
    bool initialized;
    float ignorePointerUntil;
    Shadow labelShadow;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        Initialize();
        ApplyImmediate(false);
    }

    void Update()
    {
        if (!initialized)
            Initialize();

        bool canReadPointer = Time.unscaledTime >= ignorePointerUntil;
        bool shouldBeActive = selected || (canReadPointer && IsPointerInside());
        if (active != shouldBeActive)
            active = shouldBeActive;

        Vector2 desiredPosition = basePosition + (active ? hoverOffset : Vector2.zero);
        Vector3 desiredScale = baseScale * (active ? hoverScale : 1f);
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);

        if (target != null)
        {
            target.anchoredPosition = Vector2.Lerp(target.anchoredPosition, desiredPosition, t);
            target.localScale = Vector3.Lerp(target.localScale, desiredScale, t);
        }

        if (label != null)
            label.color = Color.Lerp(label.color, active ? hoverColor : normalColor, t);

        if (indicator != null)
        {
            indicator.enabled = true;
            indicator.color = Color.Lerp(indicator.color, active ? indicatorColor : WithAlpha(indicatorColor, 0f), t);
        }

        if (labelShadow != null)
        {
            labelShadow.effectDistance = Vector2.Lerp(labelShadow.effectDistance, active ? new Vector2(3f, -3f) : new Vector2(1.8f, -1.8f), t);
            labelShadow.effectColor = Color.Lerp(labelShadow.effectColor, active ? new Color(0f, 0f, 0f, 0.92f) : new Color(0.02f, 0.04f, 0.04f, 0.68f), t);
        }
    }

    public void Configure(RectTransform targetRect, TextMeshProUGUI labelText, Color normal, Color hover, Color indicatorTint)
    {
        target = targetRect;
        label = labelText;
        normalColor = normal;
        hoverColor = hover;
        indicatorColor = indicatorTint;
        initialized = false;
        Initialize();
        ResetVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    void OnDisable()
    {
        ResetVisualState();
    }

    public void ResetVisualState()
    {
        pointerInside = false;
        selected = false;
        ignorePointerUntil = Time.unscaledTime + 0.15f;
        ApplyImmediate(false);
    }

    bool IsPointerInside()
    {
        if (!initialized)
            Initialize();

        if (target == null)
            return pointerInside;

        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera camera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            camera = canvas.worldCamera;

        if (!TryGetPointerPosition(out Vector2 pointerPosition))
            return pointerInside;

        return RectTransformUtility.RectangleContainsScreenPoint(target, pointerPosition, camera);
    }

    static bool TryGetPointerPosition(out Vector2 pointerPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            pointerPosition = Mouse.current.position.ReadValue();
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pointerPosition = Input.mousePosition;
        return true;
#else
        pointerPosition = Vector2.zero;
        return false;
#endif
    }

    void Initialize()
    {
        if (target == null)
            target = transform as RectTransform;

        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (target == null || label == null)
            return;

        basePosition = target.anchoredPosition;
        baseScale = target.localScale == Vector3.zero ? Vector3.one : target.localScale;
        labelShadow = label.GetComponent<Shadow>();
        EnsureIndicator();
        initialized = true;
    }

    void EnsureIndicator()
    {
        if (label == null)
            return;

        if (indicator == null)
        {
            Transform existing = transform.Find("HoverIndicator");
            if (existing != null)
                indicator = existing.GetComponent<TextMeshProUGUI>();
        }

        if (indicator == null)
        {
            GameObject indicatorObject = new GameObject("HoverIndicator", typeof(RectTransform), typeof(TextMeshProUGUI));
            indicatorObject.transform.SetParent(transform, false);
            indicator = indicatorObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform indicatorRect = indicator.rectTransform;
        indicatorRect.anchorMin = new Vector2(0f, 0.5f);
        indicatorRect.anchorMax = new Vector2(0f, 0.5f);
        indicatorRect.pivot = new Vector2(0f, 0.5f);
        indicatorRect.anchoredPosition = new Vector2(4f, 0f);
        indicatorRect.sizeDelta = new Vector2(34f, 52f);

        indicator.text = ">";
        indicator.fontSize = label.fontSize + 4f;
        indicator.alignment = TextAlignmentOptions.MidlineLeft;
        indicator.raycastTarget = false;
        indicator.enabled = true;
        indicator.color = WithAlpha(indicatorColor, active ? indicatorColor.a : 0f);

        if (label.font != null)
            indicator.font = label.font;
        if (label.fontSharedMaterial != null)
            indicator.fontSharedMaterial = label.fontSharedMaterial;
    }

    void ApplyImmediate(bool hovered)
    {
        active = hovered;

        if (target != null)
        {
            target.anchoredPosition = basePosition + (hovered ? hoverOffset : Vector2.zero);
            target.localScale = baseScale * (hovered ? hoverScale : 1f);
        }

        if (label != null)
            label.color = hovered ? hoverColor : normalColor;

        if (indicator != null)
            indicator.color = hovered ? indicatorColor : WithAlpha(indicatorColor, 0f);
    }

    static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
