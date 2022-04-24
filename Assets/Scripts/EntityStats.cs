using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityStats : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public float invTime;

    public delegate void OnDeath();
    public OnDeath onDeath;

    private float _nextDamageTime = 0.0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void FullHeal() => currentHealth = maxHealth;
    public void Heal(int amount) => currentHealth = Math.Min(currentHealth + amount, maxHealth);
    public void Damage(int amount)
    {
        if (Time.time > _nextDamageTime)
        {
            _nextDamageTime = Time.time + invTime;

            currentHealth -= amount;
            if (currentHealth < 0)
            {
                print("Damaged: " + name + " " + amount);
                onDeath?.Invoke();
            }
        }
    }
}
