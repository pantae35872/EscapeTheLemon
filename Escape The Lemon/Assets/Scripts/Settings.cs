using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputMode
{
    Joystick,
    Keyboard
}

public class Settings : MonoBehaviour, IDataPersistance
{
    public static Settings Instance { get; private set; }
    public float sensitivity;
    public float questionTime;
    public int healthAmount;
    public InputMode inputMode;
    public float nextQuestionTime;
    public bool sprintLock;
    public int gunDamage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LoadData(GameData data)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        sensitivity = data.sensitivity;
        questionTime = data.questionTime;
        healthAmount = data.healthAmount;   
        inputMode = data.inputMode;
        nextQuestionTime = data.nextQuestionTime;
        sprintLock = data.sprintLock;
        gunDamage = data.gunDamage;
    }

    public void SaveData(GameData data)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        data.sensitivity = sensitivity;
        data.questionTime = questionTime;
        data.healthAmount = healthAmount;
        data.inputMode = inputMode;
        data.nextQuestionTime = nextQuestionTime;
        data.sprintLock = sprintLock;
        data.gunDamage = gunDamage;
    }
}
