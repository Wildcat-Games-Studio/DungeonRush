using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    public float speed;
    public float max;
    public float min;

    void LateUpdate()
    {
        Vector2 newPos = transform.localPosition;

        newPos.y = Mathf.Cos(Time.time * speed) * 0.5f + 0.5f; // sin between 0.0 and 1.0
        newPos.y = newPos.y * (min - max) + max; // put between min and max, math done this way so at x = 0, y = min

        transform.localPosition = newPos;
    }
}
