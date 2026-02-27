using UnityEngine;

public class CrouchState : IState
{
    public void EnterState(PlayerMovementA player)
    {
        player.transform.localScale = new Vector3(player.transform.localScale.x , player.crouchYScale , player.transform.localScale.z);
        player.rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    public void ExitState(PlayerMovementA player)
    {
        player.transform.localScale = new Vector3(player.transform.localScale.x , player.startYScale , player.transform.localScale.z);
    }

    public void UpdateState(PlayerMovementA player)
    {
        player.playerSpeed = player.crouchSpeed;
        player.MovementInput();
    }
}
