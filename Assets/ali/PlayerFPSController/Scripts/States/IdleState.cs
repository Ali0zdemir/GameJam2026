using UnityEngine;

public class IdleState : IState
{
    public void EnterState(PlayerMovementA player)
    {
        Debug.Log("Entering Idle State");
    }

    public void ExitState(PlayerMovementA player)
    {
        Debug.Log("Exiting Idle State");
    }

    public void UpdateState(PlayerMovementA player)
    {
        Debug.Log("He is stopping");
    }
}
