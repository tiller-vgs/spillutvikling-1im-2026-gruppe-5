using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class EscHoldToQuit : MonoBehaviour
{
    private const float HoldToQuitSeconds = 3f;
    private const int OverlaySortOrder = 32767;
    private const string ExitText = "EXITING...";

    private GameObject _overlayRoot;
    private Image _background;
    private TextMeshProUGUI _text;
    private float _holdTime;
    private bool _isQuitting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<EscHoldToQuit>() != null)
        {
            return;
        }

        var runtimeObject = new GameObject("EscHoldToQuitRuntime");
        runtimeObject.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(runtimeObject);
        runtimeObject.AddComponent<EscHoldToQuit>();
    }

    private void Awake()
    {
        CreateOverlayIfNeeded();
        SetOverlayVisible(false);
    }

    private void Update()
    {
        if (_isQuitting)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.isPressed)
        {
            ResetHoldState();
            return;
        }

        CreateOverlayIfNeeded();
        _holdTime = Mathf.Min(HoldToQuitSeconds, _holdTime + Time.unscaledDeltaTime);
        SetOverlayVisible(true);
        UpdateOverlayVisuals();

        if (_holdTime >= HoldToQuitSeconds)
        {
            QuitGame();
        }
    }

    private void CreateOverlayIfNeeded()
    {
        if (_overlayRoot != null && _background != null && _text != null)
        {
            return;
        }

        var canvasObject = new GameObject("EscHoldToQuitCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = OverlaySortOrder;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        _overlayRoot = new GameObject("Overlay");
        _overlayRoot.transform.SetParent(canvasObject.transform, false);

        _background = _overlayRoot.AddComponent<Image>();
        _background.raycastTarget = false;
        _background.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform backgroundRect = _background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        var textObject = new GameObject("ExitText");
        textObject.transform.SetParent(_overlayRoot.transform, false);

        _text = textObject.AddComponent<TextMeshProUGUI>();
        _text.raycastTarget = false;
        _text.text = ExitText;
        _text.alignment = TextAlignmentOptions.Center;
        _text.fontStyle = FontStyles.Bold;
        _text.fontSize = 132f;
        _text.enableAutoSizing = true;
        _text.fontSizeMin = 48f;
        _text.fontSizeMax = 180f;
        _text.color = Color.white;

        if (TMP_Settings.defaultFontAsset != null)
        {
            _text.font = TMP_Settings.defaultFontAsset;
        }

        RectTransform textRect = _text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(96f, 96f);
        textRect.offsetMax = new Vector2(-96f, -96f);
    }

    private void UpdateOverlayVisuals()
    {
        float progress = Mathf.Clamp01(_holdTime / HoldToQuitSeconds);
        _background.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.72f, 0.9f, progress));
        _text.text = ExitText;
    }

    private void ResetHoldState()
    {
        if (_holdTime <= 0f)
        {
            return;
        }

        _holdTime = 0f;
        _isQuitting = false;
        SetOverlayVisible(false);
    }

    private void SetOverlayVisible(bool isVisible)
    {
        if (_overlayRoot == null || _overlayRoot.activeSelf == isVisible)
        {
            return;
        }

        _overlayRoot.SetActive(isVisible);
    }

    private void QuitGame()
    {
        _isQuitting = true;

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
