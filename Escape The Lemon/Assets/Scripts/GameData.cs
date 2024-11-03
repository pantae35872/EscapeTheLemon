using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float sensitivity;
    public float questionTime;
    public int healthAmount;
    public InputMode inputMode;
    public float nextQuestionTime;
    public bool sprintLock;
    public int gunDamage;
    public int coin;

    public GameData() {
        sensitivity = 15;
        healthAmount = 1000;
        nextQuestionTime = 30f;
        gunDamage = 10;
    }
}
