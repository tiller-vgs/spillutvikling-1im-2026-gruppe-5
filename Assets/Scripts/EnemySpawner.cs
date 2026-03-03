using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private string spawnAreaObjectName = "Spawner";
    [SerializeField] private SpriteRenderer spawnAreaRenderer;
    [SerializeField] private float spawnIntervalSeconds = 5f;
    [SerializeField] private float horizontalSpawnOffset = 0f;
    [SerializeField] private bool useRightEdge = false;
    [SerializeField] private Transform spawnedEnemiesParent;

    [Header("Enemy Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector3 spawnScale = Vector3.one;

    [Header("Enemy Visual")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color spriteColor = Color.white;

    [Header("Enemy DragonBones")]
    [SerializeField] private TextAsset sideSkeletonData;
    [SerializeField] private TextAsset sideTextureAtlasData;
    [SerializeField] private Texture2D sideTextureAtlasTexture;
    [SerializeField] private string sideDragonBonesDataName = string.Empty;
    [SerializeField] private string sideArmatureName = "armature1";
    [SerializeField] private string sideWalkingAnimationName = "walkingAnimation";
    [SerializeField] private Vector3 sideVisualOffset = Vector3.zero;
    [SerializeField] private Vector3 sideVisualScale = new Vector3(-1f, 1f, 1f);
    [SerializeField] private float armatureScale = 0.01f;
    [SerializeField] private float textureScale = 1f;
    [SerializeField] private float moveThreshold = 0.05f;

    private int _spawnedEnemiesCount;

    private void Awake()
    {
        ResolveSpawnAreaRenderer();
    }

    private IEnumerator Start()
    {
        if (!ResolveSpawnAreaRenderer())
        {
            yield break;
        }

        float waitSeconds = Mathf.Max(0.01f, spawnIntervalSeconds);
        var wait = new WaitForSeconds(waitSeconds);

        while (true)
        {
            yield return wait;
            SpawnEnemy();
        }
    }

    private bool ResolveSpawnAreaRenderer()
    {
        if (spawnAreaRenderer != null)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(spawnAreaObjectName))
        {
            GameObject spawnAreaObject = GameObject.Find(spawnAreaObjectName);
            if (spawnAreaObject != null)
            {
                spawnAreaRenderer = spawnAreaObject.GetComponent<SpriteRenderer>();
            }
        }

        if (spawnAreaRenderer == null)
        {
            spawnAreaRenderer = GetComponent<SpriteRenderer>();
        }

        if (spawnAreaRenderer != null)
        {
            return true;
        }

        Debug.LogError("EnemySpawner could not find a SpriteRenderer for the spawn area.", this);
        return false;
    }

    private void SpawnEnemy()
    {
        Bounds spawnBounds = spawnAreaRenderer.bounds;
        float spawnX = useRightEdge ? spawnBounds.max.x : spawnBounds.center.x;
        float spawnY = Random.Range(spawnBounds.min.y, spawnBounds.max.y);
        Vector3 spawnPosition = new Vector3(
            spawnX + horizontalSpawnOffset,
            spawnY,
            spawnAreaRenderer.transform.position.z
        );

        GameObject enemy = new GameObject($"Enemy1_{++_spawnedEnemiesCount}");
        if (spawnedEnemiesParent != null)
        {
            enemy.transform.SetParent(spawnedEnemiesParent, false);
        }

        enemy.transform.position = spawnPosition;
        enemy.transform.localScale = spawnScale;

        SpriteRenderer enemySpriteRenderer = enemy.AddComponent<SpriteRenderer>();
        enemySpriteRenderer.sprite = sprite;
        enemySpriteRenderer.color = spriteColor;
        enemySpriteRenderer.sortingLayerID = spawnAreaRenderer.sortingLayerID;
        enemySpriteRenderer.sortingOrder = spawnAreaRenderer.sortingOrder;
        enemySpriteRenderer.enabled = false;

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
        body.linearVelocity = Vector2.left * moveSpeed;

        EnemyWalker walker = enemy.AddComponent<EnemyWalker>();
        walker.SetMoveSpeed(moveSpeed);

        DirectionalDragonBonesView view = enemy.AddComponent<DirectionalDragonBonesView>();
        view.sideSkeletonData = sideSkeletonData;
        view.sideTextureAtlasData = sideTextureAtlasData;
        view.sideTextureAtlasTexture = sideTextureAtlasTexture;
        view.sideDragonBonesDataName = sideDragonBonesDataName;
        view.sideArmatureName = sideArmatureName;
        view.sideWalkingAnimationName = sideWalkingAnimationName;
        view.sideVisualOffset = sideVisualOffset;
        view.sideVisualScale = sideVisualScale;
        view.armatureScale = armatureScale;
        view.textureScale = textureScale;
        view.moveThreshold = moveThreshold;
        view.hideSourceSpriteRenderer = true;
        view.RefreshView();
    }
}
