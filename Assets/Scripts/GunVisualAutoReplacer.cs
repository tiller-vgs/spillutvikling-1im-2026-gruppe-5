using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class GunVisualAutoReplacer : MonoBehaviour
{
    private const string GunObjectName = "Gun";
    private const string GunClonePrefix = "Gun(";
    private const string GunVisualObjectName = "GunVisual";
    private const string GunSpritePath = "Assets/Sprites/pistol.png";

    [SerializeField] private float scanIntervalSeconds = 0.5f;
    [SerializeField] private float pulseAmplitude = 0.06f;
    [SerializeField] private float pulseFrequency = 1.4f;

    private readonly Dictionary<int, GunVisualState> _gunStates = new Dictionary<int, GunVisualState>();

    private static GunVisualAutoReplacer _instance;
    private Sprite _gunSprite;
    private float _nextScanTime;
    private bool _missingSpriteWarningShown;

    private sealed class GunVisualState
    {
        public Transform rootTransform;
        public Transform visualTransform;
        public Vector3 baseVisualScale = Vector3.one;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GunVisualAutoReplacer>() != null)
        {
            return;
        }

        var runtimeObject = new GameObject("GunVisualAutoReplacer");
        runtimeObject.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(runtimeObject);
        runtimeObject.AddComponent<GunVisualAutoReplacer>();
    }

    private void Awake()
    {
        _instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ScanAllLoadedScenes();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void ConfigureRuntimeGun(GameObject gunObject)
    {
        if (_instance == null || gunObject == null)
        {
            return;
        }

        _instance.TryEnsureGunSprite();
        _instance.ConfigureGunIfNeeded(gunObject);
    }

    private void Update()
    {
        if (Time.unscaledTime >= _nextScanTime)
        {
            _nextScanTime = Time.unscaledTime + Mathf.Max(0.1f, scanIntervalSeconds);
            ScanAllLoadedScenes();
        }

        UpdatePulse();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        ScanScene(scene);
    }

    private void ScanAllLoadedScenes()
    {
        TryEnsureGunSprite();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            ScanScene(SceneManager.GetSceneAt(i));
        }
    }

    private void ScanScene(Scene scene)
    {
        if (!scene.isLoaded)
        {
            return;
        }

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            ScanHierarchy(rootObject.transform);
        }
    }

    private void ScanHierarchy(Transform current)
    {
        ConfigureGunIfNeeded(current.gameObject);

        for (int i = 0; i < current.childCount; i++)
        {
            ScanHierarchy(current.GetChild(i));
        }
    }

    private void ConfigureGunIfNeeded(GameObject gunObject)
    {
        if (!IsGunObject(gunObject))
        {
            return;
        }

        Transform visualTransform = gunObject.transform.Find(GunVisualObjectName);
        if (visualTransform == null)
        {
            var visualObject = new GameObject(GunVisualObjectName);
            visualTransform = visualObject.transform;
            visualTransform.SetParent(gunObject.transform, false);
        }

        int gunId = gunObject.GetInstanceID();
        if (!_gunStates.TryGetValue(gunId, out GunVisualState gunState) || gunState == null)
        {
            gunState = new GunVisualState();
            _gunStates[gunId] = gunState;
        }

        gunState.rootTransform = gunObject.transform;
        gunState.visualTransform = visualTransform;

        var rootSpriteRenderer = gunObject.GetComponent<SpriteRenderer>();
        if (rootSpriteRenderer != null)
        {
            rootSpriteRenderer.enabled = false;
        }

        var spriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (_gunSprite != null)
        {
            spriteRenderer.sprite = _gunSprite;
        }

        spriteRenderer.color = Color.white;
        if (rootSpriteRenderer != null)
        {
            spriteRenderer.sortingLayerID = rootSpriteRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = rootSpriteRenderer.sortingOrder;
        }

        visualTransform.gameObject.layer = gunObject.layer;
        UpdateGunVisualTransform(gunState, 1f);
    }

    private void UpdatePulse()
    {
        if (_gunStates.Count == 0)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseFrequency * Mathf.PI * 2f) * pulseAmplitude;
        List<int> missingGunIds = null;

        foreach (KeyValuePair<int, GunVisualState> pair in _gunStates)
        {
            GunVisualState gunState = pair.Value;
            if (gunState == null || gunState.rootTransform == null || gunState.visualTransform == null)
            {
                missingGunIds ??= new List<int>();
                missingGunIds.Add(pair.Key);
                continue;
            }

            Transform gunTransform = gunState.rootTransform;
            if (!gunTransform.gameObject.activeInHierarchy)
            {
                UpdateGunVisualTransform(gunState, 1f);
                continue;
            }

            UpdateGunVisualTransform(gunState, pulse);
        }

        if (missingGunIds == null)
        {
            return;
        }

        for (int i = 0; i < missingGunIds.Count; i++)
        {
            _gunStates.Remove(missingGunIds[i]);
        }
    }

    private static void UpdateGunVisualTransform(GunVisualState gunState, float pulse)
    {
        if (gunState == null || gunState.rootTransform == null || gunState.visualTransform == null)
        {
            return;
        }

        Vector3 parentScale = gunState.rootTransform.lossyScale;
        gunState.visualTransform.localPosition = Vector3.zero;
        gunState.visualTransform.localScale = new Vector3(
            gunState.baseVisualScale.x * pulse / GetSafeScaleComponent(parentScale.x),
            gunState.baseVisualScale.y * pulse / GetSafeScaleComponent(parentScale.y),
            gunState.baseVisualScale.z / GetSafeScaleComponent(parentScale.z)
        );
    }

    private static float GetSafeScaleComponent(float scaleComponent)
    {
        return Mathf.Abs(scaleComponent) <= 0.0001f ? 1f : scaleComponent;
    }

    private bool TryEnsureGunSprite()
    {
        if (_gunSprite != null)
        {
            return true;
        }

#if UNITY_EDITOR
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(GunSpritePath);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite sprite)
            {
                _gunSprite = sprite;
                break;
            }
        }
#endif

        bool isLoaded = _gunSprite != null;
        if (!isLoaded && !_missingSpriteWarningShown)
        {
            _missingSpriteWarningShown = true;
            Debug.LogWarning("GunVisualAutoReplacer could not load pistol.png automatically.", this);
        }

        return isLoaded;
    }

    private static bool IsGunObject(GameObject gameObject)
    {
        return gameObject.name == GunObjectName || gameObject.name.StartsWith(GunClonePrefix);
    }
}
