using UnityEngine;
using UnityEngine.UI;

public class DifficultyPanel : MonoBehaviour
{
    [Header("Referencias del Panel")]
    [SerializeField] private GameObject panelDificultad;
    [SerializeField] private Button botonAbrirPanel;
    [SerializeField] private Button botonCerrar;

    private void Start()
    {
        if (panelDificultad != null)
        {
            panelDificultad.SetActive(false);
        }

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
        }
    }

    public void CerrarPanel()
    {
        if (panelDificultad != null)
        {
            panelDificultad.SetActive(false);
        }
    }

    public void IniciarJuegoFacil()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(IAController.AIDificultad.Facil);
        }
        else
        {
            GameManager.Instance.SetDificultad(IAController.AIDificultad.Facil);
        }
        GameManager.GoToPlayScene();
    }

    public void IniciarJuegoMedio()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(IAController.AIDificultad.Media);
        }
        else
        {
            GameManager.Instance.SetDificultad(IAController.AIDificultad.Media);
        }
        GameManager.GoToPlayScene();
    }

    public void IniciarJuegoDificil()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(IAController.AIDificultad.Dificil);
        }
        else
        {
            GameManager.Instance.SetDificultad(IAController.AIDificultad.Dificil);
        }
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
