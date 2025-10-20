using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntelectManager : MonoBehaviour
{

    public int maxIntelect = 10;
    public int minIntelect = 0;
    public int currentIntelect = 10;

    public float regenDelay = 5f;
    public float regenInterval = 5f; 
    public int regenAmount = 1;

    public IntelectBar intelectSlider;
    private Coroutine regenCoroutine;

    void Start()
    {
        currentIntelect = maxIntelect;
        UpdateUI();
    }

    private IEnumerator Regenerate()
    {
        // Espera antes de empezar a regenerar
        yield return new WaitForSeconds(regenDelay);

        // Luego regenera hasta llegar al máximo
        while (currentIntelect < maxIntelect)
        {
            currentIntelect = Mathf.Min(maxIntelect, currentIntelect + regenAmount);
            UpdateUI();
            yield return new WaitForSeconds(regenInterval);
        }

        regenCoroutine = null; 
    }


    public bool CanConsume(int cost)
    {
        return currentIntelect >= cost;
    }

    public bool Consume(int cost)
    {
        if (!CanConsume(cost)) return false;

        currentIntelect -= cost;
        currentIntelect = Mathf.Clamp(currentIntelect, 0, maxIntelect);
        UpdateUI();

        // Reinicia el ciclo de regeneración
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(Regenerate());

        return true;
    }


    public void AddIntelect(int amount)
    {

        currentIntelect = Mathf.Min(maxIntelect, currentIntelect + amount);
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
