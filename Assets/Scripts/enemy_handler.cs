using System.Collections;
using UnityEngine;
using static Unity.VisualScripting.Metadata;
using static UnityEngine.GraphicsBuffer;

public class enemy_handler : MonoBehaviour
{
    private Transform tf;

    public GameObject battler;

    private int child_count;

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
        GetChildren();
        StartCoroutine(Fightchildren());
        battler.GetComponent<battle_handler>().show_options();
    }


    public void GetChildren()
    {
        int child_count = tf.childCount;
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

    public IEnumerator Get_health(int child_count)
    {
        yield return true;
    }
}

