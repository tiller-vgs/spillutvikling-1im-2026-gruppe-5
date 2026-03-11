using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        GetAliveChildren();
        if (alive_children.Count > 0)
        {
            StartCoroutine(Fightchildren());
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
        for (int i = 0; i < alive_children.Count; i++)
        {
            int childIndex = alive_children[i];
            Transform child = tf.GetChild(childIndex);
            child.GetComponent<attacker>().attack_as(childIndex);
        }
        yield return new WaitForSeconds(1);
        battler.GetComponent<battle_handler>().show_options();
        yield return null;
    }

    private void GetAliveChildren()
    {
        alive_children.Clear();
        for (int i = 0; i < child_count; i++)
        {
            Transform child = tf.GetChild(i);
            float hp = child.GetComponent<attacker>().report_health();
            if (hp > 0)
            {
                alive_children.Add(i);
            }
        }
    }
}

