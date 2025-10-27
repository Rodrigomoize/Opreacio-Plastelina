using UnityEngine;

/// <summary>
/// Efecto visual de charco que aparece cuando una tropa se despawnea en el río.
/// Se expande gradualmente y luego desaparece.
/// </summary>
public class PuddleVFX : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duración de la expansión del charco")]
    public float expansionDuration = 0.5f;
    
    [Tooltip("Duración de la permanencia del charco")]
    public float holdDuration = 0.3f;
    
    [Tooltip("Duración del fade out del charco")]
    public float fadeOutDuration = 0.5f;
    
    [Tooltip("Escala inicial del charco")]
    public float initialScale = 0.1f;
    
    [Tooltip("Escala final del charco")]
    public float finalScale = 1f;

    private float elapsedTime = 0f;
    private Vector3 targetScale;
    private Material puddleMaterial;
    private Color initialColor;
    private Renderer puddleRenderer;

    private void Start()
    {
        targetScale = Vector3.one * finalScale;
        transform.localScale = Vector3.one * initialScale;

        // Obtener el renderer y material para hacer fade
        puddleRenderer = GetComponent<Renderer>();
        if (puddleRenderer != null)
        {
            puddleMaterial = puddleRenderer.material;
            initialColor = puddleMaterial.color;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Fase 1: Expansión
        if (elapsedTime < expansionDuration)
        {
            float progress = elapsedTime / expansionDuration;
            transform.localScale = Vector3.Lerp(Vector3.one * initialScale, targetScale, progress);
        }
        // Fase 2: Hold (mantener tamaño)
        else if (elapsedTime < expansionDuration + holdDuration)
        {
            transform.localScale = targetScale;
        }
        // Fase 3: Fade out
        else if (elapsedTime < expansionDuration + holdDuration + fadeOutDuration)
        {
            float fadeProgress = (elapsedTime - expansionDuration - holdDuration) / fadeOutDuration;
            
            if (puddleMaterial != null)
            {
                Color currentColor = initialColor;
                currentColor.a = Mathf.Lerp(initialColor.a, 0f, fadeProgress);
                puddleMaterial.color = currentColor;
            }
            
            // También reducir ligeramente la escala durante el fade
            transform.localScale = Vector3.Lerp(targetScale, targetScale * 0.8f, fadeProgress);
        }
        // Fase 4: Destruir
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Limpiar el material instanciado
        if (puddleMaterial != null)
        {
            Destroy(puddleMaterial);
        }
    }
}
