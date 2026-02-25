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

    public void attack_as(int index)
    {
        tf.GetComponent<Health_handler>().health = health;
        tf.GetComponent<Health_handler>().max_health = max_health;

    }

    private void OnEnable()
    {
        tf = GetComponent<Transform>();
    }
}
