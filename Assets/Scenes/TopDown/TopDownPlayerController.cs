using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f; // dönüş hızı

    [Header("Sprite Settings")]
    public SpriteRenderer spriteRenderer;
    public bool flipSpriteOnMove = true;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody rb;
    private Vector3 moveInput;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Top-down: Y pozisyonu sabit, XZ rotasyonu kilitli, Y rotasyonu serbest
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ
                       | RigidbodyConstraints.FreezePositionY;

        // Top-down’da genelde gravity kapalı olur
        rb.useGravity = false;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");

        moveInput = new Vector3(horizontal, 0f, vertical).normalized;
        bool isMoving = moveInput.magnitude > 0.1f;

        // Hareket yönüne bak
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Sprite flip (opsiyonel)
        if (flipSpriteOnMove && spriteRenderer != null)
        {
            if (horizontal < 0) spriteRenderer.flipX = true;
            else if (horizontal > 0) spriteRenderer.flipX = false;
        }

        // Animasyon
        if (animator != null)
            animator.SetBool(IsWalking, isMoving);
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = moveInput * moveSpeed;
        rb.velocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z); // Y sabit
    }
}