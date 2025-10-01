using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntelectManager : MonoBehaviour
{
    

    public int maxIntelect = 10;
    public int minIntelect = 0;
    public int currentIntelect = 10;
    public float regenInterval = 1f; 
    public int regenAmount = 1;
    public IntelectBar intelectSlider;

    void Start()
    {
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
            intelectSlider.SetIntelect();
            return true;
        }
        return false;
    }

    // Llamar cuando una defensa muera: añade 1 de intelecto
    public void AddIntelect(int amount)
    {
        currentIntelect = Mathf.Min(maxIntelect, currentIntelect + amount);
    }

    void Update()
    {
        // DEBUG: pulsando espacio consumimos 1 punto (solo en editor/testing)
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("DEBUG: Space pressed -> intentar consumir 1");
            bool ok = Consume(1);
            Debug.Log($"DEBUG: Consume(1) devolvió {ok}. CurrentIntelect = {currentIntelect}");
        }
    }


}
