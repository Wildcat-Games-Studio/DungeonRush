using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBreaker : MonoBehaviour
{
    public ParticleSystem system;
    public GameObject graphics;
    public void Break()
    {
        float totalDuration = system.main.duration + system.main.startLifetime.constantMax;
        Destroy(gameObject, totalDuration);
        graphics.SetActive(false);
        system.Play();
    }
}
