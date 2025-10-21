using UnityEngine;

/// <summary>
/// Helper para ajustar el Outline automáticamente según la plataforma.
/// Útil para optimizar el outline en WebGL.
/// </summary>
[RequireComponent(typeof(Outline))]
public class OutlineWebGLHelper : MonoBehaviour
{
    [Header("Configuración WebGL")]
    [SerializeField]
    [Tooltip("Ancho del outline en WebGL (generalmente menor que en desktop)")]
    private float webGLOutlineWidth = 2f;

    [SerializeField]
    [Tooltip("Ancho del outline en Desktop")]
    private float desktopOutlineWidth = 3f;

    [SerializeField]
    [Tooltip("Modo de outline preferido para WebGL")]
    private Outline.Mode webGLMode = Outline.Mode.OutlineVisible;

    [SerializeField]
    [Tooltip("Modo de outline preferido para Desktop")]
    private Outline.Mode desktopMode = Outline.Mode.OutlineAll;

    private Outline outlineComponent;

    void Awake()
    {
        outlineComponent = GetComponent<Outline>();
        
        if (outlineComponent == null)
        {
            Debug.LogError("[OutlineWebGLHelper] No se encontró componente Outline!");
            return;
        }

        ApplyPlatformSettings();
    }

    /// <summary>
    /// Aplica la configuración óptima según la plataforma
    /// </summary>
    private void ApplyPlatformSettings()
    {
        if (IsWebGL())
        {
            // Configuración optimizada para WebGL
            outlineComponent.OutlineWidth = webGLOutlineWidth;
            outlineComponent.OutlineMode = webGLMode;
            
            Debug.Log($"[OutlineWebGLHelper] Aplicada configuración WebGL: Width={webGLOutlineWidth}, Mode={webGLMode}");
        }
        else
        {
            // Configuración para Desktop
            outlineComponent.OutlineWidth = desktopOutlineWidth;
            outlineComponent.OutlineMode = desktopMode;
            
            Debug.Log($"[OutlineWebGLHelper] Aplicada configuración Desktop: Width={desktopOutlineWidth}, Mode={desktopMode}");
        }
    }

    /// <summary>
    /// Detecta si estamos corriendo en WebGL
    /// </summary>
    private bool IsWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Permite cambiar la configuración en runtime
    /// </summary>
    public void SetWebGLOutlineWidth(float width)
    {
        webGLOutlineWidth = width;
        if (IsWebGL() && outlineComponent != null)
        {
            outlineComponent.OutlineWidth = width;
        }
    }

    /// <summary>
    /// Permite cambiar la configuración en runtime
    /// </summary>
    public void SetDesktopOutlineWidth(float width)
    {
        desktopOutlineWidth = width;
        if (!IsWebGL() && outlineComponent != null)
        {
            outlineComponent.OutlineWidth = width;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Botón en el Inspector para probar la configuración
    /// </summary>
    [ContextMenu("Test WebGL Settings")]
    private void TestWebGLSettings()
    {
        if (outlineComponent == null)
            outlineComponent = GetComponent<Outline>();

        outlineComponent.OutlineWidth = webGLOutlineWidth;
        outlineComponent.OutlineMode = webGLMode;
        
        Debug.Log($"[OutlineWebGLHelper] Test: Aplicada configuración WebGL");
    }

    [ContextMenu("Test Desktop Settings")]
    private void TestDesktopSettings()
    {
        if (outlineComponent == null)
            outlineComponent = GetComponent<Outline>();

        outlineComponent.OutlineWidth = desktopOutlineWidth;
        outlineComponent.OutlineMode = desktopMode;
        
        Debug.Log($"[OutlineWebGLHelper] Test: Aplicada configuración Desktop");
    }
#endif
}
