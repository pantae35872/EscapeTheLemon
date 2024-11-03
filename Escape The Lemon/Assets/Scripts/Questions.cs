using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Questions : GameplayMonoBehaviour
{
    public static Questions Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            nextQuestionTimer = Settings.Instance.nextQuestionTime;
        }
    }

    private bool isAsking;
    private int inputAnswer;
    private int answer;
    private float timer;
    private float nextQuestionTimer;
    private bool wasInPausedMenu;
    private int rewardAmmoAmount;
    private int rewardHealthAmount;
    private bool correct;
    private Vector2 questionTextBeforeAnimatePos;
    private float questionTextBeforeAnimateFontSize;
    private Vector2 questionTitleBeforeAnimatePos;
    private Vector2 questionAnswerOutputResultBeforeAnimatePos;
    private Vector2 questionAnswerOutputInformationBeforeAnimatePos;
    private Vector2 questionAnswerOutputContinuneButtonBeforeAnimatePos;
    private Color questionRewardTextBeforeAnimateColor;
    private InputAction QuestionSummit;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private MultipleSelectors rewardSelector;

    private void OnEnable()
    {
        QuestionSummit = GameManager.Instance.playerControls.Player.QuestionSummit;
        QuestionSummit.Enable();
        QuestionSummit.performed += contex =>
        {
            Summit();
        };
    }

    private void OnDisable()
    {
        QuestionSummit.Disable();
    }

    protected override void NotifyResume()
    {
        GameMenu.Instance.AnswerInput.text = "";
        GameMenu.Instance.result = 0;
        ResetTimer();
        GenerateQuestion();
    }

    protected override void PauseableUpdate()
    {
        if (nextQuestionTimer > 0)
        {
            nextQuestionTimer -= Time.deltaTime;
            UpdateNextQuestionTimerDisplay(nextQuestionTimer);
        }
        else
        {
            NextQuestionFlash();
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPause)
        {
            PauseableUpdate();
        }

        if (isAsking && GameManager.Instance.currentMode == CurrentMode.QUESTION)
        {
            if (wasInPausedMenu)
            {
                NotifyResume();
                wasInPausedMenu = false;
            }
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                UpdateTimerDisplay(timer);
            }
            else
            {
                Flash();
            }
        } else if (!wasInPausedMenu && GameManager.Instance.currentMode == CurrentMode.PAUSE)
        {
            wasInPausedMenu = true;
        }
    }

    private void UpdateTimerDisplay(float time)
    {
        GameMenu.Instance.questionTimer.fillAmount = (float)Utils.MapValue(time, Settings.Instance.questionTime, 0, 1, 0);
    }

    private void UpdateNextQuestionTimerDisplay(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        GameMenu.Instance.nextQuestionTimerText.text = seconds < 10 ? string.Format("{0}:0{1}", minutes, seconds) : 
            string.Format("{0}:{1}", minutes, seconds);
        GameMenu.Instance.nextQuestionTimerDisplay.value = (float) Utils.MapValue(time, 0, 
            Settings.Instance.nextQuestionTime,
            0, 1);
    }

    private void Flash()
    {
        if (timer != 0)
        {
            timer = 0;
            UpdateTimerDisplay(timer);
        }
        Summit();
    }

    private void NextQuestionFlash()
    {
        timer = 0;
        UpdateNextQuestionTimerDisplay(timer);
        StartCoroutine(AskQuestion());
        ResetNextQuestionTimer();
    }

    private IEnumerator AskQuestion()
    {
        questionTextBeforeAnimatePos = GameMenu.Instance.questionText.transform.localPosition;
        questionTextBeforeAnimateFontSize = GameMenu.Instance.questionText.GetComponent<TextMeshProUGUI>().fontSize;
        questionTitleBeforeAnimatePos = GameMenu.Instance.questionTitle.transform.localPosition;
        GameManager.Instance.currentMode = CurrentMode.QUESTION;
        GameMenu.Instance.AnswerInput.text = "";
        GenerateQuestion();
        GameMenu.Instance.questionObject2.SetActive(true);
        GameMenu.Instance.questionObject.SetActive(false);
        GameMenu.Instance.questionAnswer.SetActive(false);
        GameMenu.Instance.selectReward.SetActive(false);
        GameMenu.Instance.questionMenu.SetActive(true);
        GameMenu.Instance.questionTitle.LeanMoveLocal(new Vector2(0, 1086), 0.8f).setEaseOutQuad();
        yield return new WaitForSeconds(0.5f);
        GameMenu.Instance.questionText.transform.LeanMoveLocal(new Vector2(2.1935e-05f, 700), 1f).setEaseOutQuad();
        yield return new WaitForSeconds(1);
        LeanTween.value(GameMenu.Instance.questionText, 220, 120, 1f).setOnUpdate(
            (float val) =>
            GameMenu.Instance.questionText.GetComponent<TextMeshProUGUI>().fontSize = val);
        GameMenu.Instance.questionText.transform.LeanMoveLocal(new Vector2(2.1935e-05f, 950), 1f).setEaseOutQuad().setOnComplete(
            () => GameMenu.Instance.questionObject.SetActive(true));
        ResetTimer();
        isAsking = true;
    }

    private void ResetNextQuestionTimer()
    {
        nextQuestionTimer = Settings.Instance.nextQuestionTime;
    }

    private void ResetTimer()
    {
        timer = Settings.Instance.questionTime;
    }

    private void GenerateQuestion()
    {
        int number1, number2;
        if (GameManager.Instance.currentDifficulty == Difficulties.EASY)
        {
            number1 = Random.Range(2, 12);
            number2 = Random.Range(2, 12);
        } else if (GameManager.Instance.currentDifficulty == Difficulties.NORMAL)
        {
            number1 = Random.Range(20, 60);
            number2 = Random.Range(20, 60);
        }
        else
        {
            number1 = Random.Range(50, 150);
            number2 = Random.Range(50, 150);
        }

        string exp = "";

        switch (Random.Range(0, GameManager.Instance.currentDifficulty == Difficulties.HARD ? 4 :
            GameManager.Instance.currentDifficulty == Difficulties.NORMAL ? 3 : 2)) {
            case 0:
                answer = number1 + number2;
                exp = "+";
                break;
            case 1:
                number1 += number2;
                answer = number1 - number2;
                exp = "-";
                break;
            case 2:
                if (GameManager.Instance.currentDifficulty == Difficulties.NORMAL)
                {
                    number1 = Random.Range(4, 12);
                    number2 = Random.Range(2, 5);
                } else
                {
                    number1 = Random.Range(4, 25);
                    number2 = Random.Range(2, 6);
                }
                answer = number1 * number2;
                exp = "x";
                break;
            case 3:
                number1 = Random.Range(4, 24);
                number2 = Random.Range(3, 7);
                answer = number1 * number2;
                int banser = answer;
                answer /= number2;
                number1 = banser;
                exp = "÷";
                break;
        }
        GameMenu.Instance.questionText.GetComponent<TextMeshProUGUI>().text = string.Format("{0} {1} {2} =", number1, exp, number2);
    }

    public void UpdateNumpad(int value)
    {
        inputAnswer = value;
        GameMenu.Instance.AnswerInput.text = value.ToString();
    }

    private void Start()
    {
        GameMenu.Instance.questionAnswerOutputContinuneButton.GetComponent<Button>().onClick.AddListener(() => {
            if (correct)
            {
                GameMenu.Instance.rewardAmmoText.text = rewardAmmoAmount + " Ammo";
                GameMenu.Instance.rewardHealthText.text = rewardHealthAmount + " Health";
                GameMenu.Instance.questionAnswer.SetActive(false);
                GameMenu.Instance.selectReward.SetActive(true);
            }
            else
            {
                Player.Instance.DecreaseHealth(rewardHealthAmount);
                GameManager.Instance.currentMode = CurrentMode.GAME;
                GameMenu.Instance.questionAnswerOutputContinuneButton.transform.localPosition = questionAnswerOutputContinuneButtonBeforeAnimatePos;
                GameMenu.Instance.questionAnswerOutputInformation.transform.localPosition = questionAnswerOutputInformationBeforeAnimatePos;
                GameMenu.Instance.questionAnswerOutputResult.transform.localPosition = questionAnswerOutputResultBeforeAnimatePos;
                GameMenu.Instance.questionRewardText.GetComponent<TextMeshProUGUI>().color = questionRewardTextBeforeAnimateColor;
                GameMenu.Instance.questionText.transform.localPosition = questionTextBeforeAnimatePos;
                GameMenu.Instance.questionText.GetComponent<TextMeshProUGUI>().fontSize = questionTextBeforeAnimateFontSize;
                GameMenu.Instance.questionTitle.transform.localPosition = questionTitleBeforeAnimatePos;
            }
        });
    }

    public void OnConfirmReward()
    {
        if (rewardSelector.currentSelect == 0)
        {
            Player.Instance.IncreaseHealth(rewardHealthAmount);
        }
        else if (rewardSelector.currentSelect == 1)
        {
            Player.Instance.ammoAmount += rewardAmmoAmount;
        }
        GameManager.Instance.currentMode = CurrentMode.GAME;
        GameMenu.Instance.questionAnswerOutputContinuneButton.transform.localPosition = questionAnswerOutputContinuneButtonBeforeAnimatePos;
        GameMenu.Instance.questionAnswerOutputInformation.transform.localPosition = questionAnswerOutputInformationBeforeAnimatePos;
        GameMenu.Instance.questionAnswerOutputResult.transform.localPosition = questionAnswerOutputResultBeforeAnimatePos;
        GameMenu.Instance.questionRewardText.GetComponent<TextMeshProUGUI>().color = questionRewardTextBeforeAnimateColor;
        GameMenu.Instance.questionText.transform.localPosition = questionTextBeforeAnimatePos;
        GameMenu.Instance.questionText.GetComponent<TextMeshProUGUI>().fontSize = questionTextBeforeAnimateFontSize;
        GameMenu.Instance.questionTitle.transform.localPosition = questionTitleBeforeAnimatePos;
    }

    private IEnumerator Result(bool correct)
    {
        questionAnswerOutputResultBeforeAnimatePos = GameMenu.Instance.questionAnswerOutputContinuneButton.transform.localPosition;
        questionAnswerOutputInformationBeforeAnimatePos = GameMenu.Instance.questionAnswerOutputContinuneButton.transform.localPosition;
        questionAnswerOutputContinuneButtonBeforeAnimatePos = GameMenu.Instance.questionAnswerOutputContinuneButton.transform.localPosition;
        questionRewardTextBeforeAnimateColor = GameMenu.Instance.questionRewardText.GetComponent<TextMeshProUGUI>().color;
        GameMenu.Instance.questionAnswerOutputInformationNumber.GetComponent<TextMeshProUGUI>().text = answer.ToString();
        GameMenu.Instance.questionObject.SetActive(false);
        GameMenu.Instance.questionObject2.SetActive(false);
        GameMenu.Instance.questionAnswer.SetActive(true);
        GameMenu.Instance.questionRewardText.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
        GameMenu.Instance.questionAnswerOutputResult.GetComponent<TextMeshProUGUI>().color = correct ? Color.green 
            : Color.red;
        GameMenu.Instance.questionAnswerOutputResult.GetComponent<TextMeshProUGUI>().text = correct ? "Correct" : "Wrong";
        yield return new WaitForSeconds(0.8f);

        this.correct = correct;

        GameMenu.Instance.questionAnswerOutputResult.LeanMoveLocal(new Vector2(-4.196167e-05f, 225), 0.5f).
            setEaseOutQuad();
        source.PlayOneShot(correct ? correctSound : wrongSound);
        yield return new WaitForSeconds(1.5f);
        GameMenu.Instance.questionAnswerOutputInformation.LeanMoveLocal(new Vector2(0, 37), 0.5f).
            setEaseOutQuad();
        GameMenu.Instance.questionAnswerOutputContinuneButton.LeanMoveLocal(new Vector2(5.8493e-05f, -250), 0.5f).
            setEaseOutQuad();
        GameMenu.Instance.questionRewardText.GetComponent<TextMeshProUGUI>().text = correct ? "" 
            : string.Format("-{0} Health", rewardHealthAmount);

        yield return new WaitForSeconds(1f);
        LeanTween.value(GameMenu.Instance.questionRewardText, new Color(0,0,0,0), correct ? Color.green 
            : Color.red, 1f).setOnUpdate(
            (Color val) =>
            GameMenu.Instance.questionRewardText.GetComponent<TextMeshProUGUI>().color = val);
    }

    public void Summit()
    {
        rewardHealthAmount = Random.Range(5, 100);
        rewardAmmoAmount = Random.Range(3, 10);
        if (inputAnswer == answer)
        {
            StartCoroutine(Result(true));
        } else
        {
            StartCoroutine(Result(false));
        }
        GameMenu.Instance.result = 0;
        inputAnswer = 0;
        isAsking = false;
    }
}
