using UnityEngine;

[ExecuteAlways]
public class EnemyRedEmissiveTint : MonoBehaviour
{
    [Header("Color del tinte emisivo")]
    [Tooltip("El valor alpha no se usa. La intensidad del brillo se controla con Emission Intensity.")]
    public Color emissiveColor = new Color(1f, 0.1f, 0.1f, 1f);

    [Range(0f, 5f)]
    [Tooltip("Controla cuán brillante es el tinte emisivo.")]
    public float emissionIntensity = 1.0f;

    [Header("Activar/desactivar emisión")]
    public bool applyEmissive = false;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private bool materialsInstanced = false; // Flag para saber si ya instanciamos los materiales

    void OnEnable()
    {
        // NO ejecutar en prefabs
        if (!Application.isPlaying && (gameObject.scene.name == null || gameObject.scene.name == ""))
        {
            return;
        }

        InitRenderers();
        ApplyEmissiveToAll();
    }

    void OnValidate()
    {
        // NO ejecutar OnValidate en prefabs (objetos sin escena asignada)
        if (gameObject.scene.name == null || gameObject.scene.name == "")
        {
            return; // Es un prefab, no hacer nada
        }

        InitRenderers();

        // Evita aplicar el efecto si el componente está desactivado
        if (!this.enabled)
        {
            RemoveEmissiveFromAll();
            return;
        }

        ApplyEmissiveToAll();
    }


    void OnDisable()
    {
        // NO ejecutar en prefabs
        if (!Application.isPlaying && (gameObject.scene.name == null || gameObject.scene.name == ""))
        {
            return;
        }

        // Cuando se desactiva el script, limpia el efecto emisivo
        RemoveEmissiveFromAll();
    }

    private void InitRenderers()
    {
        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    /// <summary>
    /// Fuerza la reinicialización de los renderers. Útil cuando se llama desde código externo.
    /// </summary>
    public void ForceRefreshRenderers()
    {
        renderers = null; // Forzar la reinicialización
        InitRenderers();
    }

    public void ApplyEmissiveToAll()
    {
        if (renderers == null) return;

        // Instanciar materiales solo la primera vez en play mode
        if (Application.isPlaying && !materialsInstanced)
        {
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                
                // Acceder a .material para crear instancia única (solo en play mode)
                Material temp = r.material;
                if (temp != null)
                {
                    // Solo accediendo ya crea la instancia
                }
            }
            materialsInstanced = true;
        }

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            // IMPORTANTE: En modo editor, siempre usar sharedMaterial para evitar errores con prefabs
            // Solo en play mode usamos material instanciado
            Material mat;
            
            if (Application.isPlaying)
            {
                // En runtime, usar material instanciado
                mat = r.material;
            }
            else
            {
                // En editor, usar sharedMaterial (funciona con prefabs y objetos de escena)
                mat = r.sharedMaterial;
            }
            
            if (mat == null)
                continue;

            r.GetPropertyBlock(mpb);

            if (applyEmissive)
            {
                // Activa la keyword de emisión
                mat.EnableKeyword("_EMISSION");

                // Calcular color final (gamma corregido)
                Color finalEmission = emissiveColor * Mathf.LinearToGammaSpace(emissionIntensity);
                mpb.SetColor("_EmissionColor", finalEmission);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mpb.SetColor("_EmissionColor", Color.black);
            }

            r.SetPropertyBlock(mpb);
        }
    }

    private void RemoveEmissiveFromAll()
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            // IMPORTANTE: En modo editor, siempre usar sharedMaterial para evitar errores con prefabs
            // Solo en play mode usamos material instanciado
            Material mat;
            
            if (Application.isPlaying)
            {
                // En runtime, usar material instanciado
                mat = r.material;
            }
            else
            {
                // En editor, usar sharedMaterial (funciona con prefabs y objetos de escena)
                mat = r.sharedMaterial;
            }
            
            if (mat == null)
                continue;

            r.GetPropertyBlock(mpb);

            // Apagar emisión completamente
            mat.DisableKeyword("_EMISSION");
            mpb.SetColor("_EmissionColor", Color.black);

            r.SetPropertyBlock(mpb);
        }
    }
}
