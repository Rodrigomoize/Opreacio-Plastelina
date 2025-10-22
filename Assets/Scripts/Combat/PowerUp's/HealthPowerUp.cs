// HealthPowerUp_Retry.cs
using System.Collections;
using UnityEngine;

public class HealthPowerUp : MonoBehaviour
{
    [SerializeField] private int healAmount = 3;
    private TowerHealthBar towerHealthBar;
    [SerializeField] private float retryDelay = 0.5f;
    [SerializeField] private int maxRetries = 10;
    [SerializeField] private bool showLogs = true;

    public string teamTag = "";

    private void Start()
    {
        StartCoroutine(FindTowerHealthBarWithRetries());
    }

    private IEnumerator FindTowerHealthBarWithRetries()
    {
        int tries = 0;
        while (tries < maxRetries && towerHealthBar == null)
        {
            TowerHealthBar[] bars = FindObjectsOfType<TowerHealthBar>();
            if (bars != null && bars.Length > 0)
            {
                // Si hay varias, podrías filtrar por nombre o tag, aquí tomo la primera
                towerHealthBar = bars[0];
                Log($"Encontrada TowerHealthBar: {towerHealthBar.gameObject.name}");
                yield break;
            }

            tries++;
            yield return new WaitForSeconds(retryDelay);
        }

        if (towerHealthBar == null)
        {
            Debug.LogError("HealthPowerUp: no se encontró TowerHealthBar tras retries.");
        }
    }

    public void Activate()
    {
        if (towerHealthBar == null)
        {
            Debug.LogError("HealthPowerUp: no se puede activar porque towerHealthBar es null.");
            return;
        }

        towerHealthBar.Heal(healAmount);
        Log($"Healed {healAmount} on {towerHealthBar.gameObject.name}");
    }

    private void Log(string msg)
    {
        if (showLogs) Debug.Log("[HealthPowerUp] " + msg);
    }
}
