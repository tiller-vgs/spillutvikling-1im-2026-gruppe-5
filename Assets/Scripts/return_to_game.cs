using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class return_to_game : MonoBehaviour
{
    public Animator Trans; //lol

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void load_level(string level)
    {
        StartCoroutine(loading_level(level));
    }

    private IEnumerator loading_level(string level)
    {
        Trans.SetTrigger("Start");
        Debug.Log("Loading back to the overworld");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(level); //change to the new name
        yield return null;
    }


}
