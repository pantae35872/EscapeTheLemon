using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : GameplayMonoBehaviour
{
    private float slowdownLength = 5f;

    public static TimeManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    protected override void PauseableUpdate()
    {
        Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
    }

    public void DoSlowmotion(float slowdownLength, float slowdownFactor)
    {
        this.slowdownLength = slowdownLength;
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale;
    }
}
