using UnityEngine;

public class AttackRangeTrigger : MonoBehaviour
{
    EnemyAI_Melee enemy;

    void Start()
    {
        enemy = GetComponentInParent<EnemyAI_Melee>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (enemy) enemy.OnAttackRangeEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (enemy) enemy.OnAttackRangeExit(other);
    }
}
