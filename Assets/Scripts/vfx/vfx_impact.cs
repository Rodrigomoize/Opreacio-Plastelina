using UnityEngine;
using System.Collections;

public class PlastilinaImpact : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private GameObject splashMesh;
    //[SerializeField] private GameObject splatDecal;

    void Start()
    {
        // Reproduce partículas
        foreach (var ps in particleSystems)
            ps.Play();

        // Escala suave del mesh
        StartCoroutine(AnimateSplash());

        // Destruir después de 2 segundos
        Destroy(gameObject, 2f);
    }

    private IEnumerator AnimateSplash()
    {
        Vector3 targetScale = Vector3.one;
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            splashMesh.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t / 0.2f);
            yield return null;
        }

        // Fade out rápido
        yield return new WaitForSeconds(0.5f);
        float fade = 1;
        var mat = splashMesh.GetComponentInChildren<Renderer>().material;
        while (fade > 0)
        {
            fade -= Time.deltaTime * 2;
            var col = mat.color;
            col.a = fade;
            mat.color = col;
            yield return null;
        }
    }
}
