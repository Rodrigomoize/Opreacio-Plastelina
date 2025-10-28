using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Animación simple de bote vertical para simular un vehículo en marcha
/// Preserva las escalas locales de los hijos especificados
/// </summary>
public class VehicleBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("Altura del bote en unidades")]
    [Range(0.01f, 0.5f)]
    public float bounceHeight = 0.05f;
    
    [Tooltip("Velocidad del bote (ciclos por segundo)")]
    [Range(0.5f, 10f)]
    public float bounceSpeed = 3f;
    
    [Header("Noise Settings")]
    [Tooltip("Añadir ruido aleatorio al movimiento")]
    public bool addNoise = true;
    
    [Tooltip("Intensidad del ruido")]
    [Range(0f, 0.1f)]
    public float noiseIntensity = 0.02f;
    
    [Tooltip("Velocidad del ruido")]
    [Range(0.1f, 5f)]
    public float noiseSpeed = 1f;
    
    [Header("Rotation Settings")]
    [Tooltip("Añadir rotación sutil al bote")]
    public bool addRotation = true;
    
    [Tooltip("Ángulo máximo de rotación en X")]
    [Range(0f, 5f)]
    public float maxRotationX = 1f;
    
    [Tooltip("Ángulo máximo de rotación en Z")]
    [Range(0f, 5f)]
    public float maxRotationZ = 1f;
    
    [Header("Scale Preservation")]
    [Tooltip("Nombres de hijos cuya escala debe preservarse (ej: FrontSlot, BackSlot)")]
    public List<string> preserveScaleChildren = new List<string> { "FrontSlot", "BackSlot", "FrontAnchor", "BackAnchor" };
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float randomOffset;
    
    // Diccionario para guardar las escalas originales de los hijos
    private Dictionary<Transform, Vector3> originalChildScales = new Dictionary<Transform, Vector3>();
    
    void Start()
    {
        // Guardar posición y rotación inicial
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        
        // Offset aleatorio para que no todas las unidades reboten en sincronía
        randomOffset = Random.Range(0f, 100f);
        
        // Guardar las escalas originales de los hijos especificados
        SaveChildScales();
    }
    
    void Update()
    {
        // Calcular el bote vertical básico (onda sinusoidal)
        float time = Time.time * bounceSpeed + randomOffset;
        float bounce = Mathf.Sin(time) * bounceHeight;
        
        // Añadir ruido Perlin si está activado
        float noise = 0f;
        if (addNoise)
        {
            float noiseTime = Time.time * noiseSpeed + randomOffset;
            noise = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * noiseIntensity;
        }
        
        // Aplicar el movimiento vertical
        Vector3 newPosition = startPosition;
        newPosition.y += bounce + noise;
        transform.localPosition = newPosition;
        
        // Aplicar rotación sutil si está activado
        if (addRotation)
        {
            float rotX = Mathf.Sin(time * 0.8f) * maxRotationX;
            float rotZ = Mathf.Cos(time * 1.2f) * maxRotationZ;
            
            Quaternion bounceRotation = Quaternion.Euler(rotX, 0f, rotZ);
            transform.localRotation = startRotation * bounceRotation;
        }
    }
    
    void LateUpdate()
    {
        // Restaurar las escalas de los hijos especificados
        RestoreChildScales();
    }
    
    /// <summary>
    /// Guarda las escalas locales originales de los hijos especificados
    /// </summary>
    private void SaveChildScales()
    {
        originalChildScales.Clear();
        
        foreach (string childName in preserveScaleChildren)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                originalChildScales[child] = child.localScale;
            }
        }
    }
    
    /// <summary>
    /// Restaura las escalas originales de los hijos (en caso de que se vean afectados)
    /// </summary>
    private void RestoreChildScales()
    {
        foreach (var kvp in originalChildScales)
        {
            if (kvp.Key != null)
            {
                kvp.Key.localScale = kvp.Value;
            }
        }
    }
    
    /// <summary>
    /// Reinicia la posición inicial (útil si el objeto se mueve)
    /// </summary>
    public void ResetStartPosition()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }
    
    /// <summary>
    /// Actualiza la lista de escalas guardadas (llamar si se añaden tropas dinámicamente)
    /// </summary>
    public void RefreshChildScales()
    {
        SaveChildScales();
    }
}
