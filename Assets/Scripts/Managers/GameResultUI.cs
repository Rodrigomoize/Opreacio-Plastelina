using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la UI de la pantalla de resultado (win/lose) con dos botones:
/// - Volver a jugar (usa la configuración del Inspector o RestartCurrentLevel por defecto)
/// - Salir al menú (MainMenu)
/// </summary>
public class GameResultUI : MonoBehaviour
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        // ✅ MODIFICADO: Solo añadir listener si NO tiene ninguno asignado en Inspector
        if (replayButton != null)
        {
            int existingListeners = replayButton.onClick.GetPersistentEventCount();
            
            if (existingListeners == 0)
            {
                // No hay listeners configurados en Inspector, usar comportamiento por defecto
                Debug.Log("[GameResultUI] No hay listeners en Inspector, usando RestartCurrentLevel por defecto");
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(OnReplayClicked);
            }
            else
            {
                // Ya tiene listeners configurados en Inspector, respetarlos
                Debug.Log($"[GameResultUI] Botón tiene {existingListeners} listener(s) configurado(s) en Inspector, respetando configuración");
            }
        }
        
        // MainMenu button siempre se configura por código
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void OnDestroy()
    {
        // Solo remover si fue añadido por código
        if (replayButton != null && replayButton.onClick.GetPersistentEventCount() == 0)
        {
            replayButton.onClick.RemoveListener(OnReplayClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }

    private void OnReplayClicked()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("[GameResultUI] 🔄 Reiniciando nivel actual (comportamiento por defecto)");
            GameManager.Instance.RestartCurrentLevel();
        }
    }

    private void OnMainMenuClicked()
    {
        GameManager.GoToMainMenu();
    }
}
