using UnityEngine;
using System.Diagnostics;

/// <summary>
/// Helper para Debug.Log que se elimina automáticamente en builds de producción.
/// Usar DebugHelper.Log() en vez de Debug.Log() para mejor performance.
/// Los logs solo aparecen en Editor y Development Builds.
/// </summary>
public static class DebugHelper
{
    /// <summary>
    /// Log normal - solo en Editor y Development builds
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    /// <summary>
    /// Log con contexto - solo en Editor y Development builds
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }

    /// <summary>
    /// Warning - solo en Editor y Development builds
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    /// <summary>
    /// Warning con contexto - solo en Editor y Development builds
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message, Object context)
    {
        UnityEngine.Debug.LogWarning(message, context);
    }

    /// <summary>
    /// Error - SIEMPRE se muestra (incluso en producción)
    /// Usar solo para errores críticos
    /// </summary>
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }

    /// <summary>
    /// Error con contexto - SIEMPRE se muestra
    /// </summary>
    public static void LogError(object message, Object context)
    {
        UnityEngine.Debug.LogError(message, context);
    }
}
