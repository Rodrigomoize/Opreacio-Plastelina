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
            Log("Slow Time ya está activo");
            return;
        }

        if (GameSpeedManager.Instance == null)
        {
            Log("GameSpeedManager no encontrado en la escena");
            return;
        }

        int affected = GameSpeedManager.Instance.ApplyTagSpeedMultiplier(aiTeamTag, speedMultiplier);
        if (affected > 0)
        {
            isActive = true;
            Log($"✅ Slow Time activado: {affected} objetos ralentizados (tag '{aiTeamTag}') con multiplicador x{speedMultiplier}");
        }
        else
        {
            Log("⚠️ No se aplicó Slow Time (objetos afectados = 0)");
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
        Log($"✅ Slow Time desactivado - {restored} objetos restaurados");
    }

    public bool IsActive() => isActive;

    private void OnDestroy()
    {
        if (isActive)
            Deactivate();
    }

    private void Log(string message)
    {
        if (showLogs)
            Debug.Log($"[TimerPowerUp] {message}");
    }

    // Métodos de prueba para editor
    [ContextMenu("Test Activate")]
    private void TestActivate() => Activate();

    [ContextMenu("Test Deactivate")]
    private void TestDeactivate() => Deactivate();
}
