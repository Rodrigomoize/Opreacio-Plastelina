using UnityEngine;
using System.Collections;

/// <summary>
/// VFX de spawn: muñeco de plastelina saliendo del suelo
/// Muestra partículas de tierra/polvo y un splat de plastelina que se contrae
/// </summary>
public class TroopSpawnVFX : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem dirtParticles;
    [SerializeField] private ParticleSystem clayParticles;
      [Header("Splat Effect (Esfera Aplastada)")]
    [SerializeField] private GameObject splatSphere; // Esfera aplastada de plastelina
    [SerializeField] private float splatInitialScale = 2f; // Escala inicial (grande)
    [SerializeField] private float splatFinalScale = 0.1f; // Escala final (pequeña)
    [SerializeField] private float splatDuration = 2f; // Duración de la contracción
    
    [Header("Settings")]
    [SerializeField] private float lifetime = 2.5f;
      void Start()
    {
        // Reproducir todas las partículas
        if (dirtParticles != null)
            dirtParticles.Play();
            
        if (clayParticles != null)
            clayParticles.Play();
        
        // Animar splat de plastelina
        if (splatSphere != null)
        {
            StartCoroutine(AnimateSplatSphere());
        }
        
        // Auto-destruir después del lifetime
        Destroy(gameObject, lifetime);
    }    /// <summary>
    /// Anima el splat de plastelina: comienza grande y se hace pequeño
    /// mientras la tropa crece desde el suelo
    /// </summary>
    private IEnumerator AnimateSplatSphere()
    {
        // Configurar escala inicial (grande y aplastada) y final (pequeña)
        Vector3 initialScale = new Vector3(splatInitialScale, splatInitialScale * 0.2f, splatInitialScale);
        Vector3 finalScale = new Vector3(splatFinalScale, splatFinalScale * 0.2f, splatFinalScale);
        
        // Elevar el splat para que se vea sobre el suelo
        Vector3 originalPos = splatSphere.transform.localPosition;
        splatSphere.transform.localPosition = new Vector3(originalPos.x, 0.1f, originalPos.z);
        
        // Empezar con escala GRANDE
        splatSphere.transform.localScale = initialScale;
          float t = 0;
        while (t < splatDuration)
        {
            t += Time.deltaTime;
            float progress = t / splatDuration;
            
            // Escala: de GRANDE a PEQUEÑO (interpolación lineal directa)
            splatSphere.transform.localScale = Vector3.Lerp(initialScale, finalScale, progress);
            
            yield return null;
        }
        
        // Al final, ocultar el splat
        splatSphere.SetActive(false);
    }
}
