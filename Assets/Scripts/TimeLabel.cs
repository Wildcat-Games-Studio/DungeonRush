using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimeLabel : MonoBehaviour
{
    TextMeshProUGUI label;

    private void Start()
    {
        label = GetComponent<TextMeshProUGUI>();
        GameState.Instance.timerUpdateFunc = TimerUpdateFunc;

    }

    bool doOnce = true;

    private void Update()
    {
        if (doOnce)
        {
            GameState.Instance.StartTimer();
            doOnce = false;
        }
    }

    void TimerUpdateFunc(float time)
    {
        int seconds = (int)time % 60;
        int minutes = (int)time / 60 % 60;
        int hours = (int)time / 3600;
        label.text = string.Format("Time: {0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}
