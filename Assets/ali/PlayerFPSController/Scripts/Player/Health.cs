using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour,IDamageable,IHealth
{
    public event System.Action OnDeath;
    public event System.Action OnHealthValueChanged;
    public Slider slider;
    int health = 100;
    public bool isDead;
    void Start()
    {
        isDead = false;
        slider.minValue = 0;
        slider.maxValue = health;
        slider.value = health;
        OnHealthValueChanged += AdjustSlider;
    }
    public void TakeDamage(int amount)
    {
        health -= amount;
        OnHealthValueChanged?.Invoke();
        if(health<=0)
        {
            Die();
            health = 0;
        }
        Debug.Log(health);
    }

    public void Die()
    {
        OnDeath?.Invoke();
        isDead = true;
    }

    public void AdjustSlider()
    {
        slider.value = health;
        if(health <= 0)
        {
            slider.value = 0;
        }
    }

}
