using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de shake de cámara para feedback visual
/// Usar CameraShake.Instance.Shake() para activar desde cualquier script
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    [Header("Shake Settings")]
    [Tooltip("Duración del shake en segundos")]
    public float shakeDuration = 0.3f;
    
    [Tooltip("Magnitud del shake (intensidad)")]
    public float shakeMagnitude = 0.2f;
    
    [Tooltip("Qué tan rápido disminuye el shake")]
    public float dampingSpeed = 1.0f;
    
    private Vector3 originalPosition;
    private bool isShaking = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void OnEnable()
    {
        originalPosition = transform.localPosition;
    }
    
    /// <summary>
    /// Inicia el efecto de shake de cámara
    /// </summary>
    public void Shake()
    {
        Shake(shakeDuration, shakeMagnitude);
    }
    
    /// <summary>
    /// Inicia el efecto de shake de cámara con parámetros personalizados
    /// </summary>
    /// <param name="duration">Duración del shake</param>
    /// <param name="magnitude">Intensidad del shake</param>
    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(DoShake(duration, magnitude));
        }
    }
    
    private IEnumerator DoShake(float duration, float magnitude)
    {
        isShaking = true;
        originalPosition = transform.localPosition;
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            
            // Reducir gradualmente la magnitud
            magnitude = Mathf.Lerp(magnitude, 0, elapsed / duration * dampingSpeed);
            
            yield return null;
        }
        
        // Restaurar posición original
        transform.localPosition = originalPosition;
        isShaking = false;
    }
}
