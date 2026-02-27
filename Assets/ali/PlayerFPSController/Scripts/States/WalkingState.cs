using UnityEngine;

public class WalkingState : IState
{
    public void EnterState(PlayerMovementA player)
    {
        Debug.Log("Entering Walking State");
    }

    public void ExitState(PlayerMovementA player)
    {
        Debug.Log("Exiting Walking State");
    }

    public void UpdateState(PlayerMovementA player)
    {
        player.playerSpeed = player.walkSpeed;
        player.MovementInput();
    }

}
