using System.Collections;
using Unity.Collections;
using UnityEngine;

public class battle_handler : MonoBehaviour
{
    public GameObject player;

    public GameObject enemies; 

    private GameObject enemy;

    private Transform tf;

    private Transform options;

    private int target;

    private int actions = 1;

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
            options.position = new Vector2(options.position.x, options.position.y - 160);
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
        else if (player_turn == false && options.position.y > 40)
        {
            options.position = new Vector2(options.position.x, options.position.y - 20);
        }
    }

    public void show_options()
    {
        player_turn = true;
        actions = 1;
    }

    private void hide_options()
    {
        player_turn = false;
    }

    public void GetSelectedEnemy(int index)
    {
        target = index;
    }

    public void attack(int damage = 1)
    {
        StartCoroutine(Attack(damage));
    }

    public IEnumerator Attack(int damage = 1)//the damage int is for debuging
    {
        if (actions > 0)
        {
            actions -= 1;
            Invoke("hide_options", 0.3f);
            yield return new WaitForSeconds(1);
            enemy = enemies.GetComponent<Transform>().Find($"enemy_{target}").gameObject;
            enemy.GetComponent<Health_handler>().take_damage(damage); //this should take in and work with the wepon system, but it is not made yet, if ever
            Invoke("enemy_turn", 2f);
        }
    }
    
    private void enemy_turn()
    {
        enemies.GetComponent<enemy_handler>().your_turn();
    }

    public void get_attacked(int damage = 1)
    {
        player.GetComponent<Health_handler>().take_damage(damage);
    }
}
