using TMPro;
using UnityEngine;

public class set_health : MonoBehaviour
{
    public TextMeshProUGUI txt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setHealth(float health, float max_health)
    {
        txt.text = $"{health}/{max_health}";
    }
}
