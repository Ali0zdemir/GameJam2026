using UnityEngine;

public interface IState 
{
    public void EnterState(PlayerMovementA player);
    public void UpdateState(PlayerMovementA player);
    public void ExitState(PlayerMovementA player);
}
