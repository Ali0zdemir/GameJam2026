using System;
using UnityEngine;

public class PlayerMovementA : MonoBehaviour
{
    public float playerSpeed;
    public float walkSpeed = 12f;
    public Rigidbody rb;
    public float jumpForce = 5f;
    public bool isGround;
    public bool isWalking;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.25f;

    [Header("Gravity")]
    public float fallMultiplier = 3f;

    [Header("Yürüme Sesi")]
    public AudioClip[] footstepSounds;
    [Range(0f, 1f)] public float footstepVolume = 0.8f;
    public float footstepInterval = 4f; // Kaç saniyede bir adım sesi

    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public float footstepTimer;

    IState currentState;
    IState Idle = new IdleState();
    IState Jump = new JumpingState();
    IState Walk = new WalkingState();
    IState Death = new DeathState();

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    Vector3 dashDirection;

    void Start()
    {
        isGround = true;
        rb = this.gameObject.GetComponent<Rigidbody>();
        playerSpeed = walkSpeed;
        currentState = Idle;
        currentState.UpdateState(this);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 dir = transform.forward * v + transform.right * h;

            if (dir.sqrMagnitude > 0.001f)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                dashDirection = dir.normalized;
            }
        }

        MovementInput();
        JumpInput();
        EvaluateState();
        currentState.UpdateState(this);
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = new Vector3(dashDirection.x * dashSpeed, rb.velocity.y, dashDirection.z * dashSpeed);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        if (!isGround && rb.velocity.y < 0f)
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
    }

    public void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        AudioClip clip = footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip, footstepVolume);
    }

    public void EvaluateState()
    {
        if (!isGround)
            ChangeState(Jump);
        else if (isWalking)
            ChangeState(Walk);
        else
            ChangeState(Idle);
    }

    public void ChangeState(IState newState)
    {
        if (currentState != newState)
        {
            currentState.ExitState(this);
            currentState = newState;
            currentState.EnterState(this);
        }
    }

    public void MovementInput()
    {
        if (isDashing) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.forward * vertical + transform.right * horizontal;
        isWalking = move.sqrMagnitude > 0.001f;

        if (isWalking)
        {
            move.Normalize();
            rb.velocity = new Vector3(move.x * playerSpeed, rb.velocity.y, move.z * playerSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    public void JumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            isGround = false;
            isWalking = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGround = true;
    }
}