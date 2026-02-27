using UnityEngine;

public class JumpingState : IState
{
    public void EnterState(PlayerMovementA player)
    {
        player.rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
    }

    public void ExitState(PlayerMovementA player)
    {
        Debug.Log("Exiting Jumping State");
    }

    public void UpdateState(PlayerMovementA player)
    {
        player.MovementInput();
    }
}
