using UnityEngine;

public class SprintingState : IState
{
    public void EnterState(PlayerMovementA player)
    {
        Debug.Log("Entering Sprinting State");
    }

    public void ExitState(PlayerMovementA player)
    {
        Debug.Log("Exiting Sprinting State");
    }

    public void UpdateState(PlayerMovementA player)
    {
        player.playerSpeed = player.sprintSpeed;
        player.MovementInput();
    }

}