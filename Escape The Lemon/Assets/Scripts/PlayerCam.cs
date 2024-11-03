using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : GameplayMonoBehaviour
{
    public static PlayerCam Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            cacheTransform = transform;
            cacheModelTransform = models.transform;
            cacheAttchmentOnCameraTransform = attchmentOnCamera.transform;
            cacheGamepadCurrent = Gamepad.current;
        }
    }

    public Transform orientation;
    [SerializeField] private GameObject models;
    [SerializeField] private GameObject attchmentOnCamera;
    private InputAction mouseXInput;
    private InputAction mouseYInput;
    private InputAction touchInput;
    private float mouseX, mouseY;
    private Transform cacheTransform;
    private Transform cacheModelTransform;
    private Transform cacheAttchmentOnCameraTransform;
    private Gamepad cacheGamepadCurrent;
    private float currentXValue, currentYValue;

    public float XRotation { get; private set; }
    public float YRotation { get; private set; }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnGameStart()
    {
        if (Settings.Instance.inputMode == InputMode.Keyboard)
        {
            mouseXInput = GameManager.Instance.playerControls.Player.MouseX;
            mouseXInput.Enable();
            mouseYInput = GameManager.Instance.playerControls.Player.MouseY;
            mouseYInput.Enable();
            mouseXInput.performed += contex =>
            {
                mouseX = Mouse.current.delta.x.ReadUnprocessedValue() * Time.deltaTime * Settings.Instance.sensitivity;
            };
            mouseYInput.performed += contex =>
            {
                mouseY = Mouse.current.delta.y.ReadUnprocessedValue() * Time.deltaTime * Settings.Instance.sensitivity;
            };
        } else
        {
            touchInput = GameManager.Instance.playerControls.Player.CameraJoystickControl;
            touchInput.Enable();
            touchInput.performed += contex =>
            {
                Vector2 InputVector = Gamepad.current.rightStick.ReadUnprocessedValue().normalized;
                mouseX = InputVector.x * Time.deltaTime * Settings.Instance.sensitivity;
                mouseY = InputVector.y * Time.deltaTime * Settings.Instance.sensitivity;
            };
        }
    }

    private void OnDisable()
    {
        if (Settings.Instance.inputMode == InputMode.Keyboard)
        {
            mouseXInput.Disable();
            mouseYInput.Disable();
        } else
        {
            touchInput.Disable();
        }
    }

    protected override void NotifyPause()
    {
        attchmentOnCamera.transform.rotation = transform.rotation;
        attchmentOnCamera.transform.position = transform.position;
    }

    protected override void PauseableUpdate()
    {
        if (Settings.Instance.inputMode == InputMode.Joystick)
        {
            Vector2 InputVector = Gamepad.current.rightStick.ReadUnprocessedValue().normalized;
            mouseX = InputVector.x * Time.deltaTime * Settings.Instance.sensitivity;
            mouseY = InputVector.y * Time.deltaTime * Settings.Instance.sensitivity;
        }

        YRotation += mouseX;
        XRotation -= mouseY;

        XRotation = Mathf.Clamp(XRotation, -45f, 45f);

        cacheTransform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        orientation.rotation = Quaternion.Euler(0, YRotation, 0);

        cacheModelTransform.rotation = Quaternion.Euler(0, YRotation, 0);
        cacheAttchmentOnCameraTransform.rotation = transform.rotation;
        cacheAttchmentOnCameraTransform.position = transform.position;
    }
}
