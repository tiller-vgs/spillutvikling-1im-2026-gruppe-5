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
    private const string GunSpritePath = "Assets/Sprites/pistol.png";

    [SerializeField] private float scanIntervalSeconds = 0.5f;
    [SerializeField] private float pulseAmplitude = 0.06f;
    [SerializeField] private float pulseFrequency = 1.4f;

    private readonly Dictionary<int, GunVisualState> _gunStates = new Dictionary<int, GunVisualState>();

    private Sprite _gunSprite;
    private float _nextScanTime;
    private bool _missingSpriteWarningShown;

    private sealed class GunVisualState
    {
        public Transform transform;
        public Vector3 baseScale;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        ScanAllLoadedScenes();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

        int gunId = gunObject.GetInstanceID();
        if (!_gunStates.TryGetValue(gunId, out GunVisualState gunState) || gunState == null)
        {
            gunState = new GunVisualState
            {
                transform = gunObject.transform,
                baseScale = gunObject.transform.localScale
            };
            _gunStates[gunId] = gunState;
        }

        var spriteRenderer = gunObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gunObject.AddComponent<SpriteRenderer>();
        }

        if (_gunSprite != null)
        {
            spriteRenderer.sprite = _gunSprite;
        }

        spriteRenderer.color = Color.white;
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
            if (gunState == null || gunState.transform == null)
            {
                missingGunIds ??= new List<int>();
                missingGunIds.Add(pair.Key);
                continue;
            }

            Transform gunTransform = gunState.transform;
            if (!gunTransform.gameObject.activeInHierarchy)
            {
                gunTransform.localScale = gunState.baseScale;
                continue;
            }

            gunTransform.localScale = new Vector3(
                gunState.baseScale.x * pulse,
                gunState.baseScale.y * pulse,
                gunState.baseScale.z
            );
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
