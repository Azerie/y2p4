using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Controls;
using FMODUnity;

public class PlayerControls : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character")]
    [SerializeField] private float MoveSpeed = 4.0f;
    [Tooltip("Sprint speed of the character")]
    [SerializeField] private float SprintSpeed = 6.0f;
    [Tooltip("Crouch speed of the character")]
    [SerializeField] private float CrouchSpeed = 2f;
    [Tooltip("Rotation speed of the character")]
    [SerializeField] private float Sensitivity = 1.0f;
    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float SpeedChangeRate = 10.0f;

    // [Space(10)]
    // [Tooltip("The height the player can jump")]
    // [SerializeField] private float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float Gravity = -15.0f;
    // [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    // [SerializeField] private float JumpTimeout = 0.1f;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private GameObject sprintPopup;

    [Space(10)]
    [Header("Head bop settings")]
    [SerializeField] private float speedMult = 20f;
    [SerializeField] private float maxHeightDiff = 0.05f;

    [Space(10)]
    [Header("Player Grounded")]
    private bool Grounded = true;
    [Tooltip("How far grounded check is offset")]
    [SerializeField] private float GroundedOffset = -0.4f;
    [Tooltip("The radius of the grounded check")]
    [SerializeField] private float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask GroundLayers;
    [SerializeField] private float maxSlopeAngle = 50f;
    [SerializeField] private float slopeCheckDistance = 0.5f;


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
    [SerializeField] private float HitboxStandingHeight = 1.5f;
    [SerializeField] private float HitboxCrouchHeight = 1f;
    [SerializeField] private float HeadStandingHeight = 1.375f;
    [SerializeField] private float HeadCrouchHeight = 0.875f;

    [Space(10)]
    [SerializeField] private string FailScene = "MainMenu";
    [SerializeField] private float KillAnimationRotationTime = 0.3f;
    [SerializeField] private float KillAnimationCameraAngle = -10f;

    [Header("FMOD State Sounds")]
    public EventReference m_OutOfStaminaEventPath = new EventReference();
    public EventReference m_RecoveringStaminaEventPath = new EventReference();

    [Space(10)]
    [Header("Debug values")]
    [SerializeField] private float _health;
    [SerializeField] private float _stamina;
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationVelocity;
    [SerializeField] private float _verticalVelocity;
    [SerializeField] private float _terminalVelocity = 53.0f;
    [SerializeField] private bool _isSprinting = false;
    [SerializeField] private bool _isCrouching = false;
    [SerializeField] private bool _isInKillAnimation = false;
    // [SerializeField] private float _jumpTimeoutDelta = 0f;
    [SerializeField] private float _cameraYrotation = 0f;

    [SerializeField] private Vector2 moveInput = Vector2.zero;
    [SerializeField] private Vector2 lookInput = Vector2.zero;
    [SerializeField] private bool isEnabled = true;
    [SerializeField] private Vector3 currentSlopeNormal = Vector3.up;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private Quaternion targetHeadRotation;
    [SerializeField] private ImageFading staminaUI;
    [SerializeField] private float killAnimationTimer = 0;

    private Rigidbody _rb;
    private PlayerInteraction _pickupHandler;
    private CapsuleCollider _hitbox;
    [SerializeField] private Transform head;
    [SerializeField] private SkillCheck skillcheck;
    [SerializeField] private bool isBopping;
    float bopStartTime;
    float bopStartOffset;
    private EvidenceJournal evidenceJournal;
    private PauseMenu pauseMenu;

    public InputActionAsset asset;
    private InputAction inputAction;
    private ButtonControl buttonControl;
    private bool isFirstSprint = true;
    private bool isPauseButtonEnabled = true;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _hitbox = GetComponentInChildren<CapsuleCollider>();
        _pickupHandler = GetComponentInChildren<PlayerInteraction>();
        _health = MaxHealth;
        _stamina = MaxStamina;
        isEnabled = true;
        head = GetComponentInChildren<PlayerInteraction>().transform;
    }

    private void Awake()
    {
        pauseMenu = FindObjectOfType<PauseMenu>();
        evidenceJournal = FindObjectOfType<EvidenceJournal>();
        skillcheck = FindObjectOfType<SkillCheck>();
        deathScreen = FindObjectOfType<DeathScreen>();
        GameObject staminaUITemp = GameObject.Find("StaminaUI");
        if (staminaUITemp != null) { staminaUI = staminaUITemp.GetComponent<ImageFading>(); }
        Cursor.visible = false;

        inputAction = asset.FindAction("Sprint");
        buttonControl = (ButtonControl)inputAction.controls[0];
        inputAction.Enable();
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.up * HitboxStandingHeight, Color.blue);

        GroundedCheck();
        ApplyGravity();
        HeadBopping();
        if (isEnabled)
        {
            Move();
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void ApplyGravity()
    {
        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity > -_terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
        
        if (Grounded)
        {
            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -0.1f;
            }
        }
    }

    private void Move()
    {
        if (buttonControl.wasPressedThisFrame)
        {
            if (_isCrouching)
            {
                OnCrouch();
            }
            if (!_isCrouching)
            {
                _isSprinting = true;
                StartHeadBop();
            }

            if (isFirstSprint)
            {
                if (sprintPopup != null)
                {
                    sprintPopup.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("Popup reminder about sprint is not assigned");
                }
                isFirstSprint = false;
            }
        }
        else if (buttonControl.wasReleasedThisFrame)
        {
            _isSprinting = false;
            StopHeadBop();
        }

        // set target speed based on move speed, sprint speed and if sprint is pressed/crouch if crouching
        float targetSpeed = _isSprinting ? SprintSpeed : (_isCrouching ? CrouchSpeed : MoveSpeed);

        if (_isSprinting)
        {
            float oldStamina = _stamina;
            _stamina -= Time.deltaTime;

            if (_stamina <= 0 && oldStamina > 0)
            {
                _isSprinting = false;
                if(!m_OutOfStaminaEventPath.IsNull)
                {
                    RuntimeManager.PlayOneShot(m_OutOfStaminaEventPath);
                }
                StopHeadBop();

                if (staminaUI != null)
                {
                    staminaUI.FadeIn();
                }
            }
            else if (_stamina >= MaxStamina / 4 && oldStamina < MaxStamina / 4)
            {
                if (!m_RecoveringStaminaEventPath.IsNull)
                {
                    RuntimeManager.PlayOneShot(m_RecoveringStaminaEventPath);
                }

                if (staminaUI != null)
                {
                    staminaUI.FadeOut();
                }
            }
        }
        else
        {
            if (_stamina <= MaxStamina)
            {
                _stamina += Time.deltaTime * MaxStamina / StaminaRecoveryRate;
            }
        }

        // if there is no input, set the target speed to 0
        if (moveInput == Vector2.zero)
        {
            targetSpeed = 0.0f;
            _isSprinting = false;
            StopHeadBop();
        }

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_rb.velocity.x, 0.0f, _rb.velocity.z).magnitude;
        if (Grounded && SlopeCheck())
        {
            currentHorizontalSpeed = (Quaternion.FromToRotation(currentSlopeNormal, Vector3.up) * _rb.velocity).magnitude;
        }

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

        // if we're on a walkable slope, rotate the gravity to be perpendicular to the slope and movement to be parallel
        if (Grounded && SlopeCheck())
        {
            _rb.velocity = Quaternion.FromToRotation(Vector3.up, currentSlopeNormal) * _rb.velocity;
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    // private void OnSprint()
    // {
    //     if (isEnabled)
    //     {
    //         if (_isCrouching)
    //         {
    //             OnCrouch();
    //         }
    //         if (!_isCrouching)
    //         {
    //             _isSprinting = !_isSprinting;
    //         }
    //     }
    // }

    private void OnSpace()
    {
        // if (isEnabled && Grounded && _jumpTimeoutDelta <= 0.0f)
        // {
        //     // the square root of H * -2 * G = how much velocity needed to reach desired height
        //     _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        // }

        // FindObjectOfType<SkillCheck>().DebugFunc();
        skillcheck.EndMinigame();
    }

    private void OnInteract()
    {
        _pickupHandler.Interact();
    }

    private void OnLook(InputValue value)
    {
        if (isEnabled)
        {
            _rotationVelocity = value.Get<Vector2>().x * Sensitivity;

            // rotate the player left and right
            // _rb.MoveRotation(transform.rotation * Quaternion.Euler(Vector3.up * _rotationVelocity));
            transform.rotation = transform.rotation * Quaternion.Euler(Vector3.up * _rotationVelocity);

            Transform head = GetComponentInChildren<Camera>().transform.parent;
            _cameraYrotation -= value.Get<Vector2>().y * Sensitivity;
            _cameraYrotation = Math.Clamp(_cameraYrotation, MinCameraAngle, MaxCameraAngle);

            Quaternion newRotation = Quaternion.Euler(_cameraYrotation, 0, 0);

            head.localRotation = newRotation;
        }
    }

    private void OnPause()
    {
        if (isPauseButtonEnabled)
        {
            pauseMenu.OnPauseButton();
        }
    }

    private void OnCrouch()
    {
        if (isEnabled)
        {
            RaycastHit hit;
            if (!_isCrouching)
            {
                _hitbox.height = HitboxCrouchHeight;
                _hitbox.center = new Vector3(0, HitboxCrouchHeight / 2, 0);
                head.localPosition = new Vector3(0, HeadCrouchHeight, 0);
                _isSprinting = false;
                _isCrouching = true;
            }
            else if (!Physics.Raycast(transform.position, transform.up, out hit, HitboxStandingHeight, GroundLayers))
            {
                _hitbox.height = HitboxStandingHeight;
                _hitbox.center = new Vector3(0, HitboxStandingHeight / 2, 0);
                head.localPosition = new Vector3(0, HeadStandingHeight, 0);
                _isCrouching = false;
            }   
            else
            {
                Debug.Log("Couldn't uncrouch, hit " + hit.transform.gameObject.name);
            }
        }
    }

    private void OnScroll(InputValue value)
    {
        if (isEnabled)
        {
            // Debug.Log(value.Get<Vector2>());
            PlayerInventory.GetInstance().SwapSelectedItem(-Mathf.RoundToInt(value.Get<Vector2>().y / 120));        
        }
    }

    private void OnJournal()
    {
        evidenceJournal.OnJournalButton();
        if (evidenceJournal.IsEnabled())
        {
            isPauseButtonEnabled = false;
        }
        else
        {
            isPauseButtonEnabled = true;
        }
    }

    private bool SlopeCheck()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, slopeCheckDistance, GroundLayers))
        {
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            if (angle < maxSlopeAngle)
            {
                currentSlopeNormal = hit.normal;
                return true;
            }
        }
        return false;
    }

    public void Die()
    {
        DisableMovement();
        isPauseButtonEnabled = false;
        if (deathScreen != null)
        {
            deathScreen.Die();
        }
        else
        {
            Debug.Log("Death screen not set, loading fail scene instead");
            SceneManager.LoadScene(FailScene);
        }
    }

    public void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            Debug.Log("Respawn point not set");
        }
        EnableMovement();
        isPauseButtonEnabled = true;
    }

    private IEnumerator KillAnimationCameraPan(EnemyBehaviour target)
    {
        killAnimationTimer = 0;
        // float step = KillAnimationRotationSpeed * Time.deltaTime;
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        Vector3 relativePos = target.transform.position - transform.position;
        targetRotation = Quaternion.LookRotation(relativePos);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

        targetHeadRotation = Quaternion.Euler(KillAnimationCameraAngle, 0, 0);
        // Debug.Log(targetHeadRotation.eulerAngles);
        // Debug.Log(head.localEulerAngles);

        // var aforwardA = head.transform.localRotation * Vector3.forward;
        // var aforwardB = targetHeadRotation * Vector3.forward;
        // Debug.Log(aforwardA);
        // Debug.Log(aforwardB);
        // var aangleA = Mathf.Atan2(aforwardA.y, aforwardA.z) * Mathf.Rad2Deg;
        // var aangleB = Mathf.Atan2(aforwardB.y, aforwardB.z) * Mathf.Rad2Deg;
        // var aangleDiff = Mathf.DeltaAngle(aangleA, aangleB);
        // Debug.Log(aangleDiff);


        // float curAngle = Quaternion.Angle(transform.rotation, targetRotation);

        while (killAnimationTimer < KillAnimationRotationTime)
        {
            // current and new rotation as float ranging forom 0 to 1
            float curRot = killAnimationTimer / KillAnimationRotationTime;
            // formula increasing between (0, 0) and (1, 1), slower increase near the start and near the end
            curRot = -2 * Mathf.Pow(curRot, 3) + 3 * Mathf.Pow(curRot, 2);
            // Debug.Log("time: " + (lookAroundTimer / AlertToRoamingTime).ToString() + "rotation: " + curRot);
            killAnimationTimer += Time.deltaTime;
            float newRot = killAnimationTimer / KillAnimationRotationTime;
            newRot = -2 * Mathf.Pow(newRot, 3) + 3 * Mathf.Pow(newRot, 2);

            // get a "forward vector" for each rotation
            var forwardA = transform.forward;
            var forwardB = targetRotation * Vector3.forward;

            // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
            var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
            var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;

            // get the signed difference in these angles
            var angleDiff = Mathf.DeltaAngle(angleA, angleB);

            float fullAngle = angleDiff / killAnimationTimer * KillAnimationRotationTime;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, (newRot - curRot) * fullAngle, 0));
            // curAngle = Quaternion.Angle(transform.rotation, targetRotation);

            // same for the head rotation
            forwardA = head.transform.localRotation * Vector3.forward;
            forwardB = targetHeadRotation * Vector3.forward;
            angleA = Mathf.Atan2(forwardA.y, forwardA.z) * Mathf.Rad2Deg;
            angleB = Mathf.Atan2(forwardB.y, forwardB.z) * Mathf.Rad2Deg;
            angleDiff = Mathf.DeltaAngle(angleA, angleB);

            fullAngle = angleDiff / killAnimationTimer * KillAnimationRotationTime;
            head.transform.localRotation = Quaternion.Euler(head.transform.localRotation.eulerAngles + new Vector3(-(newRot - curRot) * fullAngle, 0, 0));

            yield return null;
        }
    }

    public void StartKillAnimation(EnemyBehaviour target)
    {
        DisableMovement();
        _isInKillAnimation = true;
        StartCoroutine(KillAnimationCameraPan(target));
        // transform.LookAt(target.transform);
        // head.transform.LookAt(target.transform.position + new Vector3(0, target.GetHeight(), 0));
    }

    public void ExitKillAnimation()
    {
        EnableMovement();
        _isInKillAnimation = false;
    }

    private void HeadBopping()
    {
        if (isBopping)
        {
            head.transform.localPosition = new Vector3(0, HeadStandingHeight + Mathf.Sin((Time.time - bopStartTime + bopStartOffset) * speedMult) * maxHeightDiff, 0);
        }
    }

    private void StartHeadBop()
    {
        isBopping = true;
        bopStartTime = Time.time;
        bopStartOffset = (Mathf.Asin((head.transform.localPosition.y - HeadStandingHeight) / maxHeightDiff) / speedMult) * 2 * MathF.PI;
    }

    private void StopHeadBop()
    {
        isBopping = false;
    }

    public void SetCameraSensitivity(float pSensitivity)
    {
        Sensitivity = pSensitivity;
    }

    public float GetCameraSensitivity()
    {
        return Sensitivity;
    }

    public void DisableMovement() // disables all controls, not just movement
    {
        isEnabled = false;
        _rb.isKinematic = true;
    }

    public void EnableMovement()
    {
        isEnabled = true;
        if(_rb != null)
        {
            _rb.isKinematic = false;
        }
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

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public bool IsSprinting()
    {
        return _isSprinting;
    }

    public bool IsInKillAnimation()
    {
        return _isInKillAnimation;
    }
}