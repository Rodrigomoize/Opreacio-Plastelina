using UnityEngine;

/// <summary>
/// Clase base abstracta para todas las acciones de la IA
/// Cada acción (Defender, Atacar, Esperar) hereda de esta
/// </summary>
public abstract class AIAction
{
    public string nombreAccion;
    public float scoreFinal;

    protected AIAction(string nombre)
    {
        nombreAccion = nombre;
        scoreFinal = 0f;
    }

    public abstract float CalcularScore();

    public abstract void Ejecutar();

    protected float Normalizar(float valor, float min, float max)
    {
        if (max - min == 0) return 0f;
        return Mathf.Clamp01((valor - min) / (max - min));
    }

    protected float CurvaCuadratica(float x)
    {
        return x * x;
    }

    protected float CurvaCubica(float x)
    {
        return x * x * x;
    }


    protected float CurvaInversa(float x)
    {
        return 1f - Mathf.Clamp01(x);
    }


    protected float CurvaRaizCuadrada(float x)
    {
        return Mathf.Sqrt(Mathf.Clamp01(x));
    }


    protected float CurvaSigmoide(float x, float pendiente = 10f)
    {
        return 1f / (1f + Mathf.Exp(-pendiente * (x - 0.5f)));
    }
}