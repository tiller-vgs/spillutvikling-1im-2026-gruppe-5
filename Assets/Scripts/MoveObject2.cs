using UnityEngine;

public class MoveObject2 : MonoBehaviour
{
    public Transform pointA;
    public Transform pointC;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointA.position,
                pointC.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
