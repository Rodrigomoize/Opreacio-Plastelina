using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static CardManager;

/// Controlador principal de la IA
/// Usa Utility AI + Score-Based System para tomar decisiones inteligentes
public class IAController : MonoBehaviour
{
    [Header("Referencias Obligatorias")]
    [Tooltip("CardManager compartido del juego")]
    public CardManager cardManager;

    [Tooltip("IntelectManager de la IA (usar AIIntelectManager)")]
    public IntelectManager intelectManagerIA;

    [Tooltip("IntelectManager del jugador")]
    public IntelectManager intelectManagerPlayer;

    [Tooltip("Transform donde la IA spawneará sus cartas")]
    public Transform spawnPointIA;
    public Transform spawnPointIA2;


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

    public enum AIDificultad
    {
        Facil,
        Media,
        Dificil
    }

    [System.Serializable]
    public class DifficultySettings
    {
        [Header("Comportamiento IA")]
        [Tooltip("Agresividad de la IA (0=defensiva, 1=muy agresiva)")]
        [Range(0f, 1f)]
        public float agresividad = 0.5f;

        [Tooltip("Intervalo entre decisiones de la IA (en segundos)")]
        [Range(0.3f, 3f)]
        public float intervaloDecision = 1.0f;

        [Header("Recursos")]
        [Tooltip("Tiempo de regeneración de intelecto para AMBOS jugadores (segundos por punto)")]
        [Range(1.5f, 5f)]
        public float regenInterval = 2.8f;
    }

    [Header("Ajustes de Dificultad")]
    public DifficultySettings facil = new DifficultySettings
    {
        agresividad = 0.43f,
        intervaloDecision = 1.5f,
        regenInterval = 3.5f
    };

    public DifficultySettings media = new DifficultySettings
    {
        agresividad = 0.6f,
        intervaloDecision = 1.0f,
        regenInterval = 2.8f
    };

    public DifficultySettings dificil = new DifficultySettings
    {
        agresividad = 0.8f,
        intervaloDecision = 0.6f,
        regenInterval = 2.2f
    };


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

    public void SetDificultad(AIDificultad dificultad)
    {
        DifficultySettings settings = null;
        
        switch (dificultad)
        {
            case AIDificultad.Facil:
                Debug.Log("[IAController] Dificultad establecida a FÁCIL");
                settings = facil;
                break;
            case AIDificultad.Media:
                Debug.Log("[IAController] Dificultad establecida a MEDIA");
                settings = media;
                break;
            case AIDificultad.Dificil:
                Debug.Log("[IAController] Dificultad establecida a DIFÍCIL");
                settings = dificil;
                break;
        }

        if (settings != null)
        {
            // Aplicar configuración de comportamiento IA
            agresividad = settings.agresividad;
            intervaloDecision = settings.intervaloDecision;

            // Aplicar velocidad de regeneración de intelecto a la IA
            if (intelectManagerIA != null)
            {
                intelectManagerIA.regenInterval = settings.regenInterval;
            }
            else
            {
                Debug.LogWarning("[IAController] IntelectManager de IA no asignado.");
            }

            // Aplicar velocidad de regeneración de intelecto al jugador
            if (intelectManagerPlayer != null)
            {
                intelectManagerPlayer.regenInterval = settings.regenInterval;
            }
            else
            {
                Debug.LogWarning("[IAController] IntelectManager del jugador no asignado.");
            }

            Debug.Log($"[IAController] Configuración aplicada - Agresividad: {agresividad:F2}, " +
                      $"Intervalo: {intervaloDecision:F2}s, RegenInterval: {settings.regenInterval:F2}s");
        }

        // Actualizar agresividad en las acciones
        InicializarIA();
    }

    private void InicializarIA()
    {
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
        // Si hay amenaza crítica, tomar decisión inmediata (ignorando intervalo)
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
            tiempoAcumulado = 0f; // IMPORTANTE: Resetear después de tomar decisión
            TomarDecision();
        }
    }

    /// Sistema de toma de decisiones - CORAZÓN DE LA IA
    /// Evalúa todas las acciones y ejecuta la mejor
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

    /// Cambia la agresividad de la IA en runtime
    /// Útil para dificultades dinámicas
    public void SetAgresividad(float nuevaAgresividad)
    {
        agresividad = Mathf.Clamp01(nuevaAgresividad);
        Debug.Log($"[IAController] Agresividad ajustada a {agresividad:F2}");
    }

    /// Fuerza a la IA a tomar una decisión inmediatamente
    /// Útil para testing
    public void ForzarDecision()
    {
        Debug.Log("[IAController] Forzando decisión inmediata...");
        TomarDecision();
    }

    /// Devuelve información de estado para UI o debugging
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
