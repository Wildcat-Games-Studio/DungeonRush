using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityStats
{
    public int maxHealth;
    public int currentHealth;

    public void FullHeal() => currentHealth = maxHealth;
    public void Heal(int amount) => currentHealth = Math.Min(currentHealth + amount, maxHealth);
    public void Damage(int amount) => currentHealth = Math.Min(currentHealth - amount, 0);
}
