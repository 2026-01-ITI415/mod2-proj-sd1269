using UnityEngine;

public class EnemyWaveTracker : MonoBehaviour
{
    private WaveManager waveManager;
    private bool reported = false;

    public void SetWaveManager(WaveManager manager)
    {
        waveManager = manager;
    }

    private void OnDestroy()
    {
        if (!reported && waveManager != null)
        {
            reported = true;
            waveManager.NotifyEnemyDestroyed();
        }
    }
}