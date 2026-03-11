using System.Collections;
using Unity.Collections;
using UnityEngine;

public class battle_handler : MonoBehaviour
{
    public GameObject winner;

    public GameObject player;

    public GameObject enemies;

    public Animator death;

    private GameObject enemy;

    private Transform tf;

    private Transform options;

    private int target = 0;

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
        float player_health = player.GetComponent<Health_handler>().health;
        if (player_health >= 1)
        {
            player_turn = true;
            actions = 1;
        }
        else if (player_health <= 0)
        {
            player_death();
        }
        
    }

    private void player_death()
    {
        Debug.Log("death triggerd");
        death.SetTrigger("Death");
    }

    private void hide_options() //might come in handy
    {
        player_turn = false;
    }

    public void GetSelectedEnemy(int index)//to be used later in case we get more enemies
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
            player.GetComponent<Animator>().SetTrigger("Attack");
            yield return new WaitForSeconds(0.3f);
            enemy.GetComponent<Health_handler>().take_damage(damage); //this should take in and work with the wepon system, but it is not made yet, if ever gets made
            Invoke("enemy_turn", 2f);
        }
    }

    public void heal_enemy(int target, int heal)
    {
        StartCoroutine(healing_enemy(target, heal));
    }
    
    private IEnumerator healing_enemy(int target, int heal)
    {
        yield return new WaitForSeconds(0.5f);
        enemy = enemies.GetComponent<Transform>().Find($"enemy_{target}").gameObject;
        enemy.GetComponent<Health_handler>().heal(heal);

        yield return null;
    }

    private void enemy_turn()
    {
        enemies.GetComponent<enemy_handler>().your_turn();
    }
    
    public void attack_player(int damage)
    {
        StartCoroutine(get_attacked(damage));
    }

    private IEnumerator get_attacked(int damage = 1)
    {
        yield return new WaitForSeconds(0.3f);
        player.GetComponent<Health_handler>().take_damage(damage);
        yield return null;
    }
    public void win()
    {
        winner.gameObject.SetActive(true);
    }
}
