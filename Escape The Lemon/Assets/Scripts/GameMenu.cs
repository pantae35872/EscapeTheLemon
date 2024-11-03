using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public static GameMenu Instance { get; private set; }
    public GameObject pauseMenu;
    public Image health;
    public Image cooldown;
    public TextMeshProUGUI ammoAmount;
    public TMP_InputField AnswerInput;
    public GameObject questionMenu;
    public Slider bossbar;
    public GameObject gameOverMenu;
    public GameObject gameWinMenu;
    public TextMeshProUGUI gameOverReward;
    public TextMeshProUGUI gameWinReward;
    public Image questionTimer;
    public GameObject IOSAndroidControl;
    public GameObject InputSelector;
    public GameObject moveAbleStatus;
    public GameObject sprintLockStatus;
    public GameObject sprintStatus;
    public GameObject specificStatus;
    public GameObject StatusMenu;
    public GameObject ControlsMenu;
    public GameObject DifficultySelectorMenu;
    public MultipleSelectors SelectDifficultySelector;

    [Header("Question")]
    public GameObject questionObject;
    public GameObject questionObject2;
    public GameObject questionAnswer;
    public GameObject questionText;
    public GameObject questionTitle;
    public GameObject questionAnswerOutputResult;
    public GameObject questionAnswerOutputInformation;
    public GameObject questionAnswerOutputInformationNumber;
    public GameObject questionAnswerOutputContinuneButton;
    public GameObject questionRewardText;
    public TextMeshProUGUI nextQuestionTimerText;
    public Slider nextQuestionTimerDisplay;
    public TextMeshProUGUI rewardAmmoText;
    public TextMeshProUGUI rewardHealthText;
    public GameObject selectReward;
    public TextMeshProUGUI globalAmmoText;

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

    public void Quit()
    {
        #if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    [Header("Do not modify")]
    public int result;
    public bool oNegative;

    private void UpdateNumpad()
    {
        Questions.Instance.UpdateNumpad(result);
    }

    public void OnAnswerInputChanged()
    {
        if (AnswerInput.text.Length != 0 && AnswerInput.text != "-")
        {
            result = Int32.Parse(AnswerInput.text.Trim(), new CultureInfo("en-US"));
            UpdateNumpad();
        }
    }

    public void Resume()
    {
        GameManager.Instance.currentMode = GameManager.Instance.lastCurrentModeBeforePause;
    }

    public void Summit()
    {
        Questions.Instance.Summit();
    }

    public void Delete()
    {
        result = 0;
        UpdateNumpad();
    }

    public void Zero()
    {
        AppendDigit(0);
        UpdateNumpad();
    }

    public void One()
    {
        AppendDigit(1);
        UpdateNumpad();
    }

    public void Two()
    {
        AppendDigit(2);
        UpdateNumpad();
    }

    public void Three()
    {
        AppendDigit(3);
        UpdateNumpad();
    }

    public void Four()
    {
        AppendDigit(4);
        UpdateNumpad();
    }

    public void Five()
    {
        AppendDigit(5);
        UpdateNumpad();
    }

    public void Six()
    {
        AppendDigit(6);
        UpdateNumpad();
    }

    public void Seven()
    {
        AppendDigit(7);
        UpdateNumpad();
    }

    public void Eight()
    {
        AppendDigit(8);
        UpdateNumpad();
    }

    public void Nine()
    {
        AppendDigit(9);
        UpdateNumpad();
    }

    public void Negative()
    {
        if (result == 0 && AnswerInput.text.Length == 0 && !oNegative)
        {
            AnswerInput.text = "-";
            oNegative = true;
        }
    }

    private void AppendDigit(int digit)
    {
        string result = this.result.ToString() + digit.ToString();
        if (result.Length <= 4)
        {
            this.result = oNegative ? -Int32.Parse(result) : Int32.Parse(result);
            oNegative = false;
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
