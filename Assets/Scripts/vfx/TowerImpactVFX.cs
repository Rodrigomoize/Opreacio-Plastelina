using UnityEngine;
using System.Collections;

/// <summary>
/// VFX para cuando un Combined impacta en una torre
/// Gran explosión con partículas disparadas en todas direcciones
/// </summary>
public class TowerImpactVFX : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem[] particleSystems;
    
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionMesh;
    [SerializeField] private float explosionScaleMultiplier = 2f; // Más grande que el impacto normal
    [SerializeField] private float explosionDuration = 0.3f;
    
    [Header("Camera Shake (Opcional)")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float shakeMagnitude = 0.3f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    [Header("Flash Effect (Opcional)")]
    [SerializeField] private bool enableFlash = true;
    [SerializeField] private Color flashColor = new Color(1f, 0.5f, 0f, 0.3f); // Naranja
    [SerializeField] private float flashDuration = 0.15f;
    
    void Start()
    {
        // Reproducir todas las partículas
        if (particleSystems != null)
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
        }

        // Animación de explosión del mesh
        if (explosionMesh != null)
        {
            StartCoroutine(AnimateExplosion());
        }

        // Efectos opcionales
        if (enableCameraShake)
        {
            TriggerCameraShake();
        }

        if (enableFlash)
        {
            TriggerScreenFlash();
        }

        // Destruir después de que termine todo
        Destroy(gameObject, 3f);
    }

    private IEnumerator AnimateExplosion()
    {
        Vector3 targetScale = Vector3.one * explosionScaleMultiplier;
        Vector3 startScale = Vector3.zero;
        
        // Fase 1: Explosión rápida (scale up)
        float t = 0;
        while (t < explosionDuration)
        {
            t += Time.deltaTime;
            float progress = t / explosionDuration;
            
            // Usar EaseOut para que la explosión sea más impactante al inicio
            float easeProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            explosionMesh.transform.localScale = Vector3.Lerp(startScale, targetScale, easeProgress);
            yield return null;
        }

        // Fase 2: Mantener un momento
        yield return new WaitForSeconds(0.2f);

        // Fase 3: Fade out con reducción de escala
        float fadeTime = 0.5f;
        t = 0;
        
        Renderer renderer = explosionMesh.GetComponentInChildren<Renderer>();
        Material mat = null;
        
        if (renderer != null)
        {
            mat = renderer.material;
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float progress = t / fadeTime;
            
            // Reducir escala ligeramente
            explosionMesh.transform.localScale = Vector3.Lerp(targetScale, targetScale * 0.8f, progress);
            
            // Fade out del material
            if (mat != null)
            {
                Color col = mat.color;
                col.a = 1f - progress;
                mat.color = col;
            }
            
            yield return null;
        }
    }

    private void TriggerCameraShake()
    {
        // Buscar el singleton de CameraShake
        CameraShake cameraShake = CameraShake.Instance;
        if (cameraShake != null)
        {
            cameraShake.Shake(shakeDuration, shakeMagnitude);
        }
        else
        {
            Debug.LogWarning("[TowerImpactVFX] CameraShake.Instance no encontrado");
        }
    }

    private void TriggerScreenFlash()
    {
        // Buscar el singleton de ScreenFlashEffect
        ScreenFlashEffect screenFlash = ScreenFlashEffect.Instance;
        if (screenFlash != null)
        {
            screenFlash.Flash(flashColor, flashDuration);
        }
        else
        {
            Debug.LogWarning("[TowerImpactVFX] ScreenFlashEffect.Instance no encontrado");
        }
    }
}
