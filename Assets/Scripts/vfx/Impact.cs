using UnityEngine;
using System.Collections;

public class TropaImpactHandler : MonoBehaviour
{
    [SerializeField] private string enemyTag = "Tropa"; // Tag de las tropas enemigas
    [SerializeField] private GameObject vfxImpactPrefab; // Prefab del VFX
    [SerializeField] private float implosionSpeed = 3f;  // Velocidad de acercamiento
    [SerializeField] private float shrinkSpeed = 6f;     // Velocidad de reducción de escala

    private bool isImploding = false;
    private Transform targetEnemy;

    private void OnTriggerEnter(Collider other)
    {
        if (isImploding) return;

        if (other.CompareTag(enemyTag))
        {
            // Empieza la implosión entre las dos tropas
            isImploding = true;
            targetEnemy = other.transform;

            // Desactivar movimiento o IA momentáneamente
            var rb = GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            StartCoroutine(ImplodeAndExplode());
        }
    }

    private IEnumerator ImplodeAndExplode()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetEnemy.localPosition;

        float t = 0;
        Vector3 originalScale = transform.localScale;
        Vector3 enemyOriginalScale = targetEnemy ? targetEnemy.localScale : Vector3.one;

        // Estiramiento inicial
        transform.localScale *= 1.2f;
        if (targetEnemy) targetEnemy.localScale *= 1.2f;
        yield return new WaitForSeconds(0.05f);

        // Implosión (ambas se atraen y encogen)
        while (t < 1)
        {
            t += Time.deltaTime * implosionSpeed;

            if (targetEnemy)
            {
                // Ambas se acercan al punto medio
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                //targetEnemy.position = Vector3.Lerp(targetPos, startPos, t);

                // Ambas se reducen de tamaño suavemente
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t * 1.2f);
                targetEnemy.localScale = Vector3.Lerp(enemyOriginalScale, Vector3.zero, t * 1.2f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            }

            yield return null;
        }

        // Instancia el VFX en el punto medio
        Vector3 vfxPos = (transform.position + (targetEnemy ? targetEnemy.position : transform.position)) / 2f;
        Instantiate(vfxImpactPrefab, vfxPos, Quaternion.identity);

        // Destruye ambas tropas
        if (targetEnemy)
            Destroy(targetEnemy.gameObject);

        Destroy(gameObject);
    }
}
