using System.Collections.Generic;
using UnityEngine;

public class attacker : MonoBehaviour
{
    

    private Transform tf; 

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
        
    }

    private void OnEnable()
    {
        tf = GetComponent<Transform>();
    }
}
