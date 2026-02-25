using TMPro;
using UnityEngine;

public class Health_handler : MonoBehaviour
{
    public GameObject health_counter; //make dynamic

    private Transform tf; //lol

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

    private void OnEnable()
    {
        tf = GetComponent<Transform>();
    }

    public void take_damage(int damage)
    {
        health -= damage;
        health_counter.GetComponent<set_health>().setHealth(health, max_health);
    }
}
