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
        currentIntelect = intelectManager.currentIntelect;
        intelectSlider.value = currentIntelect;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
