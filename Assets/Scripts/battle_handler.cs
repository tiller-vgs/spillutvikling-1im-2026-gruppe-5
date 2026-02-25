using UnityEngine;

public class battle_handler : MonoBehaviour
{
    public GameObject player;

    public GameObject enemies; 

    private GameObject enemy;

    private Transform tf;

    private Transform options;

    private int target;

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
            hide_options();  
        }

        tf = GetComponent<Transform>();
        options = tf.Find("Canvas/Player_buttons").transform;
    }

    private void FixedUpdate()
    {
        if (player_turn == true && options.position.y < 200) 
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

    public void GetSelectedEnemy(int index)
    {
        target = index;
    }

    public void attack(int damage = 1)//the damage int is for debuging
    {
        enemy = enemies.GetComponent<Transform>().Find($"enemy_{target}").gameObject;
        enemy.GetComponent<Health_handler>().take_damage(damage); //this should take in and work with the wepon system, but it is not made yet
    }

    public void get_attacked(int damage = 1)
    {
        player.GetComponent<Health_handler>().take_damage(damage);
    }
}
