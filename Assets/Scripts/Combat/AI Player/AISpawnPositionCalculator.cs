using UnityEngine;
using UnityEngine.AI; // Agregar para NavMesh

/// <summary>
/// Calcula la mejor posición de spawn para la IA basándose en la situación táctica
/// </summary>
public class AISpawnPositionCalculator
{
    private Transform spawnArea;
    private Transform torreIA;
    private AIThreatDetector threatDetector;

    public AISpawnPositionCalculator(Transform spawn, Transform torre, AIThreatDetector detector)
    {
        spawnArea = spawn;
        torreIA = torre;
        threatDetector = detector;
    }

    /// <summary>
    /// Calcula la mejor posición para spawmear una DEFENSA
    /// - Cerca de la amenaza para interceptarla
    /// - Dentro del área de spawn permitida
    /// - Sobre el NavMesh
    /// </summary>
    public Vector3 CalcularMejorPosicionDefensa(Vector3 posicionAmenaza)
    {
        BoxCollider spawnBox = spawnArea.GetComponent<BoxCollider>();
        
        if (spawnBox == null)
        {
            // Fallback: posición aleatoria cerca de la amenaza
            Vector3 pos = CalcularPosicionAleatoria(0.7f);
            return ProyectarAlNavMesh(pos);
        }

        // Obtener bounds del área de spawn
        Bounds bounds = GetSpawnBounds(spawnBox);

        // Estrategia: Spawnear EN LA LÍNEA entre la amenaza y la torre
        // Esto maximiza la probabilidad de interceptación
        Vector3 direccionAmenaza = (posicionAmenaza - torreIA.position).normalized;
        
        // Distancia desde la torre hacia la amenaza (dentro del área de spawn)
        float distanciaOptima = bounds.size.z * 0.6f; // 60% hacia adelante
        Vector3 posicionIdeal = torreIA.position + direccionAmenaza * distanciaOptima;

        // Clampear dentro del área de spawn
        Vector3 posicionFinal = ClampToBounds(posicionIdeal, bounds);
        posicionFinal.y = spawnArea.position.y;

        // ⚡ PROYECTAR AL NAVMESH
        posicionFinal = ProyectarAlNavMesh(posicionFinal);

        Debug.Log($"[AI Spawn] Defensa: Amenaza en {posicionAmenaza}, spawn en {posicionFinal}");
        
        return posicionFinal;
    }

    /// <summary>
    /// Calcula la mejor posición para spawner un ATAQUE
    /// - Distribuida tácticamente en el área
    /// - Evita zonas con muchos defenders enemigos
    /// - Variación estratégica en profundidad
    /// - Sobre el NavMesh
    /// </summary>
    public Vector3 CalcularMejorPosicionAtaque()
    {
        BoxCollider spawnBox = spawnArea.GetComponent<BoxCollider>();
        
        if (spawnBox == null)
        {
            Vector3 pos = CalcularPosicionAleatoria(0.3f);
            return ProyectarAlNavMesh(pos);
        }

        Bounds bounds = GetSpawnBounds(spawnBox);

        // Detectar defensores enemigos en el campo
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("PlayerTeam");
        
        // Evaluar varias posiciones candidatas (aumentado para más variedad)
        int numCandidatos = 8; // Aumentado de 5 a 8
        Vector3 mejorPosicion = spawnArea.position;
        float mejorScore = float.MinValue;

        for (int i = 0; i < numCandidatos; i++)
        {
            // Generar posición candidata con variedad
            Vector3 candidata = GenerarPosicionCandidataAtaque(bounds, i);
            
            // Evaluar score de esta posición
            float score = EvaluarPosicionAtaque(candidata, enemigos, bounds);
            
            if (score > mejorScore)
            {
                mejorScore = score;
                mejorPosicion = candidata;
            }
        }

        mejorPosicion.y = spawnArea.position.y;
        
        // ⚡ PROYECTAR AL NAVMESH
        mejorPosicion = ProyectarAlNavMesh(mejorPosicion);
        
        Debug.Log($"[AI Spawn] Ataque: Mejor posición {mejorPosicion} con score {mejorScore:F2}");
        
        return mejorPosicion;
    }

    /// <summary>
    /// Genera una posición candidata para ataque con variedad estratégica
    /// </summary>
    private Vector3 GenerarPosicionCandidataAtaque(Bounds bounds, int indice)
    {
        // Estrategias variadas según el índice
        float randomX, randomZ;
        
        if (indice < 3)
        {
            // Primeros 3: Centro con variación
            randomX = Random.Range(-bounds.extents.x * 0.4f, bounds.extents.x * 0.4f);
            randomZ = Random.Range(-bounds.extents.z, bounds.extents.z);
        }
        else if (indice < 5)
        {
            // Siguientes 2: Flanqueo izquierdo/derecho
            randomX = (indice == 3) 
                ? Random.Range(-bounds.extents.x, -bounds.extents.x * 0.6f)  // Izquierda
                : Random.Range(bounds.extents.x * 0.6f, bounds.extents.x);   // Derecha
            randomZ = Random.Range(-bounds.extents.z, bounds.extents.z);
        }
        else
        {
            // Últimos 3: Completamente aleatorio (exploración)
            randomX = Random.Range(-bounds.extents.x, bounds.extents.x);
            randomZ = Random.Range(-bounds.extents.z, bounds.extents.z);
        }
        
        Vector3 offset = new Vector3(randomX, 0, randomZ);
        return bounds.center + offset;
    }

    /// <summary>
    /// Evalúa qué tan buena es una posición para atacar
    /// </summary>
    private float EvaluarPosicionAtaque(Vector3 posicion, GameObject[] enemigos, Bounds bounds)
    {
        float score = 1.0f;

        // FACTOR 1: Evitar zonas congestionadas (distancia a enemigos cercanos)
        float distanciaMinEnemigo = float.MaxValue;
        int enemigosNear = 0;

        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo == null) continue;
            
            float distancia = Vector3.Distance(posicion, enemigo.transform.position);
            
            if (distancia < distanciaMinEnemigo)
                distanciaMinEnemigo = distancia;
            
            if (distancia < 3f) // Radio de congestión
                enemigosNear++;
        }

        // Penalizar zonas congestionadas
        if (enemigosNear > 0)
        {
            score *= Mathf.Max(0.3f, 1f - (enemigosNear * 0.2f));
        }

        // Bonus por distancia razonable (no muy cerca, no muy lejos)
        if (distanciaMinEnemigo > 2f && distanciaMinEnemigo < 8f)
        {
            score *= 1.3f;
        }

        // FACTOR 2: Preferir posiciones centrales horizontalmente (mejor camino)
        float distanciaCentroX = Mathf.Abs(posicion.x - bounds.center.x);
        float factorCentrado = 1f - (distanciaCentroX / bounds.extents.x);
        score *= (0.7f + factorCentrado * 0.3f); // Bonus leve por estar centrado

        // FACTOR 3: Variación en profundidad está bien (explorar distintas líneas)
        // No penalizar ni bonificar, ya es buena variación táctica

        return score;
    }

    /// <summary>
    /// Calcula posición aleatoria simple (fallback)
    /// </summary>
    private Vector3 CalcularPosicionAleatoria(float biasAdelante)
    {
        BoxCollider spawnBox = spawnArea.GetComponent<BoxCollider>();
        
        if (spawnBox != null)
        {
            Vector3 halfSize = spawnBox.size * 0.5f;
            
            float randomX = Random.Range(-halfSize.x, halfSize.x);
            float randomZ = Random.Range(-halfSize.z * (1f - biasAdelante), halfSize.z * biasAdelante);
            
            return spawnArea.position + spawnArea.TransformDirection(new Vector3(randomX, 0, randomZ));
        }
        else
        {
            float randomX = Random.Range(-2.5f, 2.5f);
            float randomZ = Random.Range(-2.5f * (1f - biasAdelante), 2.5f * biasAdelante);
            
            return spawnArea.position + new Vector3(randomX, 0, randomZ);
        }
    }

    /// <summary>
    /// Obtiene los bounds del área de spawn en coordenadas mundiales
    /// </summary>
    private Bounds GetSpawnBounds(BoxCollider spawnBox)
    {
        // Transform local bounds to world space
        Vector3 center = spawnArea.TransformPoint(spawnBox.center);
        Vector3 size = Vector3.Scale(spawnBox.size, spawnArea.lossyScale);
        
        return new Bounds(center, size);
    }

    /// <summary>
    /// Clampea una posición dentro de los bounds del área
    /// </summary>
    private Vector3 ClampToBounds(Vector3 position, Bounds bounds)
    {
        return new Vector3(
            Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
            position.y,
            Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
        );
    }

    /// <summary>
    /// Proyecta una posición al NavMesh más cercano
    /// Esto es CRÍTICO para que las tropas puedan spawnearse correctamente
    /// </summary>
    private Vector3 ProyectarAlNavMesh(Vector3 posicion)
    {
        NavMeshHit hit;
        
        // Buscar el punto más cercano en el NavMesh (radio de búsqueda: 50 unidades - AMPLIADO)
        if (NavMesh.SamplePosition(posicion, out hit, 50f, NavMesh.AllAreas))
        {
            // Usar la posición del NavMesh encontrada
            Debug.Log($"[AI Spawn] ✅ Posición proyectada al NavMesh: {posicion} → {hit.position} (distancia: {hit.distance:F2})");
            return hit.position;
        }
        else
        {
            // Si no se encuentra NavMesh cercano, intentar con la posición del spawnArea
            Debug.LogError($"[AI Spawn] ❌ No se encontró NavMesh cerca de {posicion}. Usando fallback a spawnArea.");
            
            // Intentar proyectar desde el centro del área de spawn
            if (NavMesh.SamplePosition(spawnArea.position, out hit, 50f, NavMesh.AllAreas))
            {
                Debug.Log($"[AI Spawn] ✅ Usando centro de spawn área: {hit.position}");
                return hit.position;
            }
            
            // Último recurso: posición original (probablemente fallará)
            Debug.LogError($"[AI Spawn] ⚠️⚠️ CRÍTICO: No hay NavMesh en el área de spawn de la IA!");
            return posicion;
        }
    }
}
