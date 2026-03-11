using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public Transform pointR;
    public Transform pointB;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointR.position,
                pointB.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
