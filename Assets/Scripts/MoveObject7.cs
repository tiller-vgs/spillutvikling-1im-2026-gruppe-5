using UnityEngine;

public class MoveObject7 : MonoBehaviour
{
    public Transform pointP;
    public Transform pointQ;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointP.position,
                pointQ.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
