using UnityEngine;

[CreateAssetMenu(fileName = "New Health Data", menuName = "Game Data/Health Data")]
public class HealthData : ScriptableObject
{
    public float maxHealth = 100f;
    public float currentHealth;

    // Oyun başladığında / asset enable olduğunda canı sıfırla
    private void OnEnable()
    {
        currentHealth = maxHealth;
    }
}