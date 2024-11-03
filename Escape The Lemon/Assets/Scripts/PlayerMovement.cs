using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : GameplayMonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    private InputAction move;
    private InputAction sprint;
    private InputAction jump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    private bool isLockSprint;
    private TextMeshProUGUI sprintLockStatus;
    private TextMeshProUGUI sprintStatus;
    private Transform cacheTransform;
    [SerializeField] private AudioClip walkingSound;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air,
    }

    private void Awake()
    {
        cacheTransform = transform;
    }

    private void OnEnable()
    {
        move = GameManager.Instance.playerControls.Player.Move;
        move.Enable();

        jump = GameManager.Instance.playerControls.Player.Jump;
        jump.Enable();
        jump.performed += HandleJump;

        sprint = GameManager.Instance.playerControls.Player.Sprint;
        sprint.Enable();
        sprint.performed += contex =>
        {
            if (Settings.Instance.sprintLock)
            {
                if (Settings.Instance.inputMode == InputMode.Joystick && !isLockSprint)
                {
                    isLockSprint = true;
                }
                else if (Settings.Instance.inputMode == InputMode.Joystick && isLockSprint)
                {
                    isLockSprint = false;
                }
            }
        };
    }

    private void OnDisable()
    {
        move.Disable();
        sprint.Disable();
        jump.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        sprintLockStatus = GameMenu.Instance.sprintLockStatus.GetComponent<TextMeshProUGUI>();
        sprintStatus = GameMenu.Instance.sprintStatus.GetComponent<TextMeshProUGUI>();
    }

    protected override void PauseableUpdate()
    {
        grounded = Physics.Raycast(cacheTransform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        sprintLockStatus.text = Settings.Instance.sprintLock ? "ON" : "OFF";
        sprintLockStatus.color = Settings.Instance.sprintLock ? new Color(0.2613873f, 1, 0, 1) : 
            new Color(1, 0.05490196f, 0, 1);
        sprintStatus.text = (state == MovementState.sprinting) ? "YES" : "NO";
        sprintStatus.color = (state == MovementState.sprinting) ? new Color(0.2613873f, 1, 0, 1) :
            new Color(1, 0.05490196f, 0, 1);

        if (state == MovementState.walking)
        {
            if (!Player.Instance.source.isPlaying)
            {
                Player.Instance.source.PlayOneShot(walkingSound);
            }
        }

        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }
    protected override void PauseableFixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = move.ReadValue<Vector2>().x;
        verticalInput = move.ReadValue<Vector2>().y;
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        if (readyToJump && grounded && !GameManager.Instance.IsPause)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if (grounded && sprint.ReadValue<float>() != 0 || isLockSprint)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        } else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        } else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(cacheTransform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
