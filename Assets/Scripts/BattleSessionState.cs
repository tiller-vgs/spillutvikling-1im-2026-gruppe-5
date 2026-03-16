using UnityEngine;

public static class BattleSessionState
{
    public sealed class DragonBonesVisualState
    {
        public TextAsset sideSkeletonData;
        public TextAsset sideTextureAtlasData;
        public Texture2D sideTextureAtlasTexture;
        public string sideDragonBonesDataName = string.Empty;
        public string sideArmatureName = string.Empty;
        public string sideWalkingAnimationName = string.Empty;
        public string sideShootingAnimationName = string.Empty;
        public TextAsset frontSkeletonData;
        public TextAsset frontTextureAtlasData;
        public Texture2D frontTextureAtlasTexture;
        public string frontDragonBonesDataName = string.Empty;
        public string frontArmatureName = string.Empty;
        public string frontWalkingAnimationName = string.Empty;
        public TextAsset backSkeletonData;
        public TextAsset backTextureAtlasData;
        public Texture2D backTextureAtlasTexture;
        public string backDragonBonesDataName = string.Empty;
        public string backArmatureName = string.Empty;
        public string backWalkingAnimationName = string.Empty;
        public bool hideSourceSpriteRenderer = true;
        public Vector3 sideVisualOffset = Vector3.zero;
        public Vector3 sideVisualScale = Vector3.one;
        public Vector3 frontVisualOffset = Vector3.zero;
        public Vector3 frontVisualScale = Vector3.one;
        public Vector3 backVisualOffset = Vector3.zero;
        public Vector3 backVisualScale = Vector3.one;
        public int sortingOrderOffset;
        public float armatureScale = 0.01f;
        public float textureScale = 1f;
        public float moveThreshold = 0.05f;
    }

    public const string BattleSceneName = "Fight.turnbased";

    private const float DefaultHealth = 10f;

    public static bool HasPendingEncounter { get; private set; }
    public static bool EncounterInProgress { get; private set; }
    public static string ReturnSceneName { get; private set; } = string.Empty;
    public static string EnemyName { get; private set; } = string.Empty;
    public static float PlayerHealth { get; private set; } = DefaultHealth;
    public static float PlayerMaxHealth { get; private set; } = DefaultHealth;
    public static bool PlayerHasGun { get; private set; }
    public static float EnemyHealth { get; private set; } = DefaultHealth;
    public static float EnemyMaxHealth { get; private set; } = DefaultHealth;
    public static DragonBonesVisualState PlayerVisualState { get; private set; }
    public static DragonBonesVisualState EnemyVisualState { get; private set; }

    public static bool BeginEncounter(GameObject playerObject, GameObject enemyObject)
    {
        if (EncounterInProgress || playerObject == null || enemyObject == null)
        {
            return false;
        }

        ReturnSceneName = playerObject.scene.IsValid() ? playerObject.scene.name : string.Empty;
        EnemyName = enemyObject.name;
        ReadPlayerState(playerObject);
        ReadEnemyState(enemyObject);
        HasPendingEncounter = true;
        EncounterInProgress = true;
        return true;
    }

    public static void MarkEncounterLoaded()
    {
        HasPendingEncounter = false;
    }

    public static void NotifySceneLoaded(string sceneName)
    {
        if (!EncounterInProgress || HasPendingEncounter || sceneName == BattleSceneName)
        {
            return;
        }

        Clear();
    }

    public static void Clear()
    {
        HasPendingEncounter = false;
        EncounterInProgress = false;
        ReturnSceneName = string.Empty;
        EnemyName = string.Empty;
        PlayerHealth = DefaultHealth;
        PlayerMaxHealth = DefaultHealth;
        PlayerHasGun = false;
        EnemyHealth = DefaultHealth;
        EnemyMaxHealth = DefaultHealth;
        PlayerVisualState = null;
        EnemyVisualState = null;
    }

    private static void ReadPlayerState(GameObject playerObject)
    {
        var playerHealthHandler = playerObject.GetComponent<Health_handler>();
        if (playerHealthHandler != null)
        {
            PlayerMaxHealth = Mathf.Max(1f, playerHealthHandler.max_health);
            PlayerHealth = Mathf.Clamp(playerHealthHandler.health, 0f, PlayerMaxHealth);
        }
        else
        {
            PlayerHealth = DefaultHealth;
            PlayerMaxHealth = DefaultHealth;
        }

        var dragonBonesView = playerObject.GetComponent<DirectionalDragonBonesView>();
        PlayerHasGun = dragonBonesView != null && dragonBonesView.HasGun;
        PlayerVisualState = CaptureVisualState(dragonBonesView);
    }

    private static void ReadEnemyState(GameObject enemyObject)
    {
        var enemyHealthHandler = enemyObject.GetComponent<Health_handler>();
        if (enemyHealthHandler != null)
        {
            EnemyMaxHealth = Mathf.Max(1f, enemyHealthHandler.max_health);
            EnemyHealth = Mathf.Clamp(enemyHealthHandler.health, 0f, EnemyMaxHealth);
        }
        else
        {
            EnemyHealth = DefaultHealth;
            EnemyMaxHealth = DefaultHealth;
        }

        EnemyVisualState = CaptureVisualState(enemyObject.GetComponent<DirectionalDragonBonesView>());
    }

    private static DragonBonesVisualState CaptureVisualState(DirectionalDragonBonesView dragonBonesView)
    {
        if (dragonBonesView == null)
        {
            return null;
        }

        return new DragonBonesVisualState
        {
            sideSkeletonData = dragonBonesView.sideSkeletonData,
            sideTextureAtlasData = dragonBonesView.sideTextureAtlasData,
            sideTextureAtlasTexture = dragonBonesView.sideTextureAtlasTexture,
            sideDragonBonesDataName = dragonBonesView.sideDragonBonesDataName,
            sideArmatureName = dragonBonesView.sideArmatureName,
            sideWalkingAnimationName = dragonBonesView.sideWalkingAnimationName,
            sideShootingAnimationName = dragonBonesView.sideShootingAnimationName,
            frontSkeletonData = dragonBonesView.frontSkeletonData,
            frontTextureAtlasData = dragonBonesView.frontTextureAtlasData,
            frontTextureAtlasTexture = dragonBonesView.frontTextureAtlasTexture,
            frontDragonBonesDataName = dragonBonesView.frontDragonBonesDataName,
            frontArmatureName = dragonBonesView.frontArmatureName,
            frontWalkingAnimationName = dragonBonesView.frontWalkingAnimationName,
            backSkeletonData = dragonBonesView.backSkeletonData,
            backTextureAtlasData = dragonBonesView.backTextureAtlasData,
            backTextureAtlasTexture = dragonBonesView.backTextureAtlasTexture,
            backDragonBonesDataName = dragonBonesView.backDragonBonesDataName,
            backArmatureName = dragonBonesView.backArmatureName,
            backWalkingAnimationName = dragonBonesView.backWalkingAnimationName,
            hideSourceSpriteRenderer = dragonBonesView.hideSourceSpriteRenderer,
            sideVisualOffset = dragonBonesView.sideVisualOffset,
            sideVisualScale = dragonBonesView.sideVisualScale,
            frontVisualOffset = dragonBonesView.frontVisualOffset,
            frontVisualScale = dragonBonesView.frontVisualScale,
            backVisualOffset = dragonBonesView.backVisualOffset,
            backVisualScale = dragonBonesView.backVisualScale,
            sortingOrderOffset = dragonBonesView.sortingOrderOffset,
            armatureScale = dragonBonesView.armatureScale,
            textureScale = dragonBonesView.textureScale,
            moveThreshold = dragonBonesView.moveThreshold
        };
    }
}
