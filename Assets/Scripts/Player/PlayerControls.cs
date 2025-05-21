using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerControls : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character")]
    [SerializeField] private float MoveSpeed = 4.0f;
    [Tooltip("Sprint speed of the character")]
    [SerializeField] private float SprintSpeed = 6.0f;
    [Tooltip("Rotation speed of the character")]
    [SerializeField] private float Sensitivity = 1.0f;
    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    [SerializeField] private float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float Gravity = -15.0f;
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    [SerializeField] private float JumpTimeout = 0.1f;

    [Space(10)]
    [Header("Player Grounded")]
    private bool Grounded = true;
    [Tooltip("How far grounded check is offset")]
    [SerializeField] private float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check")]
    [SerializeField] private float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask GroundLayers;

    [Space(10)]
    [Header("Camera")]
    [Tooltip("Lower camera constraint")]
    [SerializeField] private int MinCameraAngle = -50;
    [Tooltip("Higher camera constraint")]
    [SerializeField] private int MaxCameraAngle = 50;

    [Space(10)]
    [Header("Parameters")]
    [Tooltip("Max Health")]
    [SerializeField] private float MaxHealth = 100;
    [Tooltip("Max Stamina (in seconds)")]
    [SerializeField] private float MaxStamina = 5f;
    [Tooltip("Full stamina recovery time (in seconds)")]
    [SerializeField] private float StaminaRecoveryRate = 3f;
    [SerializeField] private float StandingHeight = 2f;
    [SerializeField] private float CrouchHeight = 1f;

    [Space(10)]
    [SerializeField] private string FailScene;

    [Space(10)]
    [Header("Debug values")]
    [SerializeField] private float _health;
    [SerializeField] private float _stamina;
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationVelocity;
    [SerializeField] private float _verticalVelocity;
    [SerializeField] private float _terminalVelocity = 53.0f;
    [SerializeField] private bool _isSprinting;
    [SerializeField] private bool _isCrouching;
    [SerializeField] private float _jumpTimeoutDelta = 0f;
    [SerializeField] private float _cameraYrotation = 0f;

    [SerializeField] private Vector2 moveInput = Vector2.zero;
    [SerializeField] private Vector2 lookInput = Vector2.zero;

    private Rigidbody _rb;
    private PlayerInteraction _pickupHandler;
    private CapsuleCollider _hitbox;
    private bool isEnabled = true;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _hitbox = GetComponentInChildren<CapsuleCollider>();
        _pickupHandler = GetComponentInChildren<PlayerInteraction>();
        _health = MaxHealth;
        _stamina = MaxStamina;
        isEnabled = true;
    }

    private void Awake()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        GroundedCheck();
        ApplyGravity();
        if (isEnabled)
        {
            Move();
        }

        Debug.DrawRay(transform.position, transform.up, Color.red, StandingHeight);
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void ApplyGravity()
    {
        if (Grounded)
        {
            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity > -_terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Move()
    {
        if (_stamina <= 0)
        {
            _isSprinting = false;
        }
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _isSprinting ? SprintSpeed : MoveSpeed;

        if (_isSprinting)
        {
            _stamina -= Time.deltaTime;
        }
        else
        {
            if (_stamina <= MaxStamina)
            {
                _stamina += Time.deltaTime * MaxStamina / StaminaRecoveryRate;
            }
        }

        // if there is no input, set the target speed to 0
        if (moveInput == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_rb.velocity.x, 0.0f, _rb.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 100000f) / 100000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // normalise input direction
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

        if (moveInput != Vector2.zero)
        {
            inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        }

        // move the player
        _rb.velocity = inputDirection.normalized * _speed + new Vector3(0.0f, _verticalVelocity, 0.0f);
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    private void OnSprint()
    {
        if (!_isCrouching)
        {
            _isSprinting = !_isSprinting;
        }
    }

    private void OnJump()
    {
        if (Grounded && _jumpTimeoutDelta <= 0.0f)
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }

    private void OnInteract()
    {
        _pickupHandler.Interact();
    }

    private void OnLook(InputValue value)
    {
        _rotationVelocity = value.Get<Vector2>().x * Sensitivity * Time.deltaTime;

        // rotate the player left and right
        transform.Rotate(Vector3.up * _rotationVelocity);

        Transform head = GetComponentInChildren<Camera>().transform.parent;
        _cameraYrotation -= value.Get<Vector2>().y * Sensitivity * Time.deltaTime;
        _cameraYrotation = Math.Clamp(_cameraYrotation, MinCameraAngle, MaxCameraAngle);

        Quaternion newRotation = Quaternion.Euler(_cameraYrotation, 0, 0);

        head.localRotation = newRotation;
    }

    private void OnPause()
    {
        GameObject pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
        pauseMenu.GetComponent<PauseMenu>().Pause();
    }

    private void OnCrouch()
    {
        Physics.Raycast(transform.position, transform.up, StandingHeight); // use this later to determine if player can stand up
        if (_isCrouching)
        {
            _hitbox.height = StandingHeight;
        }
        else
        {
            _hitbox.height = CrouchHeight;
        }
        _isCrouching = !_isCrouching;
    }

    private void OnScroll(InputValue value)
    {
        // Debug.Log(value.Get<Vector2>());
        PlayerInventory.GetInstance().SwapSelectedItem(-Mathf.RoundToInt(value.Get<Vector2>().y / 120));
    }

    public void SetCameraSensitivity(int pSensitivity)
    {
        Sensitivity = pSensitivity;
    }

    public void DisableMovement()
    {
        isEnabled = false;
    }

    public void EnableMovement()
    {
        isEnabled = true;
    }

    public float GetHealth()
    {
        return _health;
    }

    public float GetStamina()
    {
        return _stamina;
    }

    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    public float GetMaxStamina()
    {
        return MaxStamina;
    }
}