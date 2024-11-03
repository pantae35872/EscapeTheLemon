using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject settings;
    public GameObject store;
    public GameObject main;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI currentDamage;

    [Header("Settings Menu Sensitivity")]
    public GameObject sensitivitySlider;
    public GameObject sensitivityNumber;
    private InputAction fullscreen;

    [Header("Settings Menu Graphics")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Selector vsyncSelector;

    public PlayerInput playerControls;
    public static Menu Instance { get; private set; }

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
        }
    }


    public void Quit() {
        #if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    private void OnEnable()
    {
        fullscreen = playerControls.Player.Fullscreen;
        fullscreen.Enable();
        fullscreen.performed += contex =>
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
            else if (!Screen.fullScreen)
            {
                Screen.fullScreen = true;
            }
        };
    }

    private void OnDisable()
    {
        fullscreen.Disable();
    }

    public void Start()
    {
        ChangeToMain();
        float value = Settings.Instance.sensitivity;
        sensitivitySlider.GetComponent<Scrollbar>().value = (float) Utils.MapValue(value, 1, 1000, 0, 1);
        sensitivityNumber.GetComponent<TextMeshProUGUI>().text = ((int) value).ToString();
        Settings.Instance.sensitivity = value;
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.onValueChanged.AddListener(OnQualityValueChanged);
    }

    private void Update()
    {
        currentDamage.text = "CURRENT GUN DAMAGE: " + Settings.Instance.gunDamage;
    }

    public void ChangeToStore()
    {
        main.SetActive(false);
        settings.SetActive(false);
        store.SetActive(true);
    }

    public void ChangeToSettings()
    {
        main.SetActive(false);
        store.SetActive(false);
        settings.SetActive(true);
    }

    private void OnQualityValueChanged(int value)
    {
        vsyncSelector.IsFirst = QualitySettings.vSyncCount >= 1;
    } 

    public void SensitivityChanged()
    {
        float value = sensitivitySlider.GetComponent<Scrollbar>().value;
        sensitivityNumber.GetComponent<TextMeshProUGUI>().text = 
            Mathf.Floor((float)
            Utils.MapValue(value, 0, 1, 1, 1000)).ToString();
        Settings.Instance.sensitivity = (float) Utils.MapValue(value, 0, 1, 1, 1000);
    }

    public void ChangeQuality()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
    }

    public void OnVsyncChange()
    {
        if (vsyncSelector.IsFirst)
        {
            QualitySettings.vSyncCount = 1;
        } else
        {
            QualitySettings.vSyncCount = 0;
        }
    }

    public void ChangeToMain()
    {
        settings.SetActive(false);
        store.SetActive(false);
        main.SetActive(true);
    }

    public void Buy()
    {
        if (CoinSystem.Instance.Coins >= 50)
        {
            CoinSystem.Instance.DecreaseCoin(50);
            Settings.Instance.gunDamage += 2;
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
}
