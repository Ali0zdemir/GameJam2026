using System;
using UnityEngine;

public class PlayerMovementA : MonoBehaviour
{
    public float playerSpeed;
    public float walkSpeed = 8f;
    public float sprintSpeed = 14f;
    public float crouchSpeed = 5f;
    public float startYScale;
    public float crouchYScale = 0.5f;
    public Rigidbody rb;
    public float jumpForce = 5f;
    public bool isGround;
    public bool isWalking;
    //private Health health;
    IState currentState;
    IState Idle = new IdleState();
    IState Jump = new JumpingState();
    IState Walk = new WalkingState();
    IState Sprint = new SprintingState();
    IState Crouch = new CrouchState();
    IState Death = new DeathState();
    

    void Start()
{
    isGround = true;
    rb = this.gameObject.GetComponent<Rigidbody>();
    startYScale = this.gameObject.transform.localScale.y;
    currentState = Idle;
    currentState.UpdateState(this);
}

    void Update()
    {
        MovementInput(); 
        JumpInput(); 
        EvaluateState();
        currentState.UpdateState(this);
    }

    public void EvaluateState()
    {
        if(!isGround){
            ChangeState(Jump);
        }
        else if(isGround && Input.GetKey(KeyCode.LeftControl))
        {
            ChangeState(Crouch);
        }
        else if(isWalking && Input.GetKey(KeyCode.LeftShift)){
            ChangeState(Sprint);
        }
        else if (isWalking){
            ChangeState(Walk);
        }
        else{
            ChangeState(Idle);
        }
    }

    public void ChangeState(IState newState)
    {
        if(currentState != newState)
        {
            currentState.ExitState(this);
            currentState = newState;
            currentState.EnterState(this);
        }
    }

    public void MovementInput()
{
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");

    Vector3 move =
        transform.forward * vertical +
        transform.right * horizontal;

    isWalking = move.sqrMagnitude > 0.001f;

    if (isWalking)
    {
        move.Normalize();
        rb.velocity = new Vector3(
            move.x * playerSpeed,
            rb.velocity.y,
            move.z * playerSpeed
        );
    }
    else
    {
        // FPS hissi = anında dur
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }
}


    public void JumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            isGround = false;
            isWalking = false;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }
}
