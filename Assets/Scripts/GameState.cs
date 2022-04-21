using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    // Important stuff
    public bilesPlayerController player = null;
    public int numBossesDefeated;

    [SerializeField]
    private int m_roomWidth = 0;
    [SerializeField]
    private int m_roomHeight = 0;

    public LayerMask playerLayerMask;

    #region Timer

    public float CurrentTime { get; private set; }
    private float lastTime;
    private bool doTimerTicks;

    public void PauseTimer() => doTimerTicks = false;
    public void StartTimer()
    {
        doTimerTicks = true;
    }
    public void ResetTimer()
    {
        CurrentTime = 0.0f;
        lastTime = 0.0f;
        timerUpdateFunc?.Invoke(CurrentTime);
    }
    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    public delegate void TimerUpdateFunc(float time);
    public TimerUpdateFunc timerUpdateFunc;

    #endregion
    private void Awake()
    {
        SetupSingleton();
        player = null;
        numBossesDefeated = 0;
    }

    private void Start()
    {
        ResetTimer();
        PauseTimer();
    }

    private void Update()
    {
        if (doTimerTicks)
        {
            lastTime = CurrentTime;
            CurrentTime += Time.deltaTime;

            // truncate and check if a second (1.0f) has passed
            if((int)CurrentTime > (int)lastTime)
            {
                timerUpdateFunc?.Invoke(CurrentTime);
            }
        }
    }

    public int GetRoomWidth() => m_roomWidth;
    public int GetRoomHeight() => m_roomHeight;

    # region SingletonStuff

    // Singleton stuff
    public static GameState Instance { get { return m_instance; } }
    private static GameState m_instance;

    private void SetupSingleton()
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
