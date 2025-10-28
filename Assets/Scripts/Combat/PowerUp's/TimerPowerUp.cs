using UnityEngine;

public class TimerPowerUp : MonoBehaviour
{
    [Header("Slow Time Settings")]
    [SerializeField] private float speedMultiplier = 0.3f; // 0.3 = 30% velocidad
    [SerializeField] private string aiTeamTag = "AITeam";

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    private bool isActive = false;

    public void Activate()
    {
        if (isActive)
        {
            return;
        }

        if (GameSpeedManager.Instance == null)
        {
            return;
        }

        int affected = GameSpeedManager.Instance.ApplyTagSpeedMultiplier(aiTeamTag, speedMultiplier);
        if (affected > 0)
        {
            isActive = true;
        }
    }

    public void Deactivate()
    {
        if (!isActive)
            return;

        if (GameSpeedManager.Instance == null)
        {
            isActive = false;
            return;
        }

        int restored = GameSpeedManager.Instance.RemoveTagSpeedMultiplier(aiTeamTag);
        isActive = false;
    }

    public bool IsActive() => isActive;

    private void OnDestroy()
    {
        if (isActive)
            Deactivate();
    }

    // Métodos de prueba para editor
    [ContextMenu("Test Activate")]
    private void TestActivate() => Activate();

    [ContextMenu("Test Deactivate")]
    private void TestDeactivate() => Deactivate();
}
