using UnityEngine;
using UnityEngine.SceneManagement;

public class return_to_game : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void go_back_to_game()
    {
        Debug.Log("Loading back to gameplay");
        //SceneManager.LoadScene("test"); //change to the new name
    }

}
