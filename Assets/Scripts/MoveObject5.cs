using UnityEngine;

public class MoveObject5 : MonoBehaviour
{
    public Transform pointI;
    public Transform pointH;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointI.position,
                pointH.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
