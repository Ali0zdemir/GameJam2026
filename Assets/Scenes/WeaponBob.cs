using UnityEngine;

public class WeaponBob : MonoBehaviour
{
    [Header("Yürüme Bob")]
    public float walkBobSpeed = 8f;
    public float walkBobAmountX = 0.003f;
    public float walkBobAmountY = 0.002f;

    [Header("Koşma Bob")]
    public float sprintBobSpeed = 14f;
    public float sprintBobAmountX = 0.007f;
    public float sprintBobAmountY = 0.005f;

    [Header("Smooth")]
    public float smoothSpeed = 6f;

    Vector3 originalPosition;
    float timer;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        bool isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);

        // Koşuyorsa sprint değerleri, yürüyorsa walk değerleri
        float currentBobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
        float currentBobX = isSprinting ? sprintBobAmountX : walkBobAmountX;
        float currentBobY = isSprinting ? sprintBobAmountY : walkBobAmountY;

        if (isMoving)
        {
            timer += Time.deltaTime * currentBobSpeed;

            Vector3 bobOffset = new Vector3(
                Mathf.Sin(timer) * currentBobX,
                Mathf.Sin(timer * 2f) * currentBobY,
                0f
            );

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalPosition + bobOffset,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            timer = Mathf.Lerp(timer, 0f, Time.deltaTime * smoothSpeed);
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}