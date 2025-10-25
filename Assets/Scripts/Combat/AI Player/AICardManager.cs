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
    [Tooltip("Intervalo MÍNIMO entre acciones (segundos)")]
    [Range(0f, 20f)]
    public float intervaloAccionMin = 1.0f;
    
    [Tooltip("Intervalo MÁXIMO entre acciones (segundos)")]
    [Range(0f, 20f)]
    public float intervaloAccionMax = 3.0f;

    [Tooltip("Agresividad de la IA: controla prioridad entre atacar y defender\n0.0 = Solo defiende\n0.5 = Equilibrado\n1.0 = Solo ataca")]
    [Range(0f, 1f)]
    public float agresividad = 0.5f;

    [Header("Tiempo de Reacción (Defensas)")]
    [Tooltip("Delay MÍNIMO antes de ejecutar una defensa (segundos)")]
    [Range(0f, 20f)]
    public float delayDefensaMin = 0.5f;

    [Tooltip("Delay MÁXIMO antes de ejecutar una defensa (segundos)")]
    [Range(0f, 20f)]
    public float delayDefensaMax = 2.0f;

    [Tooltip("Delay MÍNIMO para defensas de emergencia")]
    [Range(0f, 10f)]
    public float delayDefensaEmergenciaMin = 0.3f;

    [Tooltip("Delay MÁXIMO para defensas de emergencia")]
    [Range(0f, 10f)]
    public float delayDefensaEmergenciaMax = 1.0f;

    [Header("Debug")]
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
        [Tooltip("Agresividad de la IA: controla prioridad entre atacar y defender\n0.0 = Solo defiende\n0.5 = Equilibrado\n1.0 = Solo ataca")]
        [Range(0f, 1f)]
        public float agresividad = 0.5f;

        [Tooltip("Intervalo MÍNIMO entre acciones (segundos)")]
        [Range(0f, 20f)]
        public float intervaloAccionMin = 1.0f;

        [Tooltip("Intervalo MÁXIMO entre acciones (segundos)")]
        [Range(0f, 20f)]
        public float intervaloAccionMax = 3.0f;

        [Header("Tiempo de Reacción (Defensas)")]
        [Tooltip("Delay MÍNIMO antes de ejecutar una defensa (segundos)")]
        [Range(0f, 20f)]
        public float delayDefensaMin = 0.5f;

        [Tooltip("Delay MÁXIMO antes de ejecutar una defensa (segundos)")]
        [Range(0f, 20f)]
        public float delayDefensaMax = 2.0f;

        [Tooltip("Delay MÍNIMO para defensas de emergencia (críticas)")]
        [Range(0f, 10f)]
        public float delayDefensaEmergenciaMin = 0.3f;

        [Tooltip("Delay MÁXIMO para defensas de emergencia (críticas)")]
        [Range(0f, 10f)]
        public float delayDefensaEmergenciaMax = 1.0f;

        [Header("Recursos")]
        [Tooltip("Tiempo de regeneración de intelecto para AMBOS jugadores (segundos por punto)")]
        [Range(1.5f, 5f)]
        public float regenInterval = 2.8f;
    }

    [Header("Ajustes de Dificultad")]
    public DifficultySettings facil = new DifficultySettings
    {
        agresividad = 0.3f,
        intervaloAccionMin = 3.0f,
        intervaloAccionMax = 5.0f,
        delayDefensaMin = 1.5f,
        delayDefensaMax = 3.0f,
        delayDefensaEmergenciaMin = 0.8f,
        delayDefensaEmergenciaMax = 1.5f,
        regenInterval = 3.5f
    };

    public DifficultySettings media = new DifficultySettings
    {
        agresividad = 0.5f,
        intervaloAccionMin = 1.5f,
        intervaloAccionMax = 3.0f,
        delayDefensaMin = 0.8f,
        delayDefensaMax = 2.0f,
        delayDefensaEmergenciaMin = 0.4f,
        delayDefensaEmergenciaMax = 1.0f,
        regenInterval = 2.8f
    };

    public DifficultySettings dificil = new DifficultySettings
    {
        agresividad = 0.7f,
        intervaloAccionMin = 0.8f,
        intervaloAccionMax = 1.5f,
        delayDefensaMin = 0.3f,
        delayDefensaMax = 1.0f,
        delayDefensaEmergenciaMin = 0.1f,
        delayDefensaEmergenciaMax = 0.5f,
        regenInterval = 2.2f
    };


    // Componentes internos
    private AICardHand manoIA;
    private AIThreatDetector detectorAmenazas;
    private AISpawnPositionCalculator spawnCalculator;
    private List<AIAction> accionesPosibles;
    
    // ⚡ COOLDOWN UNIFICADO
    private float tiempoDesdeUltimaAccion = 0f;
    private float intervaloAccionActual = 0f; // Intervalo aleatorio actual

    // ⏱️ DELAY DE REACCIÓN PARA DEFENSAS
    private float tiempoEsperandoDefensa = 0f;
    private float delayDefensaActual = 0f;
    private bool esperandoParaDefender = false;
    private AIAction accionDefensaPendiente = null;

    // 🔍 DETECCIÓN DE BLOQUEO
    private int contadorEsperasConsecutivas = 0;
    private const int MAX_ESPERAS_CONSECUTIVAS = 5;
    private float tiempoUltimaAccionReal = 0f;
    private const float MAX_TIEMPO_SIN_ACCION = 15f;


    void Start()
    {
        // Validaciones
        if (!ValidarReferencias()) return;

        // Inicializar componentes
        InicializarIA();

        // ⚡ Resetear cooldown inicial para evitar que la IA actúe inmediatamente
        intervaloAccionActual = UnityEngine.Random.Range(intervaloAccionMin, intervaloAccionMax);
        tiempoDesdeUltimaAccion = 0f;

        // Inicializar timer
        tiempoUltimaAccionReal = Time.time;

        Debug.Log($"[IAController] ✅ IA inicializada con agresividad {agresividad:F2} - Primera acción en {intervaloAccionActual:F2}s");
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
            intervaloAccionMin = settings.intervaloAccionMin;
            intervaloAccionMax = settings.intervaloAccionMax;
            delayDefensaMin = settings.delayDefensaMin;
            delayDefensaMax = settings.delayDefensaMax;
            delayDefensaEmergenciaMin = settings.delayDefensaEmergenciaMin;
            delayDefensaEmergenciaMax = settings.delayDefensaEmergenciaMax;

            // Aplicar velocidad de regeneración de intelecto
            if (intelectManagerIA != null)
            {
                intelectManagerIA.regenInterval = settings.regenInterval;
            }

            if (intelectManagerPlayer != null)
            {
                intelectManagerPlayer.regenInterval = settings.regenInterval;
            }

            Debug.Log($"[IAController] Configuración aplicada:\n" +
                      $"  - Agresividad: {agresividad:F2}\n" +
                      $"  - Intervalo Acción: {intervaloAccionMin:F2}s - {intervaloAccionMax:F2}s\n" +
                      $"  - Delay Defensa: {delayDefensaMin:F2}s - {delayDefensaMax:F2}s\n" +
                      $"  - Delay Emergencia: {delayDefensaEmergenciaMin:F2}s - {delayDefensaEmergenciaMax:F2}s\n" +
                      $"  - RegenInterval: {settings.regenInterval:F2}s");
        }

        // Actualizar agresividad en las acciones
        InicializarIA();
    }

    private void InicializarIA()
    {
        manoIA = new AICardHand(cardManager);

        // Crear detector de amenazas
        detectorAmenazas = new AIThreatDetector(torreIA);

        // Crear calculador de posiciones de spawn
        spawnCalculator = new AISpawnPositionCalculator(spawnPointIA, torreIA, detectorAmenazas);

        // Crear todas las acciones posibles
        accionesPosibles = new List<AIAction>
        {
            new AccionDefender(
                intelectManagerIA,
                cardManager,
                manoIA,
                detectorAmenazas,
                spawnPointIA,
                torreIA,
                spawnCalculator
            ),

            new AccionAtacarConSuma(
                intelectManagerIA,
                cardManager,
                manoIA,
                detectorAmenazas,
                spawnPointIA,
                agresividad,
                spawnCalculator
            ),

            new AccionAtacarConResta(
                intelectManagerIA,
                cardManager,
                manoIA,
                detectorAmenazas,
                spawnPointIA,
                agresividad,
                spawnCalculator
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
        // Si estamos esperando para ejecutar una defensa
        if (esperandoParaDefender && accionDefensaPendiente != null)
        {
            tiempoEsperandoDefensa += Time.deltaTime;
            
            if (tiempoEsperandoDefensa >= delayDefensaActual)
            {
                // Ejecutar la defensa pendiente
                if (debugMode) Debug.Log($"[IA] ⏱️ Delay de defensa completado ({delayDefensaActual:F2}s) - Ejecutando ahora");
                accionDefensaPendiente.Ejecutar();
                
                // Resetear cooldown normal
                intervaloAccionActual = UnityEngine.Random.Range(intervaloAccionMin, intervaloAccionMax);
                tiempoDesdeUltimaAccion = 0f;
                tiempoUltimaAccionReal = Time.time;
                contadorEsperasConsecutivas = 0;
                
                // Limpiar estado de espera
                esperandoParaDefender = false;
                accionDefensaPendiente = null;
                tiempoEsperandoDefensa = 0f;
                
                return;
            }
            else
            {
                // Aún esperando
                if (debugMode && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[IA] ⏳ Esperando para defender... {tiempoEsperandoDefensa:F2}/{delayDefensaActual:F2}s");
                }
                return;
            }
        }
        
        // Lógica normal de cooldown
        tiempoDesdeUltimaAccion += Time.deltaTime;

        if (tiempoDesdeUltimaAccion >= intervaloAccionActual)
        {
            TomarDecision();
        }
        else if (debugMode && Time.frameCount % 120 == 0)
        {
            Debug.Log($"[IA] ⏳ Esperando cooldown... {tiempoDesdeUltimaAccion:F2}/{intervaloAccionActual:F2}s");
        }
    }

    /// <summary>
    /// Sistema de toma de decisiones - CORAZÓN DE LA IA
    /// Evalúa todas las acciones disponibles y ejecuta la mejor según agresividad
    /// </summary>
    private void TomarDecision()
    {
        if (debugMode)
        {
            Debug.Log($"\n========== [IA] NUEVA DECISIÓN (t={Time.time:F1}s) ==========");
            Debug.Log($"[IA] Cooldown listo - Puede actuar");
            Debug.Log($"[IA] Intelecto: {intelectManagerIA.currentIntelect}/{intelectManagerIA.maxIntelect}");
            Debug.Log($"[IA] Cartas en mano: {manoIA.CantidadCartas()}");
            Debug.Log($"[IA] Amenazas detectadas: {detectorAmenazas.ContarAmenazas()}");
            Debug.Log($"[IA] Agresividad: {agresividad:F2}");
            
            // 🔍 DIAGNÓSTICO DETALLADO DE CARTAS
            var todasLasCartas = manoIA.ObtenerTodasLasCartas();
            string cartasStr = "Cartas en mano: [";
            foreach (var c in todasLasCartas)
            {
                cartasStr += c.cardValue + ", ";
            }
            cartasStr += "]";
            Debug.Log($"[IA] {cartasStr}");
            
            // 🔍 VERIFICAR COMBOS POSIBLES
            var comboSuma = manoIA.EncontrarMejorComboSuma();
            var comboResta = manoIA.EncontrarMejorComboResta();
            Debug.Log($"[IA] Combo Suma disponible: {(comboSuma != null ? comboSuma.ToString() : "NINGUNO")}");
            Debug.Log($"[IA] Combo Resta disponible: {(comboResta != null ? comboResta.ToString() : "NINGUNO")}");
            
            // 🔍 VERIFICAR AMENAZAS Y CAPACIDAD DE DEFENSA
            var amenazaMasPeligrosa = detectorAmenazas.ObtenerAmenazaMasPeligrosa();
            if (amenazaMasPeligrosa != null)
            {
                Debug.Log($"[IA] Amenaza más peligrosa: Valor={amenazaMasPeligrosa.valor}, Dist={amenazaMasPeligrosa.distancia:F1}m, Peligro={amenazaMasPeligrosa.peligrosidad:F2}");
                // La IA siempre tiene cartas 1-5, solo verificar intelecto
                bool tieneIntelecto = intelectManagerIA.currentIntelect >= amenazaMasPeligrosa.valor;
                Debug.Log($"[IA] ¿Puedo defender (intelecto {intelectManagerIA.currentIntelect} >= {amenazaMasPeligrosa.valor})? {(tieneIntelecto ? "SÍ" : "NO")}");
            }
        }

        // ===== PASO 1: EVALUAR TODAS LAS ACCIONES DISPONIBLES =====
        float mejorScore = float.MinValue;
        AIAction mejorAccion = null;

        foreach (AIAction accion in accionesPosibles)
        {
            // Calcular score BASE de esta acción
            float scoreBase = accion.CalcularScore();
            
            // Si el score base es 0 o inviable, saltar
            if (scoreBase <= 0.001f)
            {
                if (debugMode)
                {
                    Debug.Log($"[IA]   • {accion.nombreAccion}: {scoreBase:F3} (inviable)");
                }
                continue;
            }

            float scoreFinal = scoreBase;

            // ⚡ APLICAR MULTIPLICADOR DE AGRESIVIDAD
            // Agresividad controla la prioridad entre atacar y defender:
            // - agresividad = 0.0 → Solo defiende (ataque x0.0, defensa x2.0)
            // - agresividad = 0.5 → Equilibrado (ataque x1.0, defensa x1.0)
            // - agresividad = 1.0 → Solo ataca (ataque x2.0, defensa x0.0)
            
            if (accion.tipoAccion == AIAction.TipoAccion.Ataque)
            {
                // Multiplicador de ataque: 0.0 (agr=0) hasta 2.0 (agr=1)
                float multiplicadorAtaque = agresividad * 2.0f;
                scoreFinal *= multiplicadorAtaque;
                
                if (debugMode)
                {
                    Debug.Log($"[IA]   • {accion.nombreAccion}: {scoreBase:F3} → {scoreFinal:F3} (x{multiplicadorAtaque:F2} agr)");
                }
            }
            else if (accion.tipoAccion == AIAction.TipoAccion.Defensa)
            {
                // Multiplicador de defensa: 2.0 (agr=0) hasta 0.0 (agr=1)
                float multiplicadorDefensa = (1.0f - agresividad) * 2.0f;
                scoreFinal *= multiplicadorDefensa;
                
                if (debugMode)
                {
                    Debug.Log($"[IA]   • {accion.nombreAccion}: {scoreBase:F3} → {scoreFinal:F3} (x{multiplicadorDefensa:F2} def)");
                }
            }
            else // Neutral (Esperar)
            {
                if (debugMode)
                {
                    Debug.Log($"[IA]   • {accion.nombreAccion}: {scoreFinal:F3} (neutral)");
                }
            }

            // ¿Es mejor que la mejor actual?
            if (scoreFinal > mejorScore)
            {
                mejorScore = scoreFinal;
                mejorAccion = accion;
            }
        }

        // ===== PASO 2: EJECUTAR LA MEJOR ACCIÓN Y RESETEAR COOLDOWN =====
        if (mejorAccion != null && mejorScore > 0.001f)
        {
            if (debugMode)
            {
                Debug.Log($"[IA] ⭐ DECISIÓN: {mejorAccion.nombreAccion} (score: {mejorScore:F3})");
            }

            // 🔍 DETECTAR BLOQUEO POR ESPERAS CONSECUTIVAS
            if (mejorAccion.nombreAccion == "Esperar")
            {
                contadorEsperasConsecutivas++;
                float tiempoSinAccion = Time.time - tiempoUltimaAccionReal;
                
                // 🔧 VERIFICAR SI LA ESPERA ES JUSTIFICADA
                bool esperaJustificada = false;
                string razonEspera = "";
                
                // Razón 1: Sin intelecto suficiente para hacer NADA
                if (intelectManagerIA.currentIntelect < 1)
                {
                    esperaJustificada = true;
                    razonEspera = "sin intelecto";
                }
                // Razón 2: Hay amenazas pero no tengo intelecto para defenderlas
                else if (detectorAmenazas.ContarAmenazas() > 0)
                {
                    var amenaza = detectorAmenazas.ObtenerAmenazaMasPeligrosa();
                    if (amenaza != null)
                    {
                        bool tieneIntelectoParaDefender = intelectManagerIA.currentIntelect >= amenaza.valor;
                        
                        if (!tieneIntelectoParaDefender)
                        {
                            esperaJustificada = true;
                            razonEspera = $"amenaza valor {amenaza.valor} pero solo tengo {intelectManagerIA.currentIntelect} de intelecto";
                        }
                    }
                }
                // Razón 3: Sin intelecto para atacar (necesita al menos 2 para combo mínimo 1+1=2)
                else if (detectorAmenazas.ContarAmenazas() == 0 && intelectManagerIA.currentIntelect < 2)
                {
                    esperaJustificada = true;
                    razonEspera = "sin amenazas y sin intelecto para atacar (necesita ≥2)";
                }
                
                if (esperaJustificada)
                {
                    Debug.Log($"[IA] ⏸️ Espera JUSTIFICADA #{contadorEsperasConsecutivas} - Razón: {razonEspera}");
                    // Reset del contador de tiempo para no activar bloqueo temporal
                    tiempoUltimaAccionReal = Time.time - (MAX_TIEMPO_SIN_ACCION * 0.5f); // Dar más margen
                }
                else
                {
                    Debug.LogWarning($"[IA] ⚠️ Espera consecutiva #{contadorEsperasConsecutivas}/{MAX_ESPERAS_CONSECUTIVAS} (tiempo sin acción: {tiempoSinAccion:F1}s)");
                }
                
                // 🚨 SISTEMA DE EMERGENCIA: Múltiples condiciones de bloqueo
                bool bloqueoConsecutivo = contadorEsperasConsecutivas >= MAX_ESPERAS_CONSECUTIVAS && !esperaJustificada;
                bool bloqueoTemporal = tiempoSinAccion >= MAX_TIEMPO_SIN_ACCION && !esperaJustificada;
                
                if (bloqueoConsecutivo || bloqueoTemporal)
                {
                    string razon = bloqueoConsecutivo ? $"{MAX_ESPERAS_CONSECUTIVAS} esperas consecutivas" : $"{tiempoSinAccion:F1}s sin actuar";
                    Debug.LogError($"[IA] 🚨 BLOQUEO DETECTADO: {razon}!");
                    Debug.LogError($"[IA] Estado crítico - Intelecto: {intelectManagerIA.currentIntelect}, Cartas: {manoIA.CantidadCartas()}, Amenazas: {detectorAmenazas.ContarAmenazas()}");
                    
                    // � RESET COMPLETO
                    contadorEsperasConsecutivas = 0;
                    tiempoUltimaAccionReal = Time.time;
                    
                    // 🔧 CRÍTICO: Limpiar marcas de amenazas defendidas para poder reevaluarlas
                    detectorAmenazas.LimpiarTodasLasMarcas();
                    
                    // 🚑 FORZAR ACCIÓN DE EMERGENCIA (RESPETANDO AGRESIVIDAD)
                    AIAction accionEmergencia = null;
                    float mejorScoreEmergencia = -1f;
                    
                    Debug.LogWarning($"[IA] 🚑 Evaluando acciones de emergencia...");
                    
                    foreach (AIAction accion in accionesPosibles)
                    {
                        if (accion.nombreAccion == "Esperar") continue;
                        
                        float scoreBase = accion.CalcularScore();
                        
                        if (scoreBase <= 0f) continue;
                        
                        float scoreFinalEmergencia = scoreBase;
                        
                        // ⚡ APLICAR MULTIPLICADOR DE AGRESIVIDAD
                        if (accion.tipoAccion == AIAction.TipoAccion.Ataque)
                        {
                            scoreFinalEmergencia *= agresividad * 2.0f;
                        }
                        else if (accion.tipoAccion == AIAction.TipoAccion.Defensa)
                        {
                            scoreFinalEmergencia *= (1.0f - agresividad) * 2.0f;
                        }
                        
                        if (scoreFinalEmergencia > mejorScoreEmergencia)
                        {
                            mejorScoreEmergencia = scoreFinalEmergencia;
                            accionEmergencia = accion;
                        }
                    }
                    
                    if (accionEmergencia != null && mejorScoreEmergencia > 0.001f)
                    {
                        Debug.LogWarning($"[IA] 🚑 EMERGENCIA: Forzando {accionEmergencia.nombreAccion} (score: {mejorScoreEmergencia:F3})");
                        
                        // Si es defensa de emergencia, aplicar delay también
                        if (accionEmergencia.tipoAccion == AIAction.TipoAccion.Defensa)
                        {
                            delayDefensaActual = UnityEngine.Random.Range(delayDefensaEmergenciaMin, delayDefensaEmergenciaMax);
                            esperandoParaDefender = true;
                            accionDefensaPendiente = accionEmergencia;
                            tiempoEsperandoDefensa = 0f;
                            
                            if (debugMode) Debug.Log($"[IA] 🛡️ Defensa de emergencia - delay: {delayDefensaActual:F2}s");
                        }
                        else
                        {
                            // No es defensa, ejecutar inmediatamente
                            accionEmergencia.Ejecutar();
                            intervaloAccionActual = UnityEngine.Random.Range(intervaloAccionMin, intervaloAccionMax);
                            tiempoDesdeUltimaAccion = 0f;
                            tiempoUltimaAccionReal = Time.time;
                        }
                        
                        if (debugMode)
                        {
                            Debug.Log("==========================================================\n");
                        }
                        return;
                    }
                    else
                    {
                        Debug.LogError("[IA] 💀 CRÍTICO: No hay acciones de emergencia viables.");
                        tiempoDesdeUltimaAccion = intervaloAccionMin * 0.9f;
                    }
                }
            }
            else
            {
                // Reset contadores si ejecuta algo que NO es esperar
                contadorEsperasConsecutivas = 0;
                tiempoUltimaAccionReal = Time.time;
            }

            // 🛡️ Si es una DEFENSA, aplicar delay de reacción
            if (mejorAccion.tipoAccion == AIAction.TipoAccion.Defensa)
            {
                delayDefensaActual = UnityEngine.Random.Range(delayDefensaMin, delayDefensaMax);
                esperandoParaDefender = true;
                accionDefensaPendiente = mejorAccion;
                tiempoEsperandoDefensa = 0f;
                
                if (debugMode) Debug.Log($"[IA] 🛡️ Defensa detectada - aplicando delay de reacción: {delayDefensaActual:F2}s");
                return; // NO ejecutar aún, esperar el delay
            }

            // Ejecutar acción normal (ataque, esperar, etc.)
            mejorAccion.Ejecutar();

            // ⚡ RESETEAR COOLDOWN
            if (mejorAccion.tipoAccion != AIAction.TipoAccion.Neutral)
            {
                // Acción real: generar nuevo intervalo aleatorio y resetear
                intervaloAccionActual = UnityEngine.Random.Range(intervaloAccionMin, intervaloAccionMax);
                tiempoDesdeUltimaAccion = 0f;
                if (debugMode) Debug.Log($"[IA] ⏱️ Cooldown reseteado - próxima acción en {intervaloAccionActual:F2}s");
            }
            else
            {
                // Esperar: reintentar en la mitad del intervalo mínimo
                tiempoDesdeUltimaAccion = intervaloAccionMin * 0.5f;
                if (debugMode) Debug.Log($"[IA] ⏱️ Esperando - reintento en {intervaloAccionMin * 0.5f:F2}s");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning($"[IA] ❌ No hay acciones viables (mejor score: {mejorScore:F3})");
                Debug.LogWarning($"[IA] 🔍 Estado: Intelecto={intelectManagerIA.currentIntelect}/{intelectManagerIA.maxIntelect}, Cartas={manoIA.CantidadCartas()}, Amenazas={detectorAmenazas.ContarAmenazas()}");
            }
            
            // � NO resetear cooldown si no hay acciones viables
            // Dejar que siga acumulando para intentar de nuevo
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
    /// Cambia la agresividad de la IA en runtime.
    /// 0.0 = Solo defiende | 0.5 = Equilibrado | 1.0 = Solo ataca
    /// </summary>
    public void SetAgresividad(float nuevaAgresividad)
    {
        agresividad = Mathf.Clamp01(nuevaAgresividad);
        Debug.Log($"[IAController] Agresividad ajustada a {agresividad:F2}");
    }

    /// <summary>
    /// Fuerza a la IA a tomar una decisión inmediatamente (útil para testing)
    /// </summary>
    public void ForzarDecision()
    {
        Debug.Log("[IAController] Forzando decisión inmediata...");
        TomarDecision(); // Sin parámetros ahora que usamos cooldown unificado
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
