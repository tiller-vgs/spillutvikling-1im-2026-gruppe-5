using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class BattleEncounterRuntimeBootstrap : MonoBehaviour
{
    private static readonly Regex EnemyNameRegex = new Regex(
        @"^Enemy_\d+$",
        RegexOptions.Compiled
    );
    private const bool BattlePlayerFacesLeft = true;
    private const bool BattleEnemyFacesLeft = false;

    [SerializeField] private float scanIntervalSeconds = 0.5f;

    private float _nextScanTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<BattleEncounterRuntimeBootstrap>() != null)
        {
            return;
        }

        var runtimeObject = new GameObject("BattleEncounterRuntimeBootstrap");
        runtimeObject.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(runtimeObject);
        runtimeObject.AddComponent<BattleEncounterRuntimeBootstrap>();
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
        HandleScene(scene);
    }

    private void ScanAllLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            HandleScene(SceneManager.GetSceneAt(i));
        }
    }

    private void HandleScene(Scene scene)
    {
        if (!scene.isLoaded)
        {
            return;
        }

        if (scene.name == BattleSessionState.BattleSceneName)
        {
            ApplyEncounterToBattleScene(scene);
            return;
        }

        BattleSessionState.NotifySceneLoaded(scene.name);
        ConfigureEncounterTriggers(scene);
    }

    private static void ConfigureEncounterTriggers(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            ConfigureEncounterTriggersInHierarchy(rootObject.transform);
        }
    }

    private static void ConfigureEncounterTriggersInHierarchy(Transform current)
    {
        if (EnemyNameRegex.IsMatch(current.gameObject.name) &&
            current.GetComponent<EnemyEncounterTrigger>() == null)
        {
            current.gameObject.AddComponent<EnemyEncounterTrigger>();
        }

        for (int i = 0; i < current.childCount; i++)
        {
            ConfigureEncounterTriggersInHierarchy(current.GetChild(i));
        }
    }

    private static void ApplyEncounterToBattleScene(Scene scene)
    {
        if (!BattleSessionState.HasPendingEncounter)
        {
            return;
        }

        battle_handler battle = FindComponentInScene<battle_handler>(scene);
        if (battle == null)
        {
            BattleSessionState.MarkEncounterLoaded();
            return;
        }

        ApplyPlayerState(battle.player);
        ApplyEnemyState(battle.enemies);
        battle.player_turn = true;
        battle.GetSelectedEnemy(0);
        battle.show_options();
        BattleSessionState.MarkEncounterLoaded();
    }

    private static void ApplyPlayerState(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return;
        }

        ApplyHealthState(
            playerObject.GetComponent<Health_handler>(),
            BattleSessionState.PlayerHealth,
            BattleSessionState.PlayerMaxHealth
        );
        ApplyDragonBonesVisual(
            playerObject,
            BattleSessionState.PlayerVisualState,
            BattlePlayerFacesLeft,
            BattleSessionState.PlayerHasGun
        );
    }

    private static void ApplyEnemyState(GameObject enemiesRoot)
    {
        if (enemiesRoot == null)
        {
            return;
        }

        GameObject enemyObject = FindBattleEnemy(enemiesRoot.transform);
        if (enemyObject == null)
        {
            return;
        }

        enemyObject.SetActive(true);
        ApplyHealthState(
            enemyObject.GetComponent<Health_handler>(),
            BattleSessionState.EnemyHealth,
            BattleSessionState.EnemyMaxHealth
        );
        ApplyDragonBonesVisual(
            enemyObject,
            BattleSessionState.EnemyVisualState,
            BattleEnemyFacesLeft,
            false
        );
    }

    private static void ApplyHealthState(Health_handler healthHandler, float health, float maxHealth)
    {
        if (healthHandler == null)
        {
            return;
        }

        healthHandler.max_health = Mathf.Max(1f, maxHealth);
        healthHandler.health = Mathf.Clamp(health, 0f, healthHandler.max_health);
        if (healthHandler.health_counter == null)
        {
            return;
        }

        var counter = healthHandler.health_counter.GetComponent<set_health>();
        if (counter != null)
        {
            counter.setHealth(healthHandler.health, healthHandler.max_health);
        }
    }

    private static GameObject FindBattleEnemy(Transform enemiesRoot)
    {
        if (enemiesRoot == null)
        {
            return null;
        }

        Transform namedEnemy = enemiesRoot.Find("enemy_0");
        if (namedEnemy != null)
        {
            return namedEnemy.gameObject;
        }

        return enemiesRoot.childCount > 0 ? enemiesRoot.GetChild(0).gameObject : null;
    }

    private static void ApplyDragonBonesVisual(
        GameObject targetObject,
        BattleSessionState.DragonBonesVisualState visualState,
        bool faceLeft,
        bool hasGun)
    {
        if (targetObject == null || visualState == null)
        {
            return;
        }

        Rigidbody2D rigidbody = targetObject.GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = targetObject.AddComponent<Rigidbody2D>();
        }

        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;
        rigidbody.linearVelocity = Vector2.zero;
        rigidbody.angularVelocity = 0f;
        rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;

        DirectionalDragonBonesView view = targetObject.GetComponent<DirectionalDragonBonesView>();
        if (view == null)
        {
            view = targetObject.AddComponent<DirectionalDragonBonesView>();
        }

        ApplyVisualStateToView(view, visualState, faceLeft);
        view.RefreshView();
        if (hasGun)
        {
            view.EquipGun();
        }
        else if (view.HasGun)
        {
            view.UnequipGun();
        }
    }

    private static void ApplyVisualStateToView(
        DirectionalDragonBonesView view,
        BattleSessionState.DragonBonesVisualState visualState,
        bool faceLeft)
    {
        view.sideSkeletonData = visualState.sideSkeletonData;
        view.sideTextureAtlasData = visualState.sideTextureAtlasData;
        view.sideTextureAtlasTexture = visualState.sideTextureAtlasTexture;
        view.sideDragonBonesDataName = visualState.sideDragonBonesDataName;
        view.sideArmatureName = visualState.sideArmatureName;
        view.sideWalkingAnimationName = visualState.sideWalkingAnimationName;
        view.sideShootingAnimationName = visualState.sideShootingAnimationName;
        view.frontSkeletonData = visualState.frontSkeletonData;
        view.frontTextureAtlasData = visualState.frontTextureAtlasData;
        view.frontTextureAtlasTexture = visualState.frontTextureAtlasTexture;
        view.frontDragonBonesDataName = visualState.frontDragonBonesDataName;
        view.frontArmatureName = visualState.frontArmatureName;
        view.frontWalkingAnimationName = visualState.frontWalkingAnimationName;
        view.backSkeletonData = visualState.backSkeletonData;
        view.backTextureAtlasData = visualState.backTextureAtlasData;
        view.backTextureAtlasTexture = visualState.backTextureAtlasTexture;
        view.backDragonBonesDataName = visualState.backDragonBonesDataName;
        view.backArmatureName = visualState.backArmatureName;
        view.backWalkingAnimationName = visualState.backWalkingAnimationName;
        view.hideSourceSpriteRenderer = visualState.hideSourceSpriteRenderer;
        view.sideVisualOffset = visualState.sideVisualOffset;
        view.sideVisualScale = new Vector3(
            GetFacingScaleX(visualState.sideVisualScale.x, faceLeft),
            visualState.sideVisualScale.y,
            visualState.sideVisualScale.z
        );
        view.frontVisualOffset = visualState.frontVisualOffset;
        view.frontVisualScale = visualState.frontVisualScale;
        view.backVisualOffset = visualState.backVisualOffset;
        view.backVisualScale = visualState.backVisualScale;
        view.sortingOrderOffset = visualState.sortingOrderOffset;
        view.armatureScale = visualState.armatureScale;
        view.textureScale = visualState.textureScale;
        view.moveThreshold = visualState.moveThreshold;
    }

    private static float GetFacingScaleX(float baseScaleX, bool faceLeft)
    {
        float absoluteScale = Mathf.Abs(baseScaleX);
        if (absoluteScale <= 0.0001f)
        {
            absoluteScale = 1f;
        }

        return faceLeft ? -absoluteScale : absoluteScale;
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            T component = rootObject.GetComponentInChildren<T>(true);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }
}
