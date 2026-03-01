using UnityEngine;

public class WaveMember : MonoBehaviour
{
    private BossFightManager manager;
    private bool reported;

    public void Init(BossFightManager m)
    {
        manager = m;
        reported = false;
        manager.RegisterWaveEnemy(this);
    }

    void OnDestroy()
    {
        if (reported) return;
        reported = true;
        if (manager != null) manager.UnregisterWaveEnemy(this);
    }
}