using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FractureObject : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab con los fragmentos fracturados (con Rigidbodies)")]
    public GameObject fracturedObject;
    
    [Tooltip("Efecto visual de explosión")]
    public GameObject explosionVFX;

    [Header("Explosion Settings")]
    public float explosionMinForce = 5;
    public float explosionMaxForce = 100;
    public float explosionForceRadius = 10;
    public float fragScaleFactor = 1;

    private GameObject fractObj;

    public void Explode()
    {
        // Destruir todos los hijos (meshes originales)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Instanciar el objeto fracturado en la posición actual
        if (fracturedObject != null)
        {
            fractObj = Instantiate(fracturedObject, transform.position, transform.rotation);

            // Aplicar fuerza de explosión a cada fragmento
            foreach (Transform t in fractObj.transform)
            {
                var rb = t.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(Random.Range(explosionMinForce, explosionMaxForce),
                                         transform.position,
                                         explosionForceRadius);
                    StartCoroutine(Shrink(t, 2));
                }
            }

            Destroy(fractObj, 5);
        }

        // Instanciar efecto visual de explosión
        if (explosionVFX != null)
        {
            GameObject explovFX = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(explovFX, 7);
        }
    }

    IEnumerator Shrink(Transform t, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 newScale = t.localScale;

        while (newScale.x >= 0)
        {
            newScale -= new Vector3(fragScaleFactor, fragScaleFactor, fragScaleFactor);
            t.localScale = newScale;
            yield return new WaitForSeconds(0.05f);
        }
    }

}
