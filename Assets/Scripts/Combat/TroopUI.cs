using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TroopUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas worldCanvas;
    public Image iconImage;
    public TextMeshProUGUI valueText;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset sobre la tropa
    
    [Header("Team Sprites")]
    public Sprite blueTeamIcon; // Icono para equipo azul (PlayerTeam)
    public Sprite redTeamIcon; // Icono para equipo rojo (AITeam)
    public Sprite defaultIcon; // Icono por defecto si no se especifica equipo
    
    private Transform targetTransform;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Configurar el canvas para que sea World Space
        if (worldCanvas != null)
        {
            worldCanvas.renderMode = RenderMode.WorldSpace;
        }
    }
    
    void LateUpdate()
    {
        // Seguir la posición de la tropa
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offset;
            
            // Hacer que el canvas mire a la cámara
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                 mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
    
    /// <summary>
    /// Inicializa el UI de la tropa
    /// </summary>
    /// <param name="target">Transform de la tropa a seguir</param>
    /// <param name="value">Valor numérico de la tropa (1-5)</param>
    /// <param name="teamTag">Tag del equipo ("PlayerTeam" o "AITeam")</param>
    /// <param name="icon">Sprite del icono (opcional, sobreescribe la selección por equipo)</param>
    public void Initialize(Transform target, int value, string teamTag = "", Sprite icon = null)
    {
        targetTransform = target;
        
        // Configurar el texto del valor
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
        
        // Configurar el icono según el equipo
        if (iconImage != null)
        {
            if (icon != null)
            {
                // Si se proporciona un icono específico, usarlo
                iconImage.sprite = icon;
            }
            else
            {
                // Seleccionar icono según el tag del equipo
                if (teamTag == "PlayerTeam" && blueTeamIcon != null)
                {
                    iconImage.sprite = blueTeamIcon;
                }
                else if (teamTag == "AITeam" && redTeamIcon != null)
                {
                    iconImage.sprite = redTeamIcon;
                }
                else if (defaultIcon != null)
                {
                    iconImage.sprite = defaultIcon;
                }
            }
        }
    }
    
    /// <summary>
    /// Actualiza el valor mostrado
    /// </summary>
    public void UpdateValue(int newValue)
    {
        if (valueText != null)
        {
            valueText.text = newValue.ToString();
        }
    }
}
