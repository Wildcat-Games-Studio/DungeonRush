using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityStats : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public int maxHealth;
    public int currentHealth;

    public float invTime;

    public delegate void OnDeath();
    public OnDeath onDeath;

    public delegate void OnDamage(int currentHealth);
    public OnDamage onDamage;

    private float _nextDamageTime = 0.0f;
    private int frame = 0;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (Time.time < _nextDamageTime && frame % 5 == 0)
        {
            spriteRenderer.forceRenderingOff = !spriteRenderer.forceRenderingOff;
        }
        frame++;
    }

    void MakeVisible()
    {
        spriteRenderer.forceRenderingOff = false;
    }

    public void FullHeal() => currentHealth = maxHealth;
    public void Heal(int amount) => currentHealth = Math.Min(currentHealth + amount, maxHealth);
    public void Damage(int amount)
    {
        if (Time.time > _nextDamageTime)
        {
            _nextDamageTime = Time.time + invTime;

            frame = 0;
            Invoke("MakeVisible", invTime);

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                MakeVisible();
                _nextDamageTime = 0;
                onDeath?.Invoke();
            }
            else
            {
                onDamage?.Invoke(currentHealth);
            }
        }
    }
}
