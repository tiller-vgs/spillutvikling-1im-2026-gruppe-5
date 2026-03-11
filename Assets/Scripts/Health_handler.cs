using System.Collections;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class Health_handler : MonoBehaviour
{
    public GameObject health_counter; //make dynamic

    private Transform tf; //lol

    private ParticleSystem par;

    public AudioSource hit_1;

    public AudioSource hit_2;

    public AudioSource hit_3;

    public AudioSource death;

    private Animator anim;

    private int chosen_hit_sfx;

    public float health = 10;

    public float max_health = 10;
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
        tf = GetComponent<Transform>();
        par = GetComponent<ParticleSystem>();
        chosen_hit_sfx = Random.Range(1, 3);
        anim = GetComponent<Animator>();
        //chosen_hit_sfx = 1; //debug
    }

    public void take_damage(int damage)
    {
        health = health - damage;
        if (gameObject.name != "Player")
        {
            if (health > 0)
            {
                StartCoroutine(play_hit());
            }
            else
            {
                StartCoroutine(play_death());
            }
        }
        else if(gameObject.name == "Player")
        {
            StartCoroutine(Player_damage());
        }

    }

    private IEnumerator Player_damage()
    {
        anim.SetTrigger("Damage");
        yield return new WaitForSeconds(0.4f);
        health_counter.GetComponent<set_health>().setHealth(health, max_health);
        par.Play();
        yield return null;
    }

    private IEnumerator play_hit()
    {
        float pitch = Random.Range(90, 120);
        pitch = pitch / 100;
        if (chosen_hit_sfx == 1)
        {
            hit_1.pitch = pitch;
            hit_1.Play();
        }
        else if (chosen_hit_sfx == 2)
        {
            hit_2.pitch = pitch;
            hit_2.Play();
        }
        else
        {
            hit_3.pitch = pitch;
            hit_3.Play();
        }
        gameObject.GetComponent<attacker>().get_hit();
        yield return new WaitForSeconds(0.4f);
        health_counter.GetComponent<set_health>().setHealth(health, max_health);
        par.Play();
        yield return null;
    }
    private IEnumerator play_death()
    {
        float pitch = Random.Range(90, 120);
        pitch = pitch / 100;
        yield return null;
        death.Play();
        yield return new WaitForSeconds(0.2f);
        par.Play();
    }
    public void heal(int heal)
    {
        health += heal;
        if (health > max_health)
        {
            health = max_health;
        }
        health_counter.GetComponent<set_health>().setHealth(health, max_health);
    }
}
