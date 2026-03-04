using UnityEngine;

public class MoveObject8 : MonoBehaviour
{
    public Transform pointZ;
    public Transform pointY;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointZ.position,
                pointY.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
