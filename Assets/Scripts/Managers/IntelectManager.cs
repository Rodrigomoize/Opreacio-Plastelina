using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Sistema de regeneración de Intelecto (Elixir) estilo Clash Royale
/// Valores por defecto: 1 punto cada 2.8s (velocidad normal de CR)
/// </summary>
public class IntelectManager : MonoBehaviour
{

    public int maxIntelect = 10;
    public int minIntelect = 0;
    public int currentIntelect = 4;

    public float regenDelay = 0f;
    public float regenInterval = 2.8f; 
    public int regenAmount = 1;

    public IntelectBar intelectSlider;
    private Coroutine regenCoroutine;
    
    // Para interpolación suave del slider
    private float currentIntelectFloat = 4f;
    private float targetIntelectFloat = 4f;

    void Start()
    {
        //currentIntelect = maxIntelect;
        currentIntelectFloat = currentIntelect;
        targetIntelectFloat = currentIntelect;
        UpdateUI();
        
        // Iniciar regeneración automática desde el inicio (como Clash Royale)
        if (regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(Regenerate());
        }
    }

    void Update()
    {
        // Interpolar suavemente hacia el valor objetivo
        if (currentIntelectFloat < targetIntelectFloat)
        {
            // Incrementar a una velocidad constante: 1 punto por regenInterval segundos
            currentIntelectFloat += (regenAmount * Time.deltaTime) / regenInterval;
            currentIntelectFloat = Mathf.Min(currentIntelectFloat, targetIntelectFloat);
            UpdateUI();
        }
    }

    private IEnumerator Regenerate()
    {
        // Espera antes de empezar a regenerar
        yield return new WaitForSeconds(regenDelay);

        // Luego regenera hasta llegar al máximo
        while (currentIntelectFloat < maxIntelect)
        {
            // Incrementar el objetivo en +1 desde donde esté actualmente
            targetIntelectFloat = Mathf.Min(maxIntelect, targetIntelectFloat + regenAmount);
            
            // Esperar el intervalo de regeneración
            yield return new WaitForSeconds(regenInterval);
            
            // DESPUÉS del tiempo, actualizar el valor entero basado en el floor del float actual
            currentIntelect = Mathf.FloorToInt(currentIntelectFloat);
        }

        regenCoroutine = null; 
    }


    public bool CanConsume(int cost)
    {
        return currentIntelect >= cost;
    }

    /// <summary>
    /// Obtiene el valor actual interpolado del intelecto (para UI suave)
    /// </summary>
    public float GetCurrentIntelectFloat()
    {
        return currentIntelectFloat;
    }

    public bool Consume(int cost)
    {
        if (!CanConsume(cost)) return false;

        currentIntelect -= cost;
        currentIntelect = Mathf.Clamp(currentIntelect, 0, maxIntelect);
        
        // Actualizar valores de interpolación instantáneamente cuando se consume
        currentIntelectFloat = currentIntelect;
        targetIntelectFloat = currentIntelect;
        
        UpdateUI();

        // Reinicia el ciclo de regeneración
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(Regenerate());

        return true;
    }


    public void AddIntelect(int amount)
    {
        // Incrementar directamente ambos valores (actual y objetivo) para bonus instantáneo
        currentIntelectFloat = Mathf.Min(maxIntelect, currentIntelectFloat + amount);
        targetIntelectFloat = Mathf.Min(maxIntelect, targetIntelectFloat + amount);
        
        // Actualizar el valor entero al floor del float
        currentIntelect = Mathf.FloorToInt(currentIntelectFloat);
        
        UpdateUI();
        
        // NO reiniciar la corrutina - dejarla continuar su ciclo natural
        // La regeneración seguirá funcionando normalmente en segundo plano
    }

    void UpdateUI()
    {
        if (intelectSlider != null)
        {
            intelectSlider.SetIntelect();
        }
        else
        {
            UnityEngine.Debug.LogWarning("[IntelectManager] intelectSlider no asignado.");
        }
    }



}
