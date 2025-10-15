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

        // 1) Si hay RawImage + worldCamera -> convertir screen->local dentro del RawImage y normalizar correctamente
        if (mapRawImage != null && worldCamera != null)
        {
            RectTransform rt = mapRawImage.rectTransform;
            Vector2 localPoint;

            // IMPORTANTE: seleccionar la c�mara correcta para ScreenPointToLocalPointInRectangle:
            // - si el Canvas es Screen Space - Overlay -> usar null
            // - si es Screen Space - Camera -> usar canvas.worldCamera
            // - si es World Space -> usar canvas.worldCamera (o eventData.pressEventCamera)
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

                // Ray desde worldCamera usando viewport (normalized)
                Ray ray = worldCamera.ViewportPointToRay(new Vector3(normalized.x, normalized.y, 0f));
                usedRay = ray;
                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.cyan, 2f);

                if (Physics.Raycast(ray, out RaycastHit hit, fallbackRayDistance, raycastMask))
                {
                    spawnPos = hit.point;
                    hitFound = true;
                    Debug.Log($"[PlayableAreaUI] Hit via RawImage at world pos {spawnPos} (normalized {normalized}). Collider: {hit.collider.name}");
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

        // 2) Fallback - ScreenPointToRay con la c�mara del evento o main
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
                    Debug.Log($"[PlayableAreaUI] Hit via ScreenPointToRay at {spawnPos} using camera {cam.name}. Collider: {hit2.collider.name}");
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
                Debug.Log($"[PlayableAreaUI] Ning�n hit en Physics; usando intersecci�n con plano Y={planeY} -> {spawnPos}");
                hitFound = true;
            }
            else
            {
                Debug.LogError("[PlayableAreaUI] No hubo hit y la intersecci�n con el plano fall�; spawnPos queda en Vector3.zero");
            }
        }

        // 4) Forzar lado seg�n normalized.x � pero preservando magnitud y aplicando clamp:
        int sideSign = (normalized.x > 0.5f) ? 1 : -1; // >0.5 derecha, <=0.5 izquierda
        if (Mathf.Abs(spawnPos.x) > 0.001f)
        {
            // preservamos magnitud y flip si es necesario
            spawnPos.x = Mathf.Clamp(sideSign * Mathf.Abs(spawnPos.x), -maxSpawnX, maxSpawnX);
        }
        else
        {
            // si no tenemos magnitud (ej: spawnPos.x == 0 por fallback raro), colocamos en una X razonable dentro de l�mites
            spawnPos.x = sideSign * maxSpawnX;
        }

        const float rayDownFromAbove = 50f;    // altura desde la que raycastamos hacia abajo (tweak)
        const float navSampleRadius = 4f;      // radio para buscar NavMesh cercano (tweak)
        const float spawnYOffset = 0.05f;      // elevar ligeramente para evitar interpenetraci�n

        // 1) Raycast vertical hacia abajo desde una altura razonable por encima de spawnPos
        Ray downRay = new Ray(new Vector3(spawnPos.x, spawnPos.y + rayDownFromAbove * 0.5f, spawnPos.z), Vector3.down);
        if (Physics.Raycast(downRay, out RaycastHit downHit, rayDownFromAbove, raycastMask))
        {
            spawnPos.y = downHit.point.y + spawnYOffset;
            Debug.Log($"[PlayableAreaUI] Ground hit under spawn -> y={spawnPos.y} (collider: {downHit.collider.name})");
        }
        else
        {
            // 2) Si no hay collider por debajo (p. ej. spawn muy alto o terreno no en raycastMask),
            //    intentamos samplear el NavMesh cercano.
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, navSampleRadius, NavMesh.AllAreas))
            {
                spawnPos = navHit.position + Vector3.up * spawnYOffset;
                Debug.Log($"[PlayableAreaUI] NavMesh.SamplePosition ajust� spawn a {spawnPos}");
            }
            else
            {
                // 3) �ltimo recurso: intentar samplear en un rango m�s grande o usar planeY (tu fallback)
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit2, navSampleRadius * 2f, NavMesh.AllAreas))
                {
                    spawnPos = navHit2.position + Vector3.up * spawnYOffset;
                    Debug.LogWarning($"[PlayableAreaUI] NavMesh.SamplePosition (rango2) ajust� spawn a {spawnPos}");
                }
                else
                {
                    Debug.LogWarning("[PlayableAreaUI] No se encontr� suelo ni NavMesh cerca; spawnPos puede quedarse en plano. Revisa colliders / NavMesh.");
                    // opcional: forzamos Y = planeY si quieres evitar alt�simos
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
