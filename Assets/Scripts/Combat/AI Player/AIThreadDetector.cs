using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Detecta ataques enemigos (CombinedCards del jugador) y evalúa su peligrosidad
/// </summary>
public class AIThreatDetector
{
    private Transform miTorre;

    // Distancias de referencia para calcular urgencia
    private const float distancia_cercana = 5f;  // Muy cerca
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

        GameObject[] objetosEnemigos = GameObject.FindGameObjectsWithTag("PlayerTeam");

        foreach (GameObject obj in objetosEnemigos)
        {
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

        // COMBINACIÓN: 
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

        return amenazas[0];
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
}