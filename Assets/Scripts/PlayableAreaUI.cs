using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayableAreaUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    public PlayerCardManager playerManager;
    public Camera worldCamera;   
    public RawImage mapRawImage;
    public Collider playAreaCollider;
    public LayerMask raycastMask = ~0;

    public float maxSpawnX = 5f;
    public float planeY = 0f; // fallback plane
    public float fallbackRayDistance = 200f;
    
    [Header("Deployment Zone Feedback")]
    [Tooltip("Componente de feedback visual de zonas de despliegue")]
    public DeploymentZoneFeedback zoneFeedback;

    void Awake()
    {
        if (playerManager == null) Debug.LogWarning("PlayableAreaUI: playerManager no asignado.");
        if (worldCamera == null) Debug.Log("[PlayableAreaUI] worldCamera no asignada (usa Camera.main si corresponde).");
        if (mapRawImage == null) Debug.Log("[PlayableAreaUI] mapRawImage no asignado: se usará ScreenPointToRay si es posible.");
        if (worldCamera == null) worldCamera = Camera.main;
        if (zoneFeedback == null) Debug.LogWarning("[PlayableAreaUI] zoneFeedback no asignado - no habrá feedback visual de zonas.");
    }
    
    /// Muestra las zonas de despliegue (llamado desde PlayerCardManager cuando hay cartas seleccionadas)
    public void ShowDeploymentZones()
    {
        if (zoneFeedback != null)
        {
            zoneFeedback.ShowZones();
        }
    }
    
    /// Oculta las zonas de despliegue (llamado cuando no hay cartas listas para desplegar)
    public void HideDeploymentZones()
    {
        if (zoneFeedback != null)
        {
            zoneFeedback.HideZones();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cuando el cursor entra en el área, actualizar feedback
        UpdateHoverFeedback(eventData);
    }
    
    public void OnPointerMove(PointerEventData eventData)
    {
        // Actualizar feedback mientras el cursor se mueve
        UpdateHoverFeedback(eventData);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Cuando el cursor sale, resetear feedback
        if (zoneFeedback != null)
        {
            zoneFeedback.ResetHoverFeedback();
        }
    }
    
    /// Actualiza el feedback de hover según la posición del cursor
    private void UpdateHoverFeedback(PointerEventData eventData)
    {
        if (zoneFeedback == null) return;
        
        Vector2 normalized = GetNormalizedPosition(eventData);
        zoneFeedback.UpdateHoverFeedback(normalized.x);
    }
    
    /// Calcula la posición normalizada (0-1) del cursor dentro del área
    private Vector2 GetNormalizedPosition(PointerEventData eventData)
    {
        if (mapRawImage == null) return new Vector2(0.5f, 0.5f);
        
        RectTransform rt = mapRawImage.rectTransform;
        Vector2 localPoint;

        Canvas canvas = mapRawImage.canvas;
        Camera camForRect = null;
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                camForRect = null;
            else
                camForRect = canvas.worldCamera ?? eventData.pressEventCamera ?? Camera.main;
        }
        else
        {
            camForRect = eventData.pressEventCamera ?? Camera.main;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, camForRect, out localPoint))
        {
            float nx = (localPoint.x + rt.rect.width * rt.pivot.x) / rt.rect.width;
            float ny = (localPoint.y + rt.rect.height * rt.pivot.y) / rt.rect.height;
            return new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));
        }
        
        return new Vector2(0.5f, 0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Verificar si el gameplay está desactivado (ej: durante secuencia de victoria)
        if (GameManager.Instance != null && GameManager.Instance.IsGameplayDisabled)
        {
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        bool hitFound = false;
        Ray usedRay = default;
        Vector2 normalized = new Vector2(0.5f, 0.5f); // fallback

        // ✅ NUEVO: Calcular normalizedX ANTES de procesar el clic
        if (mapRawImage != null && worldCamera != null)
        {
            RectTransform rt = mapRawImage.rectTransform;
            Vector2 localPoint;

            Canvas canvas = mapRawImage.canvas;
            Camera camForRect = null;
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    camForRect = null;
                else
                    camForRect = canvas.worldCamera ?? eventData.pressEventCamera ?? Camera.main;
            }
            else
            {
                camForRect = eventData.pressEventCamera ?? Camera.main;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, camForRect, out localPoint))
            {
                // Convertir localPoint (relativo al pivot) a normalized 0..1
                float nx = (localPoint.x + rt.rect.width * rt.pivot.x) / rt.rect.width;
                float ny = (localPoint.y + rt.rect.height * rt.pivot.y) / rt.rect.height;
                normalized = new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));

                // ✅ VERIFICAR RESTRICCIÓN DEL TUTORIAL ANTES DE CONTINUAR
                if (TutorialManager.Instance != null && TutorialManager.Instance.IsRestrictedToLeftZone())
                {
                    // Si el clic está en la zona DERECHA (>=0.5), bloquear
                    if (normalized.x >= 0.5f)
                    {
                        Debug.LogWarning("[Tutorial] ⛔ Solo puedes desplegar en la zona IZQUIERDA durante este paso");
                        
                        // ✅ Opcional: Mostrar feedback visual de error (shake de la barra, etc.)
                        if (playerManager != null && playerManager.GetComponent<IntelectBar>() != null)
                        {
                            playerManager.GetComponent<IntelectBar>().ShakeBar(0.3f, 5f);
                        }
                        
                        return; // ⛔ BLOQUEAR DESPLIEGUE
                    }
                }

                Ray ray = worldCamera.ViewportPointToRay(new Vector3(normalized.x, normalized.y, 0f));
                usedRay = ray;
                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.cyan, 2f);

                if (Physics.Raycast(ray, out RaycastHit hit, fallbackRayDistance, raycastMask))
                {
                    spawnPos = hit.point;
                    hitFound = true;
                }
            }
            else
            {
                Debug.LogWarning("[PlayableAreaUI] ScreenPointToLocalPointInRectangle falló para el RawImage. (camera para rect = " + (camForRect ? camForRect.name : "null") + ")");
            }
        }

        if (!hitFound)
        {
            Camera cam = eventData.pressEventCamera ?? worldCamera ?? Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(eventData.position);
                usedRay = ray;
                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.yellow, 2f);

                if (Physics.Raycast(ray, out RaycastHit hit2, fallbackRayDistance, raycastMask))
                {
                    spawnPos = hit2.point;
                    hitFound = true;
                }
            }
            else
            {
                Debug.LogWarning("[PlayableAreaUI] No hay cámara disponible para ScreenPointToRay.");
            }
        }

        // 3) Si sigue sin hit -> fallback: ray-plane intersection (plano Y=planeY)
        if (!hitFound)
        {
            Camera cam = eventData.pressEventCamera ?? worldCamera ?? Camera.main;
            Ray rayToUse = (usedRay.direction != Vector3.zero) ? usedRay : (cam != null ? cam.ScreenPointToRay(eventData.position) : new Ray(Vector3.zero, Vector3.forward));
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            if (groundPlane.Raycast(rayToUse, out float enter))
            {
                spawnPos = rayToUse.GetPoint(enter);
                Debug.DrawRay(rayToUse.origin, rayToUse.direction * enter, Color.magenta, 2f);
                hitFound = true;
            }
            else
            {
                Debug.LogError("[PlayableAreaUI] No hubo hit y la intersección con el plano falló; spawnPos queda en Vector3.zero");
            }
        }

        int sideSign = (normalized.x > 0.5f) ? 1 : -1; 
        if (Mathf.Abs(spawnPos.x) > 0.001f)
        {
            spawnPos.x = Mathf.Clamp(sideSign * Mathf.Abs(spawnPos.x), -maxSpawnX, maxSpawnX);
        }
        else
        {
            spawnPos.x = sideSign * maxSpawnX;
        }

        const float rayDownFromAbove = 50f;    
        const float navSampleRadius = 4f;      
        const float spawnYOffset = 0.05f;      


        Ray downRay = new Ray(new Vector3(spawnPos.x, spawnPos.y + rayDownFromAbove * 0.5f, spawnPos.z), Vector3.down);
        if (Physics.Raycast(downRay, out RaycastHit downHit, rayDownFromAbove, raycastMask))
        {
            spawnPos.y = downHit.point.y + spawnYOffset;
        }
        else
        {

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, navSampleRadius, NavMesh.AllAreas))
            {
                spawnPos = navHit.position + Vector3.up * spawnYOffset;
            }
            else
            {
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit2, navSampleRadius * 2f, NavMesh.AllAreas))
                {
                    spawnPos = navHit2.position + Vector3.up * spawnYOffset;
                }
                else
                {
                    spawnPos.y = planeY + spawnYOffset;
                }
            }
        }

        if (playAreaCollider != null) spawnPos = playAreaCollider.ClosestPoint(spawnPos);

        playerManager?.HandlePlayAreaClick(spawnPos);
    }
}