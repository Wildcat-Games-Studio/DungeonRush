using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    // Important stuff
    public PlayerStats playerStats;
    public SpellStats spellStats;

    public int numBossesDefeated;

    # region SingletonStuff

    // Singleton stuff
    public static GameState Instance { get { return m_instance; } }
    private static GameState m_instance;

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            m_instance = this;
        }
    }

    #endregion 
}
