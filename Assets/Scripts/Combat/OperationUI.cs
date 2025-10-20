using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OperationUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas worldCanvas;
    public Image iconImage;
    public TextMeshProUGUI operationText;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 3f, 0); // Offset sobre la operación
    
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
        // Seguir la posición de la operación
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
    /// Inicializa el UI de la operación
    /// </summary>
    /// <param name="target">Transform de la operación a seguir</param>
    /// <param name="valueA">Primer valor de la operación</param>
    /// <param name="valueB">Segundo valor de la operación</param>
    /// <param name="operatorSymbol">Símbolo de la operación ('+' o '-')</param>
    /// <param name="teamTag">Tag del equipo ("PlayerTeam" o "AITeam")</param>
    /// <param name="icon">Sprite del icono (opcional, sobreescribe la selección por equipo)</param>
    public void Initialize(Transform target, int valueA, int valueB, char operatorSymbol, string teamTag = "", Sprite icon = null)
    {
        targetTransform = target;
        
        // Configurar el texto de la operación
        if (operationText != null)
        {
            string operation = $"{valueA}{operatorSymbol}{valueB}";
            operationText.text = operation;
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
    /// Actualiza la operación mostrada
    /// </summary>
    public void UpdateOperation(int valueA, int valueB, char operatorSymbol)
    {
        if (operationText != null)
        {
            string operation = $"{valueA}{operatorSymbol}{valueB}";
            operationText.text = operation;
        }
    }
}
