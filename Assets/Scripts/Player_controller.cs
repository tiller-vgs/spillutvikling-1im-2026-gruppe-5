using UnityEngine;
using UnityEngine.InputSystem;

public class Player_controller : MonoBehaviour
{
    private const float ShootCooldownSeconds = 0.5f;

    public float movespeed = 6.7f;
    public InputActionAsset inputsystem;

    private InputAction _moveAction;
    private InputAction _attackAction;
    private Vector2 _moveDirection;
    private Rigidbody2D _rb;
    private DirectionalDragonBonesView _dragonBonesView;
    private float _nextShootTime;

    private void Awake()
    {
        _moveAction = inputsystem != null
            ? inputsystem.FindAction("Player/Move", false) ??
              inputsystem.FindAction("player/Move", false) ??
              inputsystem.FindAction("Move", false) ??
              inputsystem.FindAction("move", false)
            : null;
        _attackAction = inputsystem != null
            ? inputsystem.FindAction("Player/Attack", false) ??
              inputsystem.FindAction("player/Attack", false) ??
              inputsystem.FindAction("Attack", false) ??
              inputsystem.FindAction("attack", false)
            : null;
        _rb = GetComponent<Rigidbody2D>();
        _dragonBonesView = GetComponent<DirectionalDragonBonesView>();

        if (_rb != null)
        {
            _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnEnable()
    {
        if (_attackAction != null)
        {
            _attackAction.performed += OnAttackPerformed;
            _attackAction.Enable();
        }

        if (_moveAction != null)
        {
            _moveAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (_attackAction != null)
        {
            _attackAction.performed -= OnAttackPerformed;
            _attackAction.Disable();
        }

        if (_moveAction != null)
        {
            _moveAction.Disable();
        }
    }

    private void Update()
    {
        _moveDirection = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
    }

    private void FixedUpdate()
    {
        run();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (context.control == null ||
            context.control.device is not Mouse ||
            context.control.name != "leftButton" ||
            Time.time < _nextShootTime)
        {
            return;
        }

        if (_dragonBonesView != null && _dragonBonesView.PlayShootingAnimation())
        {
            _nextShootTime = Time.time + ShootCooldownSeconds;
        }
    }

    private void run()
    {
        if (_rb == null)
        {
            return;
        }

        _rb.linearVelocity = new Vector2(movespeed * _moveDirection.x, movespeed * _moveDirection.y);
    }
}
