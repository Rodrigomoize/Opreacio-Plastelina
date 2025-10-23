using UnityEngine;
using System.Collections;

/// <summary>
/// VFX de spawn: muñeco de plastelina saliendo del suelo
/// Muestra partículas de tierra/polvo y anima el efecto de surgir
/// </summary>
public class TroopSpawnVFX : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem dirtParticles;
    [SerializeField] private ParticleSystem clayParticles;
    
    [Header("Ground Effect")]
    [SerializeField] private GameObject groundRipple; // Efecto de onda en el suelo
    [SerializeField] private float rippleMaxScale = 2f;
    [SerializeField] private float rippleDuration = 1f;
    
    [Header("Settings")]
    [SerializeField] private float lifetime = 2.5f;
    
    void Start()
    {
        // Reproducir todas las partículas
        if (dirtParticles != null)
            dirtParticles.Play();
            
        if (clayParticles != null)
            clayParticles.Play();
        
        // Animar efecto de onda en el suelo
        if (groundRipple != null)
        {
            StartCoroutine(AnimateGroundRipple());
        }
        
        // Auto-destruir después del lifetime
        Destroy(gameObject, lifetime);
    }
    
    /// <summary>
    /// Anima la onda en el suelo que se expande desde el centro
    /// </summary>
    private IEnumerator AnimateGroundRipple()
    {
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * rippleMaxScale;
        
        float t = 0;
        while (t < rippleDuration)
        {
            t += Time.deltaTime;
            float progress = t / rippleDuration;
            
            // Escala con ease-out
            float easeProgress = 1f - Mathf.Pow(1f - progress, 3f);
            groundRipple.transform.localScale = Vector3.Lerp(initialScale, targetScale, easeProgress);
            
            // Fade out
            if (groundRipple.TryGetComponent<Renderer>(out Renderer renderer))
            {
                Material mat = renderer.material;
                Color col = mat.color;
                col.a = 1f - progress;
                mat.color = col;
            }
            
            yield return null;
        }
    }
}
