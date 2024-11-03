using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum Difficulties
{
    EASY = 0,
    NORMAL,
    HARD
}

public enum CurrentMode
{
    PAUSE,
    QUESTION,
    GAME,
    GAMEOVER,
    SELECT_INPUT,
    SELECT_DIFFICULTY,
    WIN,
}

public abstract class GameplayMonoBehaviour : MonoBehaviour
{
    [SerializeField] private bool pauseable = true;
    protected virtual void PauseableUpdate() { }
    protected virtual void PauseableFixedUpdate() { }
    protected virtual void NotifyPause() { }
    protected virtual void NotifyResume() { }
    public virtual void OnGameStart() { }
    private bool wasPaused = false;

    private void Update()
    {
        if (!GameManager.Instance.IsPause || !pauseable)
        {
            PauseableUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsPause || !pauseable)
        {
            if (wasPaused)
            {
                NotifyResume();
                wasPaused = false;
            }

            PauseableFixedUpdate();
        }
        else if (!wasPaused)
        {
            NotifyPause();
            wasPaused = true;
        }
    }
}

public class GameManager : MonoBehaviour
{
    public GameObject prefab;
    public CurrentMode currentMode;
    public CurrentMode lastCurrentModeBeforePause { get; private set; }
    public Difficulties currentDifficulty { get; private set; }
    public bool IsPause { get; private set; }
    public static GameManager Instance { get; private set; }
    public PlayerInput playerControls { get; private set; }
    private InputAction pause;
    private InputAction fullScreen;
    [SerializeField] private GameObject terrain;
    [SerializeField] private GameObject postProcessing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            playerControls = new PlayerInput();
            terrain.SetActive(true);
            postProcessing.SetActive(true);
        }
    }

    private void OnEnable()
    {
        pause = playerControls.Player.Pause;
        pause.Enable();
        pause.performed += Pause;

        fullScreen = playerControls.Player.Fullscreen;
        fullScreen.Enable();
        fullScreen.performed += Fullscreen;
    }

    private void OnDisable()
    {
        pause.Disable();
        fullScreen.Disable();
    }

    void Start()
    {
        Application.targetFrameRate = -1;
        Camera.main.farClipPlane = 1000f;
        currentMode = CurrentMode.SELECT_INPUT;
        GameMenu.Instance.InputSelector.GetComponent<Selector>().onConfirm = (isFirst) =>
        {
            if (isFirst)
            {
                Settings.Instance.inputMode = InputMode.Keyboard;
                GameMenu.Instance.IOSAndroidControl.SetActive(false);
                GameMenu.Instance.specificStatus.SetActive(false);
                Settings.Instance.sprintLock = false;
            }
            else
            {
                Settings.Instance.inputMode = InputMode.Joystick;
                GameMenu.Instance.IOSAndroidControl.SetActive(true);
                RectTransform rectTransform = GameMenu.Instance.moveAbleStatus.GetComponent<RectTransform>();
                rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 752);
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
                GameMenu.Instance.specificStatus.SetActive(true);
                Settings.Instance.sprintLock = true;
            }
            GameMenu.Instance.InputSelector.SetActive(false);
            currentMode = CurrentMode.SELECT_DIFFICULTY;
        };

        if (Settings.Instance.questionTime == 0) Settings.Instance.questionTime = 10f;
        GameMenu.Instance.pauseMenu.SetActive(false);
    }

    public void OnConfirmSelectDifficulty()
    {
        currentDifficulty = (Difficulties) GameMenu.Instance.SelectDifficultySelector.currentSelect; 
        GameMenu.Instance.DifficultySelectorMenu.SetActive(false);
        IEnumerable<GameplayMonoBehaviour> objects =
        FindObjectsOfType<MonoBehaviour>().OfType<GameplayMonoBehaviour>();
        foreach (GameplayMonoBehaviour obj in objects)
        {
            obj.OnGameStart();
        }
        currentMode = CurrentMode.GAME;
        GameMenu.Instance.StatusMenu.SetActive(true);
        GameMenu.Instance.ControlsMenu.SetActive(true);
        Camera.main.farClipPlane = 100f;
    }

    public void Pause()
    {
        IsPause = true;
        Physics.simulationMode = SimulationMode.Script;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnPause()
    {
        IsPause = false;
        GameMenu.Instance.pauseMenu.SetActive(false);
        Physics.simulationMode = SimulationMode.FixedUpdate;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        switch (currentMode)
        {
            case CurrentMode.GAME:
                UnPause();
                GameMenu.Instance.pauseMenu.SetActive(false);
                GameMenu.Instance.questionMenu.SetActive(false);
                break;
            case CurrentMode.QUESTION:
                Pause();
                GameMenu.Instance.pauseMenu.SetActive(false);
                GameMenu.Instance.questionMenu.SetActive(true);
                break;
            case CurrentMode.GAMEOVER:
                Pause();
                GameMenu.Instance.gameOverMenu.SetActive(true);
                break;
            case CurrentMode.PAUSE:
                Pause();
                GameMenu.Instance.pauseMenu.SetActive(true);
                break;
            case CurrentMode.SELECT_INPUT:
                Pause();
                GameMenu.Instance.InputSelector.SetActive(true);
                break;
            case CurrentMode.SELECT_DIFFICULTY:
                Pause();
                GameMenu.Instance.DifficultySelectorMenu.SetActive(true);
                break;
            case CurrentMode.WIN:
                Pause();
                GameMenu.Instance.gameWinMenu.SetActive(true);
                break;
        }
    }
    public void Pause(InputAction.CallbackContext context)
    {
        if (currentMode != CurrentMode.GAMEOVER && currentMode != CurrentMode.SELECT_INPUT)
        {
            if (currentMode != CurrentMode.PAUSE)
            {
                lastCurrentModeBeforePause = currentMode;
                currentMode = CurrentMode.PAUSE;
            }
            else if (currentMode != CurrentMode.GAME)
            {
                currentMode = lastCurrentModeBeforePause;
            }
        }
    }

    private void Fullscreen(InputAction.CallbackContext context) {
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }
        else if (!Screen.fullScreen)
        {
            Screen.fullScreen = true;
        }
    }

    public void GameOver()
    {
        currentMode = CurrentMode.GAMEOVER;
    }

    public void GameWin()
    {
        currentMode = CurrentMode.WIN;
    }
}
