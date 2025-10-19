using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static CardManager;

/// <summary>
/// Controlador principal de la IA
/// Usa Utility AI + Score-Based System para tomar decisiones inteligentes
/// </summary>
public class IAController : MonoBehaviour
{
    [Header("Referencias Obligatorias")]
    [Tooltip("CardManager compartido del juego")]
    public CardManager cardManager;

    [Tooltip("IntelectManager de la IA (usar AIIntelectManager)")]
    public IntelectManager intelectManagerIA;

    [Tooltip("Transform donde la IA spawneará sus cartas")]
    public Transform spawnPointIA;

    [Tooltip("Transform de la torre de la IA")]
    public Transform torreIA;


    [Header("Configuración de Comportamiento")]
    [Tooltip("Cada cuántos segundos la IA toma una decisión")]
    [Range(0.5f, 5f)]
    public float intervaloDecision = 0.8f; // ⚡ REDUCIDO para respuesta más rápida

    [Tooltip("Agresividad de la IA (0=defensiva, 1=muy agresiva)")]
    [Range(0f, 1f)]
    public float agresividad = 0.5f;

    [Tooltip("Mostrar logs detallados de decisiones")]
    public bool debugMode = true;


    // Componentes internos
    private AICardHand manoIA;
    private AIThreatDetector detectorAmenazas;
    private List<AIAction> accionesPosibles;
    private float tiempoAcumulado = 0f;


    void Start()
    {
        // Validaciones
        if (!ValidarReferencias()) return;

        // Inicializar componentes
        InicializarIA();

        Debug.Log($"[IAController] ✅ IA inicializada con agresividad {agresividad:F2}");
    }

    private bool ValidarReferencias()
    {
        bool valido = true;

        if (cardManager == null)
        {
            Debug.LogError("[IAController] CardManager no asignado!");
            valido = false;
        }

        if (intelectManagerIA == null)
        {
            Debug.LogError("[IAController] AIIntelectManager de IA no asignado!");
            valido = false;
        }

        if (spawnPointIA == null)
        {
            Debug.LogError("[IAController] SpawnPoint de IA no asignado!");
            valido = false;
        }

        if (torreIA == null)
        {
            Debug.LogError("[IAController] Torre de IA no asignada!");
            valido = false;
        }

        return valido;
    }

    private void InicializarIA()
    {
        // Crear mano de cartas (4 cartas iniciales)
        manoIA = new AICardHand(cardManager);

        // Crear detector de amenazas
        detectorAmenazas = new AIThreatDetector(torreIA);

        // Crear todas las acciones posibles
        accionesPosibles = new List<AIAction>
        {
            new AccionDefender(
                intelectManagerIA,
                cardManager,
                manoIA,
                detectorAmenazas,
                spawnPointIA,
                torreIA
            ),

            new AccionAtacarConSuma(
                intelectManagerIA,
                cardManager,
                manoIA,
                detectorAmenazas,
                spawnPointIA,
                agresividad
            ),

            new AccionAtacarConResta(
                intelectManagerIA,
                cardManager,
                manoIA,
                detectorAmenazas,
                spawnPointIA,
                agresividad
            ),

            new AccionEsperar(
                intelectManagerIA,
                manoIA,
                detectorAmenazas
            )
        };
    }

    void Update()
    {
        // ⚡ SISTEMA DE REACCIÓN RÁPIDA
        // Si hay amenaza crítica, tomar decisión inmediata
        if (detectorAmenazas != null && detectorAmenazas.HayAmenazaCritica())
        {
            Debug.Log("[IAController] ⚠️ AMENAZA CRÍTICA - Reacción inmediata");
            TomarDecision();
            tiempoAcumulado = 0f; // Resetear contador
            return;
        }

        // Contador de tiempo para toma de decisiones normal
        tiempoAcumulado += Time.deltaTime;

        if (tiempoAcumulado >= intervaloDecision)
        {
            tiempoAcumulado = 0f;
            TomarDecision();
        }
    }

    /// <summary>
    /// Sistema de toma de decisiones - CORAZÓN DE LA IA
    /// Evalúa todas las acciones y ejecuta la mejor
    /// </summary>
    private void TomarDecision()
    {
        if (debugMode)
        {
            Debug.Log($"\n========== [IA] NUEVA DECISIÓN (t={Time.time:F1}s) ==========");
            Debug.Log($"[IA] Intelecto: {intelectManagerIA.currentIntelect}/{intelectManagerIA.maxIntelect}");
            Debug.Log($"[IA] Cartas en mano: {manoIA.CantidadCartas()}");
            Debug.Log($"[IA] Amenazas detectadas: {detectorAmenazas.ContarAmenazas()}");
        }

        // ===== PASO 1: EVALUAR TODAS LAS ACCIONES =====
        float mejorScore = float.MinValue;
        AIAction mejorAccion = null;

        foreach (AIAction accion in accionesPosibles)
        {
            // Calcular score de esta acción
            float score = accion.CalcularScore();

            if (debugMode)
            {
                Debug.Log($"[IA]   • {accion.nombreAccion}: {score:F3}");
            }

            // ¿Es mejor que la mejor actual?
            if (score > mejorScore)
            {
                mejorScore = score;
                mejorAccion = accion;
            }
        }

        // ===== PASO 2: EJECUTAR LA MEJOR ACCIÓN =====
        if (mejorAccion != null && mejorScore > 0.01f) // Threshold mínimo
        {
            if (debugMode)
            {
                Debug.Log($"[IA] ⭐ DECISIÓN: {mejorAccion.nombreAccion} (score: {mejorScore:F3})");
            }

            mejorAccion.Ejecutar();
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"[IA] ❌ No hay acciones viables (mejor score: {mejorScore:F3})");
            }
        }

        if (debugMode)
        {
            Debug.Log("==========================================================\n");
        }
    }


    // ===================================================================
    // MÉTODOS PÚBLICOS PARA DEBUGGING Y AJUSTES EN RUNTIME
    // ===================================================================

    /// <summary>
    /// Cambia la agresividad de la IA en runtime
    /// Útil para dificultades dinámicas
    /// </summary>
    public void SetAgresividad(float nuevaAgresividad)
    {
        agresividad = Mathf.Clamp01(nuevaAgresividad);
        Debug.Log($"[IAController] Agresividad ajustada a {agresividad:F2}");
    }

    /// <summary>
    /// Fuerza a la IA a tomar una decisión inmediatamente
    /// Útil para testing
    /// </summary>
    public void ForzarDecision()
    {
        Debug.Log("[IAController] Forzando decisión inmediata...");
        TomarDecision();
    }

    /// <summary>
    /// Devuelve información de estado para UI o debugging
    /// </summary>
    public string ObtenerEstadoIA()
    {
        string estado = $"IA Status:\n";
        estado += $"Intelecto: {intelectManagerIA.currentIntelect}/{intelectManagerIA.maxIntelect}\n";
        estado += $"Cartas: {manoIA.CantidadCartas()}\n";
        estado += $"Amenazas: {detectorAmenazas.ContarAmenazas()}\n";
        estado += $"Agresividad: {agresividad:F2}";
        return estado;
    }


    // ===================================================================
    // GIZMOS PARA VISUALIZACIÓN EN EDITOR
    // ===================================================================

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (torreIA == null) return;

        // Dibujar radio de detección de amenazas
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(torreIA.position, 20f);

        // Dibujar amenazas detectadas
        if (detectorAmenazas != null)
        {
            var amenazas = detectorAmenazas.DetectarAmenazas();

            foreach (var amenaza in amenazas)
            {
                if (amenaza.objeto == null) continue;

                // Color según peligrosidad
                if (amenaza.peligrosidad > 0.7f)
                    Gizmos.color = Color.red;
                else if (amenaza.peligrosidad > 0.4f)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.green;

                // Línea desde amenaza a torre
                Gizmos.DrawLine(amenaza.objeto.transform.position, torreIA.position);

                // Esfera en la amenaza
                Gizmos.DrawWireSphere(amenaza.objeto.transform.position, 0.5f);
            }
        }
    }
}
