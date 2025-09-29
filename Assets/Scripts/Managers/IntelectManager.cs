using UnityEngine;
using System.Collections;

public class IntelectManager : MonoBehaviour
{
    public int maxIntelect = 10;
    public int currentIntelect = 0;
    public float regenInterval = 1f; // segundos entre +1
    public int regenAmount = 1;

    void Start()
    {
        currentIntelect = 0;
        StartCoroutine(RegenerateCoroutine());
    }

    IEnumerator RegenerateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);
            currentIntelect = Mathf.Min(maxIntelect, currentIntelect + regenAmount);
            // Debug.Log($"Intelect: {currentIntelect}/{maxIntelect}");
        }
    }

    public bool CanConsume(int cost)
    {
        return currentIntelect >= cost;
    }

    public bool Consume(int cost)
    {
        if (CanConsume(cost))
        {
            currentIntelect -= cost;
            return true;
        }
        return false;
    }

    // Llamar cuando una defensa muera: añade 1 de intelecto
    public void AddIntelect(int amount)
    {
        currentIntelect = Mathf.Min(maxIntelect, currentIntelect + amount);
    }
}
