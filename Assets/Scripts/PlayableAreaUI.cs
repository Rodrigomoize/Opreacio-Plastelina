using UnityEngine;
using UnityEngine.EventSystems;

public class PlayableAreaUI : MonoBehaviour, IPointerClickHandler
{
    public PlayerCardManager playerManager;

    void Awake()
    {
        if (playerManager == null) Debug.LogWarning("PlayableAreaUI: playerManager no asignado.");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 spawnPos = Vector3.zero;
        if (playerManager != null && playerManager.spawnPoint != null) spawnPos = playerManager.spawnPoint.position;
        playerManager?.HandlePlayAreaClick(spawnPos);
    }
}
