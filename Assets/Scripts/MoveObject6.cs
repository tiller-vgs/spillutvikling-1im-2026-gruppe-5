using UnityEngine;

public class MoveObject6 : MonoBehaviour
{
    public Transform pointJ;
    public Transform pointK;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointJ.position,
                pointK.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
