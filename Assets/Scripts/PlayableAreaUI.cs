using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayableAreaUI : MonoBehaviour, IPointerClickHandler
{
    public PlayerCardManager playerManager;
    public Camera worldCamera;   
    public RawImage mapRawImage;
    public Collider playAreaCollider;
    public LayerMask raycastMask = ~0;

    public float maxSpawnX = 5f;
    public float planeY = 0f; // fallback plane
    public float fallbackRayDistance = 200f;

    void Awake()
    {
        if (playerManager == null) Debug.LogWarning("PlayableAreaUI: playerManager no asignado.");
        if (worldCamera == null) Debug.Log("[PlayableAreaUI] worldCamera no asignada (usa Camera.main si corresponde).");
        if (mapRawImage == null) Debug.Log("[PlayableAreaUI] mapRawImage no asignado: se usar� ScreenPointToRay si es posible.");
        if (worldCamera == null) worldCamera = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 spawnPos = Vector3.zero;
        bool hitFound = false;
        Ray usedRay = default;
        Vector2 normalized = new Vector2(0.5f, 0.5f); // fallback

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

                Ray ray = worldCamera.ViewportPointToRay(new Vector3(normalized.x, normalized.y, 0f));
                usedRay = ray;
                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.cyan, 2f);

                if (Physics.Raycast(ray, out RaycastHit hit, fallbackRayDistance, raycastMask))
                {
                    spawnPos = hit.point;
                    hitFound = true;
                }
                else
                {
                    Debug.Log($"[PlayableAreaUI] No hit con ViewportRay (normalized={normalized}). Comprueba colliders y cullingMask de worldCamera.");
                }
            }
            else
            {
                Debug.LogWarning("[PlayableAreaUI] ScreenPointToLocalPointInRectangle fall� para el RawImage. (camera para rect = " + (camForRect ? camForRect.name : "null") + ")");
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
                else
                {
                    Debug.Log("[PlayableAreaUI] ScreenPointToRay no golpe� nada. Intentando fallback plano.");
                }
            }
            else
            {
                Debug.LogWarning("[PlayableAreaUI] No hay c�mara disponible para ScreenPointToRay.");
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
                Debug.LogError("[PlayableAreaUI] No hubo hit y la intersecci�n con el plano fall�; spawnPos queda en Vector3.zero");
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

        Debug.Log($"[PlayableAreaUI] Final spawnPos usado: {spawnPos}  (normalized.x={normalized.x})");

        // 5) Llamar al manager con la posici�n calculada
        playerManager?.HandlePlayAreaClick(spawnPos);
    }
}