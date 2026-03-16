using System.Collections;
using System.Globalization;
using UnityEngine;

public class PoliceCarAfterGunPickup : MonoBehaviour
{
    private const float WaitAfterGunPickup = 5f;
    private const float MoveDuration = 3f;
    private const float WaitAfterSpawn = 1f;
    private const float WaitAfterTurn = 1f;
    private const float TargetX = 6.86f;
    private const float BobAmplitude = 0.1f;
    private const float BobFrequency = 6f;

    private Player_controller _player;
    private bool _started;

    private void Start()
    {
        if (OverworldStoryState.IsCompleted)
        {
            enabled = false;
            return;
        }

        StartCoroutine(WaitForGunPickup());
    }

    private IEnumerator WaitForGunPickup()
    {
        while (!PlayerHasGun())
        {
            yield return null;
        }

        if (_started)
        {
            yield break;
        }

        _started = true;
        yield return new WaitForSeconds(WaitAfterGunPickup);
        yield return MoveCar();
        GameObject enemy = SpawnEnemy();
        if (enemy == null)
        {
            yield break;
        }

        SpawnedEnemyChasePlayer chase = enemy.GetComponent<SpawnedEnemyChasePlayer>();
        if (chase != null)
        {
            chase.FacePlayer();
        }

        yield return new WaitForSeconds(WaitAfterSpawn);

        yield return new WaitForSeconds(WaitAfterTurn);

        EnemyEncounterTrigger trigger = enemy.GetComponent<EnemyEncounterTrigger>();
        if (trigger != null)
        {
            trigger.enabled = true;
        }

        if (chase != null)
        {
            chase.FacePlayer();
            chase.StartChasing();
        }
    }

    private IEnumerator MoveCar()
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < MoveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / MoveDuration);
            float x = Mathf.Lerp(startPosition.x, TargetX, progress);
            float y = startPosition.y + Mathf.Sin(progress * Mathf.PI * 2f * BobFrequency) * BobAmplitude;
            transform.position = new Vector3(x, y, startPosition.z);
            yield return null;
        }

        transform.position = new Vector3(TargetX, startPosition.y, startPosition.z);
    }

    private GameObject SpawnEnemy()
    {
        GameObject enemy = new GameObject(GetNextEnemyName());
        enemy.transform.position = GetEnemySpawnPosition();

        SpriteRenderer policeCarSprite = GetComponent<SpriteRenderer>();
        SpriteRenderer enemySprite = enemy.AddComponent<SpriteRenderer>();
        if (policeCarSprite != null)
        {
            enemySprite.sortingLayerID = policeCarSprite.sortingLayerID;
            enemySprite.sortingOrder = policeCarSprite.sortingOrder + 1;
        }

        Rigidbody2D enemyBody = enemy.AddComponent<Rigidbody2D>();
        enemyBody.bodyType = RigidbodyType2D.Kinematic;
        enemyBody.gravityScale = 0f;
        enemyBody.constraints |= RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D enemyCollider = enemy.AddComponent<BoxCollider2D>();
        enemyCollider.isTrigger = false;
        enemyCollider.size = new Vector2(0.65f, 0.9f);

        EnemyEncounterTrigger trigger = enemy.AddComponent<EnemyEncounterTrigger>();
        trigger.enabled = false;

        enemy.AddComponent<SpawnedEnemyChasePlayer>();
        return enemy;
    }

    private Vector3 GetEnemySpawnPosition()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return transform.position;
        }

        Bounds bounds = spriteRenderer.bounds;
        return new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
    }

    private string GetNextEnemyName()
    {
        int highestNumber = 0;
        Transform[] sceneTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

        for (int i = 0; i < sceneTransforms.Length; i++)
        {
            Transform current = sceneTransforms[i];
            if (current == null)
            {
                continue;
            }

            string objectName = current.name;
            if (!objectName.StartsWith("Enemy_"))
            {
                continue;
            }

            string numberPart = objectName.Substring("Enemy_".Length);
            if (!int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out int enemyNumber))
            {
                continue;
            }

            if (enemyNumber > highestNumber)
            {
                highestNumber = enemyNumber;
            }
        }

        return "Enemy_" + (highestNumber + 1).ToString(CultureInfo.InvariantCulture);
    }

    private bool PlayerHasGun()
    {
        if (_player == null)
        {
            _player = FindObjectOfType<Player_controller>();
        }

        if (_player == null)
        {
            return false;
        }

        DirectionalDragonBonesView view = _player.GetComponent<DirectionalDragonBonesView>();
        return view != null && view.HasGun;
    }
}
