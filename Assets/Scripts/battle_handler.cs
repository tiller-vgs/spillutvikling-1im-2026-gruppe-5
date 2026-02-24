using UnityEngine;

public class battle_handler : MonoBehaviour
{
    public Transform options;

    public GameObject player;

    public GameObject enemy; //Make this system dymaic in the furure

    public bool player_turn = true;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        if (player_turn == false)
        {
            hide_options();  //FIX
        }
    }

    private void FixedUpdate()
    {
        if (player_turn == true && options.position.y < 250) 
        {
            //Debug.Log("movin' up");
            options.position = new Vector2(options.position.x, options.position.y + 20);
        }
    }

    public void show_options()
    {
        player_turn = true;
    }

    private void hide_options()
    {
        player_turn = false;
        options.position = new Vector2(options.position.x, options.position.y - 160);
    }

    public void attack()
    {
        Debug.Log($"Attacking");
        enemy.GetComponent<Health_handler>().take_damage(1);
    }
}
