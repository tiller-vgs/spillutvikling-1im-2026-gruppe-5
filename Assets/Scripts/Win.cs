using UnityEngine;

public class Win : MonoBehaviour
{
    private RectTransform top_tf;
    private RectTransform bottom_tf;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        var Top_text = GetComponent<Transform>().GetChild(0);
        var bottom_text = GetComponent<Transform>().GetChild(1);
        if (Top_text.name == "Buttom_text")
        {
            bottom_text = Top_text;
        }
        top_tf = Top_text.gameObject.GetComponent<RectTransform>();
        bottom_tf = bottom_text.gameObject.GetComponent<RectTransform>();
    }

// Update is called once per frame
    void Update()
    {
        if (top_tf.position.y > 280) 
        {
            top_tf.position = new Vector2(top_tf.position.x, top_tf.position.y - 10);
        }
        if (bottom_tf.position.y < 100)
        {
            bottom_tf.position = new Vector2(bottom_tf.position.x, bottom_tf.position.y + 10);
        }
    }
}

