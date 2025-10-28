using UnityEngine;

/// <summary>
/// Script de TEST para verificar que el shake de IntelectBar funciona.
/// Añadir a un objeto con IntelectBar y presionar ESPACIO para probar el shake.
/// </summary>
public class TestIntelectBarShake : MonoBehaviour
{
    private IntelectBar intelectBar;

    void Start()
    {
        intelectBar = GetComponent<IntelectBar>();
        if (intelectBar == null)
        {
            Debug.LogError("[TestIntelectBarShake] No se encontró IntelectBar en este objeto!");
        }
        else
        {
            Debug.LogWarning("[TestIntelectBarShake] Presiona ESPACIO para probar el shake");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogWarning("[TestIntelectBarShake] ESPACIO presionado - ejecutando shake");
            if (intelectBar != null)
            {
                intelectBar.ShakeBar(0.5f, 15f);
            }
        }
    }
}
