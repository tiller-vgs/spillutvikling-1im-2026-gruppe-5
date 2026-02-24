using UnityEngine;

public class battle_handler : MonoBehaviour
{
    public Transform options;
    
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
        options.position = new Vector2(options.position.x , options.position.y - 153);
        Debug.Log(options.position.y);
    }

    private void FixedUpdate()
    {
        if (player_turn == true && options.position.y < 250) 
        {
            Debug.Log("movin' up");
            options.position = new Vector2(options.position.x, options.position.y + 1);
        }
    }

    public void show_options()
    {
        player_turn = true;
    }
}
