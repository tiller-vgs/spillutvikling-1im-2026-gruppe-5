using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player_controller : MonoBehaviour
{
    public float movespeed = 6.7f;

    private InputAction move_act;

    private Vector2 move_dir;

    private Rigidbody2D rb;

    public InputActionAsset inputsystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Run script working as intended");
    }

    private void Awake()
    {
        var playermMap = inputsystem.FindActionMap("player");

        move_act = playermMap.FindAction("Move");

        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        move_dir = move_act.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        run();
    }

    private void run()
    {
        rb.linearVelocity = new Vector2(movespeed * move_dir.x, movespeed * move_dir.y);
    }
}
