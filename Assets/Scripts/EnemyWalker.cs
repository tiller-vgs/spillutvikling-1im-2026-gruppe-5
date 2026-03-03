using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyWalker : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D _body;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _body.gravityScale = 0f;
        _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        ApplyVelocity();
    }

    private void FixedUpdate()
    {
        ApplyVelocity();
    }

    public void SetMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = newMoveSpeed;
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (_body == null)
        {
            return;
        }

        _body.linearVelocity = Vector2.left * moveSpeed;
    }
}
