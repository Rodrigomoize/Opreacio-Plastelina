using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Detecta ataques enemigos (CombinedCards del jugador) y eval�a su peligrosidad
/// </summary>
public class AIThreatDetector
{
    private Transform miTorre;
    
    // 🔧 Sistema para rastrear amenazas ya defendidas
    private Dictionary<GameObject, float> amenazasDefendidas = new Dictionary<GameObject, float>();
    private const float TIEMPO_MARCA_DEFENSA = 14f; // Marcar como defendida por 6 segundos

    // Distancias de referencia para calcular urgencia
    private const float distancia_cercana = 1f;  // Muy cerca
    private const float distancia_lejana = 20f;  // Lejos

    public AIThreatDetector(Transform torreIA)
    {
        miTorre = torreIA;

        if (miTorre == null)
        {
            Debug.LogError("[AIThreatDetector] Torre de la IA no asignada!");
        }
    }

    public class Amenaza
    {
        public GameObject objeto;           
        public int valor;                   
        public float distancia;           
        public float peligrosidad;          

        public override string ToString()
        {
            return $"Ataque valor {valor} a {distancia:F1}m (peligrosidad: {peligrosidad:F2})";
        }
    }

    public List<Amenaza> DetectarAmenazas()
    {
        List<Amenaza> amenazas = new List<Amenaza>();
        
        // 🔧 Limpiar amenazas defendidas que ya expiraron
        LimpiarAmenazasDefendidasExpiradas();

        GameObject[] objetosEnemigos = GameObject.FindGameObjectsWithTag("PlayerTeam");

        foreach (GameObject obj in objetosEnemigos)
        {
            // 🔧 SKIP: Si esta amenaza ya está siendo defendida, no la incluir
            if (EstaAmenazaDefendida(obj))
            {
                Debug.Log($"[AIThreatDetector] Saltando amenaza {obj.name} - ya está siendo defendida");
                continue;
            }
            
            CharacterCombined combined = obj.GetComponent<CharacterCombined>();

            if (combined != null)
            {
                Amenaza amenaza = new Amenaza();
                amenaza.objeto = obj;
                amenaza.valor = combined.GetValue();
                amenaza.distancia = Vector3.Distance(obj.transform.position, miTorre.position);
                amenaza.peligrosidad = CalcularPeligrosidad(amenaza.valor, amenaza.distancia);

                amenazas.Add(amenaza);

                Debug.Log($"[AIThreatDetector] Detecté amenaza: {amenaza}");
            }
        }

        amenazas = amenazas.OrderByDescending(a => a.peligrosidad).ToList();

        return amenazas;
    }

    private float CalcularPeligrosidad(int valor, float distancia)
    {

        float scoreValor = Mathf.Clamp01(valor / 5f);

        float scoreProximidad = 1f - Mathf.Clamp01(distancia / distancia_lejana);

        scoreProximidad = scoreProximidad * scoreProximidad;

        // COMBINACI�N: 
        // - 50% del score viene del valor del ataque
        // - 50% del score viene de la proximidad
        float peligrosidad = (scoreValor * 0.5f) + (scoreProximidad * 0.5f);

        if (distancia < distancia_cercana)
        {
            peligrosidad = Mathf.Min(1f, peligrosidad * 1.5f);
        }

        return peligrosidad;
    }

    public Amenaza ObtenerAmenazaMasPeligrosa()
    {
        List<Amenaza> amenazas = DetectarAmenazas();

        if (amenazas.Count == 0)
        {
            return null;
        }

        Debug.Log($"[AIThreatDetector] Detectadas {amenazas.Count} amenazas totales");

        // 🔧 FIX: Filtrar amenazas ya defendidas
        int amenazasDefendidas = 0;
        foreach (Amenaza amenaza in amenazas)
        {
            bool estaDefendida = EstaAmenazaDefendida(amenaza.objeto);
            Debug.Log($"[AIThreatDetector]   - Amenaza valor {amenaza.valor} a {amenaza.distancia:F1}m: {(estaDefendida ? "YA DEFENDIDA ❌" : "SIN DEFENDER ✅")}");
            
            if (!estaDefendida)
            {
                Debug.Log($"[AIThreatDetector] ⭐ Amenaza más peligrosa SIN DEFENDER: valor {amenaza.valor}, dist={amenaza.distancia:F1}m");
                return amenaza; // Primera amenaza NO defendida
            }
            else
            {
                amenazasDefendidas++;
            }
        }

        // Si todas están defendidas, devolver null
        Debug.Log($"[AIThreatDetector] ⚠️ Todas las {amenazasDefendidas} amenazas ya están defendidas");
        return null;
    }

    public int ContarAmenazas()
    {
        return DetectarAmenazas().Count;
    }

    public bool HayAmenazaCritica()
    {
        List<Amenaza> amenazas = DetectarAmenazas();

        foreach (Amenaza a in amenazas)
        {
            if (a.distancia < distancia_cercana)
            {
                return true;
            }
        }

        return false;
    }
    
    // ===== 🔧 SISTEMA DE MARCADO DE AMENAZAS DEFENDIDAS =====
    
    /// <summary>
    /// Marca una amenaza como defendida para evitar defenderla múltiples veces
    /// </summary>
    public void MarcarAmenazaComoDefendida(GameObject amenaza)
    {
        if (amenaza != null)
        {
            amenazasDefendidas[amenaza] = Time.time;
            Debug.Log($"[AIThreatDetector] ✅ Amenaza {amenaza.name} marcada como defendida");
        }
    }
    
    /// <summary>
    /// Verifica si una amenaza ya está siendo defendida
    /// </summary>
    private bool EstaAmenazaDefendida(GameObject amenaza)
    {
        if (amenaza == null) return false;
        return amenazasDefendidas.ContainsKey(amenaza);
    }
    
    /// <summary>
    /// Limpia amenazas defendidas que ya expiraron o fueron destruidas
    /// </summary>
    private void LimpiarAmenazasDefendidasExpiradas()
    {
        float tiempoActual = Time.time;
        List<GameObject> amenazasARemover = new List<GameObject>();
        
        foreach (var kvp in amenazasDefendidas)
        {
            GameObject amenaza = kvp.Key;
            float tiempoMarcado = kvp.Value;
            
            // Remover si: fue destruida O pasó el tiempo de marca
            if (amenaza == null || (tiempoActual - tiempoMarcado) > TIEMPO_MARCA_DEFENSA)
            {
                amenazasARemover.Add(amenaza);
            }
        }
        
        foreach (GameObject amenaza in amenazasARemover)
        {
            amenazasDefendidas.Remove(amenaza);
            if (amenaza != null)
            {
                Debug.Log($"[AIThreatDetector] ⏱️ Marca de defensa expirada para {amenaza.name}");
            }
        }
    }
    
    /// <summary>
    /// Limpia TODAS las marcas de defensa (usado en modo emergencia para reevaluar)
    /// </summary>
    public void LimpiarTodasLasMarcas()
    {
        int cantidad = amenazasDefendidas.Count;
        amenazasDefendidas.Clear();
        Debug.LogWarning($"[AIThreatDetector] 🔄 Limpiadas {cantidad} marcas de defensa para reevaluación");
    }
}

