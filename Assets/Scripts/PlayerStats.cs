using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Heath")]
    public int maxHealth;
    public int currentHealth;

    [Header("Speeds")]
    public float maxSpeed;
    public float accelRate;
    public float friction;

    [Header("Rolling")]
    public float rollCooldown;
}
