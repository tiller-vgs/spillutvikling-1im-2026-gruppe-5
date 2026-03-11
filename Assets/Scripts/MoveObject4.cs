using UnityEngine;

public class MoveObject4 : MonoBehaviour
{
    public Transform pointF;
    public Transform pointG;
    public float duration = 2f;

    float timeElapsed = 0f;

    void Update()
    {
        if (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(
                pointF.position,
                pointG.position,
                timeElapsed / duration
            );

            timeElapsed += Time.deltaTime;
        }
    }
}
