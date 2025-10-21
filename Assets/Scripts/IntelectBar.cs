using UnityEngine;
using UnityEngine.UI;

public class IntelectBar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Slider intelectSlider;
    public IntelectManager intelectManager;
    private int maxIntelect = 10;
    private int minIntelect;
    private int currentIntelect;

    void Start()
    {
        SetIntelect();
        intelectSlider.value = maxIntelect;
    }

    public void SetIntelect()
    {
        maxIntelect = intelectManager.maxIntelect;
        minIntelect = intelectManager.minIntelect;
        
        // Usar el valor float interpolado en lugar del int para suavidad
        float currentValue = intelectManager.GetCurrentIntelectFloat();
        intelectSlider.maxValue = maxIntelect;
        intelectSlider.minValue = minIntelect;
        intelectSlider.value = currentValue;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
