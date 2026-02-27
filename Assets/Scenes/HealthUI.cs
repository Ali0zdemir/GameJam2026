using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public HealthData healthData;
    public Slider healthSlider;

    void Start()
    {
        // Set slider's max value to player's max health
        healthSlider.maxValue = healthData.maxHealth;
    }

    void Update()
    {
        // Update the slider value every frame
        healthSlider.value = healthData.currentHealth;

        // Hide the fill area completely if health is 0 or below
        if (healthData.currentHealth <= 0)
        {
            healthSlider.fillRect.gameObject.SetActive(false);
        }
        else
        {
            healthSlider.fillRect.gameObject.SetActive(true);
        }
    }
}