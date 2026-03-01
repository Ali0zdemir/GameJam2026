using UnityEngine;

public class WalkingState : IState
{
    public void EnterState(PlayerMovementA player)
    {
        player.footstepTimer = 0f;
    }

    public void ExitState(PlayerMovementA player)
    {
        player.footstepTimer = 0f;
        player.audioSource.Stop(); // Durunca anında kes
    }

    public void UpdateState(PlayerMovementA player)
    {
        player.playerSpeed = player.walkSpeed;
        player.MovementInput();

        if (!player.isWalking || !player.isGround)
        {
            player.audioSource.Stop();
            return;
        }

        player.footstepTimer -= Time.deltaTime;
        if (player.footstepTimer <= 0f)
        {
            player.PlayFootstep();
            player.footstepTimer = player.footstepInterval;
        }
    }
}