using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class EnemyVisualAutoReplacer : MonoBehaviour
{
    private static readonly Regex EnemyNameRegex = new Regex(
        @"^Enemy_\d+$",
        RegexOptions.Compiled
    );

    private const string SkeletonAssetPath = "Assets/Characters/Enemy1/enemy1_ske.json";
    private const string TextureAtlasAssetPath = "Assets/Characters/Enemy1/enemy1_tex.json";
    private const string TextureAssetPath = "Assets/Characters/Enemy1/enemy1_tex.png";

    [SerializeField] private float scanIntervalSeconds = 0.5f;
    [SerializeField] private string dragonBonesDataName = "enemy1_ske";
    [SerializeField] private string armatureName = "armature1";
    [SerializeField] private string walkingAnimationName = "walkingAnimation";
    [SerializeField] private Vector3 visualOffset = Vector3.zero;
    [SerializeField] private Vector3 visualScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float armatureScale = 0.01f;
    [SerializeField] private float textureScale = 1f;
    [SerializeField] private float moveThreshold = 0.05f;

    private readonly HashSet<int> _configuredEnemyIds = new HashSet<int>();

    private TextAsset _skeletonData;
    private TextAsset _textureAtlasData;
    private Texture2D _textureAtlasTexture;
    private float _nextScanTime;
    private bool _missingAssetsWarningShown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<EnemyVisualAutoReplacer>() != null)
        {
            return;
        }

        var runtimeObject = new GameObject("EnemyVisualAutoReplacer");
        runtimeObject.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(runtimeObject);
        runtimeObject.AddComponent<EnemyVisualAutoReplacer>();
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
        if (Time.unscaledTime < _nextScanTime)
        {
            return;
        }

        _nextScanTime = Time.unscaledTime + Mathf.Max(0.1f, scanIntervalSeconds);
        ScanAllLoadedScenes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        ScanScene(scene);
    }

    private void ScanAllLoadedScenes()
    {
        if (!TryEnsureEnemyAssets())
        {
            return;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            ScanScene(SceneManager.GetSceneAt(i));
        }
    }

    private void ScanScene(Scene scene)
    {
        if (!scene.isLoaded || !TryEnsureEnemyAssets())
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
        ConfigureEnemyIfNeeded(current.gameObject);

        for (int i = 0; i < current.childCount; i++)
        {
            ScanHierarchy(current.GetChild(i));
        }
    }

    private void ConfigureEnemyIfNeeded(GameObject enemyObject)
    {
        if (!EnemyNameRegex.IsMatch(enemyObject.name))
        {
            return;
        }

        int enemyId = enemyObject.GetInstanceID();
        if (_configuredEnemyIds.Contains(enemyId))
        {
            return;
        }

        var rigidbody = enemyObject.GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = enemyObject.AddComponent<Rigidbody2D>();
        }

        rigidbody.gravityScale = 0f;
        rigidbody.linearVelocity = Vector2.zero;
        rigidbody.angularVelocity = 0f;
        rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;

        var walker = enemyObject.GetComponent<EnemyWalker>();
        if (walker != null)
        {
            Destroy(walker);
        }

        var view = enemyObject.GetComponent<DirectionalDragonBonesView>();
        if (view == null)
        {
            view = enemyObject.AddComponent<DirectionalDragonBonesView>();
        }

        view.sideSkeletonData = _skeletonData;
        view.sideTextureAtlasData = _textureAtlasData;
        view.sideTextureAtlasTexture = _textureAtlasTexture;
        view.sideDragonBonesDataName = dragonBonesDataName;
        view.sideArmatureName = armatureName;
        view.sideWalkingAnimationName = walkingAnimationName;
        view.sideVisualOffset = visualOffset;
        view.sideVisualScale = visualScale;
        view.frontSkeletonData = null;
        view.frontTextureAtlasData = null;
        view.frontTextureAtlasTexture = null;
        view.frontDragonBonesDataName = string.Empty;
        view.frontArmatureName = string.Empty;
        view.frontWalkingAnimationName = string.Empty;
        view.backSkeletonData = null;
        view.backTextureAtlasData = null;
        view.backTextureAtlasTexture = null;
        view.backDragonBonesDataName = string.Empty;
        view.backArmatureName = string.Empty;
        view.backWalkingAnimationName = string.Empty;
        view.hideSourceSpriteRenderer = true;
        view.armatureScale = armatureScale;
        view.textureScale = textureScale;
        view.moveThreshold = moveThreshold;
        view.RefreshView();

        _configuredEnemyIds.Add(enemyId);
    }

    private bool TryEnsureEnemyAssets()
    {
        if (_skeletonData != null &&
            _textureAtlasData != null &&
            _textureAtlasTexture != null)
        {
            return true;
        }

#if UNITY_EDITOR
        _skeletonData ??= AssetDatabase.LoadAssetAtPath<TextAsset>(SkeletonAssetPath);
        _textureAtlasData ??= AssetDatabase.LoadAssetAtPath<TextAsset>(TextureAtlasAssetPath);
        _textureAtlasTexture ??= AssetDatabase.LoadAssetAtPath<Texture2D>(TextureAssetPath);
#endif

        bool assetsLoaded = _skeletonData != null &&
                            _textureAtlasData != null &&
                            _textureAtlasTexture != null;

        if (!assetsLoaded && !_missingAssetsWarningShown)
        {
            _missingAssetsWarningShown = true;
            Debug.LogWarning(
                "EnemyVisualAutoReplacer could not load Enemy1 DragonBones assets automatically. " +
                "This replacement currently relies on editor asset loading.",
                this
            );
        }

        return assetsLoaded;
    }
}
