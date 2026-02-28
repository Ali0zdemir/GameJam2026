using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
    }
}