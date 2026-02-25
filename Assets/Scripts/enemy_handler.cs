using System.Collections;
using UnityEngine;
using static Unity.VisualScripting.Metadata;
using static UnityEngine.GraphicsBuffer;

public class enemy_handler : MonoBehaviour
{
    private Transform tf;

    public GameObject battler;

    private object child_0;
    private object child_1;
    private object child_2;

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
        StartCoroutine(Children());
        battler.GetComponent<battle_handler>().show_options();
    }


    public IEnumerator Children()
    {
        var child_count = tf.childCount;
        var child = tf.GetChild(0);
        Debug.Log(child.name);
        Debug.Log(child_count);
        for (int i = 0; i < child_count; i++)
        {
            child = tf.GetChild(i);
            //child.GetComponent<enemy>().attack_as(i);
        }
        yield return null;
    }
}

