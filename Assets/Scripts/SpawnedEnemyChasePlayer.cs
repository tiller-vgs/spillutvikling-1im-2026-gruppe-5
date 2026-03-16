using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class SpawnedEnemyChasePlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.2f;

    private Player_controller _player;
    private Rigidbody2D _body;
    private bool _isChasing;
    private bool _dragonBonesFacingConfigured;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.gravityScale = 0f;
        _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (!_isChasing)
        {
            _body.linearVelocity = Vector2.zero;
            return;
        }

        if (_player == null)
        {
            _player = FindFirstObjectByType<Player_controller>();
        }

        if (_player == null)
        {
            _body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentPosition = transform.position;
        float deltaX = _player.transform.position.x - currentPosition.x;

        if (Mathf.Abs(deltaX) <= 0.0001f)
        {
            _body.linearVelocity = Vector2.zero;
            return;
        }

        _body.linearVelocity = new Vector2(Mathf.Sign(deltaX) * moveSpeed, 0f);
    }

    public void StartChasing()
    {
        _isChasing = true;
    }

    public void FacePlayer()
    {
        if (_player == null)
        {
            _player = FindFirstObjectByType<Player_controller>();
        }

        if (_player == null)
        {
            return;
        }

        FaceDirection(_player.transform.position.x - transform.position.x);
    }

    private void FaceDirection(float horizontalDirection)
    {
        if (Mathf.Abs(horizontalDirection) <= 0.0001f)
        {
            return;
        }

        DirectionalDragonBonesView view = GetComponent<DirectionalDragonBonesView>();
        if (view != null)
        {
            if (!_dragonBonesFacingConfigured)
            {
                view.sideVisualScale = new Vector3(
                    -Mathf.Abs(view.sideVisualScale.x),
                    view.sideVisualScale.y,
                    view.sideVisualScale.z
                );
                view.RefreshView();
                _dragonBonesFacingConfigured = true;
            }

            view.FaceSide(horizontalDirection);
            return;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            bool shouldFlipX = horizontalDirection < 0f;
            if (spriteRenderer.flipX == shouldFlipX)
            {
                return;
            }

            spriteRenderer.flipX = shouldFlipX;
        }
    }

    private void OnDisable()
    {
        if (_body != null)
        {
            _body.linearVelocity = Vector2.zero;
        }
    }
}
