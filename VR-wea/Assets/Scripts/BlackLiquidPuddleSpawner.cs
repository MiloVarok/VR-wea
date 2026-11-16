using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BlackLiquidPuddleSpawner : MonoBehaviour
{
    [Tooltip("Material Mat_BlackLiquid")]
    public Material liquidMaterial;
    public float puddleSize = 0.068f;
    public float puddleDuration = 5.5f;
    [Tooltip("Intervalo mínimo entre manchas")]
    public float spawnCooldown = 0.08f;
    [Tooltip("Máximo de charcos simultáneos")]
    public int maxActivePuddles = 25;

    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private Queue<GameObject> activePuddles = new Queue<GameObject>();
    private float lastSpawnTime = 0f;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        // Ignora colisiones contra la propia corona
        if (transform.parent != null && other.transform.IsChildOf(transform.parent))
            return;

        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        int count = ps.GetCollisionEvents(other, collisionEvents);
        if (count == 0) return;

        lastSpawnTime = Time.time;
        ParticleCollisionEvent hit = collisionEvents[0];

        SpawnPuddle(hit.intersection + hit.normal * 0.0012f, hit.normal);
    }

    private void SpawnPuddle(Vector3 position, Vector3 normal)
    {
        if (activePuddles.Count >= maxActivePuddles)
        {
            GameObject oldest = activePuddles.Dequeue();
            if (oldest != null) Destroy(oldest);
        }

        GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(puddle.GetComponent<Collider>());

        puddle.transform.position = position;
        puddle.transform.up = normal;
        puddle.transform.localScale = new Vector3(0.008f, 0.0006f, 0.008f);

        MeshRenderer renderer = puddle.GetComponent<MeshRenderer>();
        if (renderer != null && liquidMaterial != null)
        {
            renderer.sharedMaterial = liquidMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        activePuddles.Enqueue(puddle);
        StartCoroutine(AnimatePuddle(puddle));
    }

    private IEnumerator AnimatePuddle(GameObject puddle)
    {
        float targetScale = puddleSize * Random.Range(0.85f, 1.45f);
        float elapsed = 0f;

       
        while (elapsed < 0.35f && puddle != null)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(0.008f, targetScale, elapsed / 0.35f);
            puddle.transform.localScale = new Vector3(s, 0.0006f, s);
            yield return null;
        }

        yield return new WaitForSeconds(puddleDuration);

        // Absorción gradual
        elapsed = 0f;
        while (elapsed < 1.2f && puddle != null)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(targetScale, 0f, elapsed / 1.2f);
            puddle.transform.localScale = new Vector3(s, 0.0006f, s);
            yield return null;
        }

        if (puddle != null) Destroy(puddle);
    }
}