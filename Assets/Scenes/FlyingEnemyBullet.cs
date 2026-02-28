using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBullet : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float lifetime = 4f;
    public float damage = 10f; // 10 hasar
    public GameObject hitEffect;

    [Header("Tag Ayarları")]
    public string playerTag = "Player";
    public string enemyTag = "Enemy";

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(enemyTag)) return;

        if (col.gameObject.CompareTag(playerTag))
        {
            // 3D player health kullan
            PlayerHealth3D playerHealth = col.gameObject.GetComponent<PlayerHealth3D>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.LookRotation(col.contacts[0].normal));

        Destroy(gameObject);
    }
}