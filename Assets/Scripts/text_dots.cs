using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Loading;
using UnityEngine;

public class text_dots : MonoBehaviour
{
    private TextMeshProUGUI text;

    private bool loading = false;

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
        text = GetComponent<TextMeshProUGUI>();
        loading = true;
        StartCoroutine(animate_the_dots());
    }

    private IEnumerator animate_the_dots()
    {
        while (loading)
        {
            text.text = "Loading.";
            yield return new WaitForSeconds(0.5f);
            text.text = "Loading..";
            yield return new WaitForSeconds(0.5f);
            text.text = "Loading...";
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }
}
