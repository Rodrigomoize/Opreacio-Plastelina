using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel simple de selecci�n de dificultad
/// Solo abre/cierra el panel, los botones llaman directamente a m�todos p�blicos
/// </summary>
public class DifficultyPanel : MonoBehaviour
{
    [Header("Referencias del Panel")]
    [SerializeField] private GameObject panelDificultad;
    [SerializeField] private Button botonAbrirPanel;

    [Header("Bot�n Cerrar (Opcional)")]
    [SerializeField] private Button botonCerrar;

    private void Start()
    {
        // Panel oculto al inicio
        if (panelDificultad != null)
        {
            panelDificultad.SetActive(false);
        }

        // Configurar botones de abrir/cerrar panel
        if (botonAbrirPanel != null)
        {
            botonAbrirPanel.onClick.RemoveAllListeners();
            botonAbrirPanel.onClick.AddListener(AbrirPanel);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(CerrarPanel);
        }
    }

    public void AbrirPanel()
    {
        if (panelDificultad != null)
        {
            panelDificultad.SetActive(true);
            Debug.Log("[DifficultyPanel] Panel abierto");
        }
    }

    public void CerrarPanel()
    {
        if (panelDificultad != null)
        {
            panelDificultad.SetActive(false);
            Debug.Log("[DifficultyPanel] Panel cerrado");
        }
    }

    // M�todos p�blicos para llamar desde los botones en Unity Inspector
    public void IniciarJuegoFacil()
    {
        Debug.Log("[DifficultyPanel] Iniciando juego F�CIL");
        GameManager.Instance.SetDificultad(IAController.AIDificultad.Facil);
        GameManager.GoToPlayScene();
    }

    public void IniciarJuegoMedio()
    {
        Debug.Log("[DifficultyPanel] Iniciando juego MEDIO");
        GameManager.Instance.SetDificultad(IAController.AIDificultad.Media);
        GameManager.GoToPlayScene();
    }

    public void IniciarJuegoDificil()
    {
        Debug.Log("[DifficultyPanel] Iniciando juego DIF�CIL");
        GameManager.Instance.SetDificultad(IAController.AIDificultad.Dificil);
        GameManager.GoToPlayScene();
    }

    private void OnDestroy()
    {
        if (botonAbrirPanel != null)
            botonAbrirPanel.onClick.RemoveAllListeners();

        if (botonCerrar != null)
            botonCerrar.onClick.RemoveAllListeners();
    }
}