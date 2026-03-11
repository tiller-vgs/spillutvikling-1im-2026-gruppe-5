using UnityEngine;

public class MoveObject3 : MonoBehaviour
{
    public Transform pointD;
    public Transform pointE;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointD.position,
                pointE.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
