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
        InitRenderers();
        ApplyEmissiveToAll();
    }

    void OnValidate()
    {
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
        // 🔴 Cuando se desactiva el script, limpia el efecto emisivo
        RemoveEmissiveFromAll();
    }

    private void InitRenderers()
    {
        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    public void ApplyEmissiveToAll()
    {
        if (renderers == null) return;

        // Solo instanciar materiales en runtime (play mode), no en editor ni en prefabs
        bool useInstancedMaterials = Application.isPlaying;

        // Instanciar materiales solo la primera vez en play mode
        if (useInstancedMaterials && !materialsInstanced)
        {
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                
                // Usar .material en vez de .sharedMaterial para crear instancia única
                // Esto se hace automáticamente al acceder a .material
                if (r.material != null)
                {
                    // Solo accediendo ya crea la instancia
                }
            }
            materialsInstanced = true;
        }

        foreach (Renderer r in renderers)
        {
            // En editor usamos sharedMaterial, en runtime usamos material instanciado
            Material mat = useInstancedMaterials ? r.material : r.sharedMaterial;
            
            if (r == null || mat == null)
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

        // En editor usamos sharedMaterial, en runtime usamos material instanciado
        bool useInstancedMaterials = Application.isPlaying;

        foreach (Renderer r in renderers)
        {
            // En editor usamos sharedMaterial, en runtime usamos material instanciado
            Material mat = useInstancedMaterials ? r.material : r.sharedMaterial;
            
            if (r == null || mat == null)
                continue;

            r.GetPropertyBlock(mpb);

            // Apagar emisión completamente
            mat.DisableKeyword("_EMISSION");
            mpb.SetColor("_EmissionColor", Color.black);

            r.SetPropertyBlock(mpb);
        }
    }
}
