using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    public float speed;

    void Update()
    {
        if (target == null) return;
        transform.position = Vector2.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }
}
