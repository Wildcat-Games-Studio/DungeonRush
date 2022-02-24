using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/SpellObject")]
public class SpellStats : ScriptableObject
{
    public float damge;
    public float cooldown;
}
