using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEngine;

public class attacker : MonoBehaviour
{
    public int damage = 1;

    public int MaxHeal = 4;
    
    public int DoAttack = 6;

    public int DoNothing = 1;

    private Animator anim;

    private Transform tf;

    private GameObject RingLeader;

    private float health = 10;

    private float max_health = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float report_health()
    {
        health = tf.GetComponent<Health_handler>().health;
        max_health = tf.GetComponent<Health_handler>().max_health;
        return health;
    }

    public void attack_as(int index)
    {
        //index currently does nothing, but eventualy if we get/got time it could come in handy. I hope 
        int would_heal = (int)(max_health / health)-1;
        if (would_heal > MaxHeal) 
        {
            would_heal = MaxHeal;
        }
        int BoringMathStuff = (would_heal + DoAttack + DoNothing); 
        int choice = Random.Range(1, BoringMathStuff);
        //Debug.Log($"Total chance is {BoringMathStuff}");
        

        if (choice >= 0 && choice <= 5) 
        {
            Debug.Log($"Enemy {index} has chosen to attack, with a {choice}"); //said it would come in handy //(Did it realy though?)
            StartCoroutine(attack());
            
            
        }
        else if (choice > (5) && choice <= (5+would_heal))
        {
            Debug.Log($"Enemy {index} has chosen to heal, with a {choice}");
            int heal = Random.Range(0, 2);
            RingLeader.GetComponent<battle_handler>().heal_enemy(index, heal);
        }
        else
        {
            Debug.Log($"Enemy {index} has chosen to... do nothing, with a {choice}");
        }
    }

    private IEnumerator attack()
    {
        RingLeader.GetComponent<battle_handler>().attack_player(damage);
        var dragonBonesView = GetComponent<DirectionalDragonBonesView>();
        if (dragonBonesView != null)
        {
            dragonBonesView.PlayTestShootingAnimation();
        }
        yield return new WaitForSeconds(0.3f);
        anim.SetTrigger("Attack");
        yield return null;
    }

    public void get_hit()
    {
        anim.SetTrigger("Damage");
    }

    private void OnEnable()
    {
        tf = GetComponent<Transform>();
        anim = GetComponent<Animator>();
        RingLeader = GameObject.Find("Battle_handler");
    }
}
