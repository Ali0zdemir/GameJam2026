using UnityEngine;

public class WheelSpin : MonoBehaviour
{
    public float speed = 200f; 
    private float currentX;

    void Start()
    {
        currentX = transform.localEulerAngles.x;
    }

    void Update()
    {
        currentX += speed * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(currentX, 90f, 90f);
    }
}