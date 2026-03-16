using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class EnemyEncounterTrigger : MonoBehaviour
{
    [SerializeField] private float screenDistanceFraction = 0.5f;
    [SerializeField] private float minimumTriggerDistance = 1.5f;

    private bool _hasTriggered;
    private Player_controller _playerController;
    private Collider2D _enemyCollider;

    private void Awake()
    {
        _enemyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (_hasTriggered)
        {
            return;
        }

        if (_playerController == null)
        {
            _playerController = FindFirstObjectByType<Player_controller>();
        }

        if (_playerController == null || !IsPlayerInEncounterRange(_playerController.gameObject))
        {
            return;
        }

        TryStartEncounter(_playerController.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryStartEncounter(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartEncounter(other != null ? other.gameObject : null);
    }

    private void TryStartEncounter(GameObject otherObject)
    {
        if (_hasTriggered || otherObject == null)
        {
            return;
        }

        var playerController = otherObject.GetComponent<Player_controller>() ??
                               otherObject.GetComponentInParent<Player_controller>();
        if (playerController == null)
        {
            return;
        }

        GameObject playerObject = playerController.gameObject;
        if (!BattleSessionState.BeginEncounter(playerObject, gameObject))
        {
            return;
        }

        _hasTriggered = true;
        SceneManager.LoadScene(BattleSessionState.BattleSceneName);
    }

    private bool IsPlayerInEncounterRange(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return false;
        }

        float triggerDistance = GetTriggerDistance();
        float triggerDistanceSqr = triggerDistance * triggerDistance;
        Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
        Vector2 enemyPoint = _enemyCollider != null
            ? _enemyCollider.ClosestPoint(playerObject.transform.position)
            : (Vector2)transform.position;
        Vector2 playerPoint = playerCollider != null
            ? playerCollider.ClosestPoint(enemyPoint)
            : (Vector2)playerObject.transform.position;
        if (_enemyCollider != null)
        {
            enemyPoint = _enemyCollider.ClosestPoint(playerPoint);
        }

        return (enemyPoint - playerPoint).sqrMagnitude <= triggerDistanceSqr;
    }

    private float GetTriggerDistance()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return minimumTriggerDistance;
        }

        if (mainCamera.orthographic)
        {
            float worldHeight = mainCamera.orthographicSize * 2f;
            float worldWidth = worldHeight * mainCamera.aspect;
            float screenDistance = Mathf.Min(worldWidth, worldHeight) * screenDistanceFraction;
            return Mathf.Max(minimumTriggerDistance, screenDistance);
        }

        float fallbackDistance = Vector3.Distance(
            mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)),
            mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, 10f))
        ) * screenDistanceFraction * 2f;
        return Mathf.Max(minimumTriggerDistance, fallbackDistance);
    }
}
