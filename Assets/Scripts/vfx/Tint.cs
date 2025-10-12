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

    private void ApplyEmissiveToAll()
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            if (r == null || r.sharedMaterial == null)
                continue;

            r.GetPropertyBlock(mpb);

            if (applyEmissive)
            {
                // Activa la keyword de emisión (para Built-in y URP)
                r.sharedMaterial.EnableKeyword("_EMISSION");

                // Calcular color final (gamma corregido)
                Color finalEmission = emissiveColor * Mathf.LinearToGammaSpace(emissionIntensity);
                mpb.SetColor("_EmissionColor", finalEmission);
            }
            else
            {
                r.sharedMaterial.DisableKeyword("_EMISSION");
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
            if (r == null || r.sharedMaterial == null)
                continue;

            r.GetPropertyBlock(mpb);

            // Apagar emisión completamente
            r.sharedMaterial.DisableKeyword("_EMISSION");
            mpb.SetColor("_EmissionColor", Color.black);

            r.SetPropertyBlock(mpb);
        }
    }
}
