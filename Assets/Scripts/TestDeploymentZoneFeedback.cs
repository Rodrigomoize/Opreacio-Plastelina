using UnityEngine;

/// <summary>
/// Script de TEST para verificar que el parpadeo de DeploymentZoneFeedback funciona.
/// Añadir a un objeto con DeploymentZoneFeedback y presionar Z para mostrar/ocultar zonas.
/// </summary>
public class TestDeploymentZoneFeedback : MonoBehaviour
{
    private DeploymentZoneFeedback zoneFeedback;
    private bool isShowing = false;

    void Start()
    {
        zoneFeedback = GetComponent<DeploymentZoneFeedback>();
        if (zoneFeedback == null)
        {
            Debug.LogError("[TestDeploymentZoneFeedback] No se encontró DeploymentZoneFeedback en este objeto!");
        }
        else
        {
            Debug.LogWarning("[TestDeploymentZoneFeedback] Presiona Z para mostrar/ocultar zonas con parpadeo");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!isShowing)
            {
                Debug.LogWarning("[TestDeploymentZoneFeedback] Z presionado - mostrando zonas");
                zoneFeedback.ShowZones();
                isShowing = true;
            }
            else
            {
                Debug.LogWarning("[TestDeploymentZoneFeedback] Z presionado - ocultando zonas");
                zoneFeedback.HideZones();
                isShowing = false;
            }
        }
    }
}
