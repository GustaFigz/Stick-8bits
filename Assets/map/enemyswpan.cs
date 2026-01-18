using System.Collections;
using UnityEngine;

public class PlaneEnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Wave")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private int enemiesPerWave = 3;

    [Header("Spawn area")]
    [SerializeField] private float xSpread = 2.5f;       // espalha no X à volta do avião
    [SerializeField] private float yOffset = -0.2f;       // nasce ligeiramente abaixo do avião
    [SerializeField] private float yJitter = 0.15f;       // pequena variação para não ficar “linha perfeita”

    [Header("Anti-stack")]
    [SerializeField] private float minSeparation = 0.5f;  // raio mínimo entre inimigos
    [SerializeField] private int maxAttemptsPerEnemy = 8;
    [SerializeField] private LayerMask enemyLayerMask;    // layer dos inimigos (recomendado)

    [Header("Optional")]
    [SerializeField] private Transform spawnOrigin;       // se null, usa este transform

    private Coroutine _loop;

    private void OnEnable()
    {
        if (_loop == null) _loop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWave();
            // WaitForSeconds pausa a coroutine por tempo “scaled” (depende do Time.timeScale). [web:37]
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnWave()
    {
        if (enemyPrefab == null) return;

        Transform origin = spawnOrigin != null ? spawnOrigin : transform;
        Vector2 basePos = origin.position;

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector2 spawnPos = FindFreeSpawnPosition(basePos);

            var go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Empurrãozinho horizontal para não “colarem” ao cair (opcional).
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float push = Random.Range(-0.5f, 0.5f);
                rb.linearVelocity = new Vector2(push, rb.linearVelocity.y);
            }
        }
    }

    private Vector2 FindFreeSpawnPosition(Vector2 basePos)
    {
        Vector2 candidate = basePos + new Vector2(0f, yOffset);

        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            float x = Random.Range(-xSpread, xSpread);
            float y = Random.Range(-yJitter, yJitter);
            candidate = basePos + new Vector2(x, yOffset + y);

            // OverlapCircle verifica se existe um Collider2D dentro de um círculo (usando layer mask se quiseres). [web:31]
            bool occupied = Physics2D.OverlapCircle(candidate, minSeparation, enemyLayerMask) != null;
            if (!occupied) return candidate;
        }

        // fallback: mesmo se estiver ocupado, devolve o último candidato (evita “não spawnar”).
        return candidate;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform origin = spawnOrigin != null ? spawnOrigin : transform;
        Vector3 c = origin.position + Vector3.down * yOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(c + Vector3.left * xSpread, c + Vector3.right * xSpread);

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
        Gizmos.DrawWireSphere(c, minSeparation);
    }
#endif
}
