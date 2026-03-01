using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireInterval = 3f;

    [Header("Animation")]
    public Animator anim;
    public string fireAnimTrigger = "Fire";

    float timer;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            timer = 0f;
            Fire();
        }
    }

    void Fire()
    {
        if (fireballPrefab == null || firePoint == null) return;

        if (anim != null)
            anim.SetTrigger(fireAnimTrigger);

        Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
    }
}