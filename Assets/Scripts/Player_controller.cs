using UnityEngine;
using UnityEngine.InputSystem;

public class Player_controller : MonoBehaviour
{
    private const float ShootCooldownSeconds = 0.6f;
    private const float GunPickupRadius = 1.5f;
    private const float GunDropFootOffset = 0.12f;
    private const string GunObjectName = "Gun";

    public float movespeed = 6.7f;
    public InputActionAsset inputsystem;

    private InputAction _moveAction;
    private InputAction _attackAction;
    private InputAction _interactAction;
    private Vector2 _moveDirection;
    private Rigidbody2D _rb;
    private Collider2D _pickupCollider;
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
        _interactAction = inputsystem != null
            ? inputsystem.FindAction("Player/Interact", false) ??
              inputsystem.FindAction("player/Interact", false) ??
              inputsystem.FindAction("Interact", false) ??
              inputsystem.FindAction("interact", false)
            : null;
        _rb = GetComponent<Rigidbody2D>();
        _pickupCollider = GetComponent<Collider2D>();
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

        if (_interactAction != null)
        {
            _interactAction.performed += OnInteractPerformed;
            _interactAction.Enable();
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

        if (_interactAction != null)
        {
            _interactAction.performed -= OnInteractPerformed;
            _interactAction.Disable();
        }

        if (_moveAction != null)
        {
            _moveAction.Disable();
        }
    }

    private void Update()
    {
        _moveDirection = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            TryDropGun();
        }
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
            return_to_game.PlayGunshotSfx();
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        TryPickUpGun();
    }

    private void run()
    {
        if (_rb == null)
        {
            return;
        }

        _rb.linearVelocity = new Vector2(movespeed * _moveDirection.x, movespeed * _moveDirection.y);
    }

    private void TryPickUpGun()
    {
        if (_dragonBonesView == null || _dragonBonesView.HasGun)
        {
            return;
        }

        Transform gun = FindNearestGun();
        if (gun == null)
        {
            return;
        }

        _dragonBonesView.EquipGun();
        Destroy(gun.gameObject);
    }

    private void TryDropGun()
    {
        if (_dragonBonesView == null || !_dragonBonesView.HasGun)
        {
            return;
        }

        var droppedGun = new GameObject(GunObjectName);
        droppedGun.transform.position = GetGunDropPosition();
        GunVisualAutoReplacer.ConfigureRuntimeGun(droppedGun);
        _dragonBonesView.UnequipGun();
    }

    private Vector3 GetGunDropPosition()
    {
        Vector3 dropPosition = transform.position;
        if (_pickupCollider == null)
        {
            dropPosition.y -= 0.35f;
            return dropPosition;
        }

        Bounds bounds = _pickupCollider.bounds;
        dropPosition.x = bounds.center.x;
        dropPosition.y = bounds.min.y + GunDropFootOffset;
        return dropPosition;
    }

    private Transform FindNearestGun()
    {
        float maxSqrDistance = GunPickupRadius * GunPickupRadius;
        float nearestSqrDistance = maxSqrDistance;
        Transform nearestGun = null;
        Transform[] sceneTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (Transform candidate in sceneTransforms)
        {
            if (candidate == null ||
                !candidate.gameObject.scene.IsValid() ||
                !candidate.gameObject.activeInHierarchy ||
                !IsGunObject(candidate))
            {
                continue;
            }

            float sqrDistance = GetGunPickupSqrDistance(candidate);
            if (sqrDistance > nearestSqrDistance)
            {
                continue;
            }

            nearestSqrDistance = sqrDistance;
            nearestGun = candidate;
        }

        return nearestGun;
    }

    private float GetGunPickupSqrDistance(Transform gunTransform)
    {
        if (gunTransform == null)
        {
            return float.PositiveInfinity;
        }

        Vector2 playerPoint = _pickupCollider != null
            ? _pickupCollider.ClosestPoint(gunTransform.position)
            : (Vector2)transform.position;

        Collider2D gunCollider = gunTransform.GetComponent<Collider2D>();
        if (gunCollider != null)
        {
            Vector2 gunPoint = gunCollider.ClosestPoint(playerPoint);
            if (_pickupCollider != null)
            {
                playerPoint = _pickupCollider.ClosestPoint(gunPoint);
            }

            return (gunPoint - playerPoint).sqrMagnitude;
        }

        Renderer gunRenderer = gunTransform.GetComponent<Renderer>();
        if (gunRenderer == null)
        {
            gunRenderer = gunTransform.GetComponentInChildren<Renderer>();
        }

        if (gunRenderer != null)
        {
            Vector3 gunPoint = gunRenderer.bounds.ClosestPoint(playerPoint);
            if (_pickupCollider != null)
            {
                playerPoint = _pickupCollider.ClosestPoint(gunPoint);
            }

            return ((Vector2)gunPoint - playerPoint).sqrMagnitude;
        }

        return ((Vector2)gunTransform.position - playerPoint).sqrMagnitude;
    }

    private static bool IsGunObject(Transform candidate)
    {
        string objectName = candidate.name;
        return objectName == GunObjectName || objectName.StartsWith("Gun(");
    }
}
