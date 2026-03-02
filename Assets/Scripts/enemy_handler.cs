using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Metadata;
using static UnityEngine.GraphicsBuffer;

public class enemy_handler : MonoBehaviour
{
    private Transform tf;

    public GameObject battler;

    private List<int> alive_children = new List<int>();

    private int child_count;

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
    }

    public void your_turn()
    {
        GetChildren();
        StartCoroutine(Get_alive());
        if (alive_children.Count > 0)
        {
            StartCoroutine(Fightchildren());
            battler.GetComponent<battle_handler>().show_options();
        }
        else if (alive_children.Count == 0)
        {
            Debug.Log(alive_children.Count);
            battler.GetComponent<battle_handler>().win();
        }
        
    }


    public void GetChildren()
    {
        child_count = tf.childCount;
        Debug.Log($"there are {child_count} enemies");
    }

    private IEnumerator Fightchildren()
    {
        var child = tf.GetChild(0);
        for (int i = 0; i < child_count; i++)
        {
            child = tf.GetChild(i);
            //child.GetComponent<enemy>().attack_as(i);
        }
        yield return null;
    }

    public IEnumerator Get_alive()
    {
        alive_children.Clear();
        var child = tf.GetChild(0);
        for (int i = 0; i < child_count; i++)
        {
            child = tf.GetChild(i);
            float hp = child.GetComponent<attacker>().report_health();
            if (hp > 0)
            { 
                alive_children.Add(i);
            }
        }
        yield return null;
    }
}

