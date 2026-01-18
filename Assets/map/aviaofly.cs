using UnityEngine;

public class PlanePatrolX : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float xMin = -10f;
    [SerializeField] private float xMax = 10f;

    [Header("Flip")]
    [SerializeField] private SpriteRenderer spriteRenderer; // opcional
    [SerializeField] private bool flipOnlySprite = true;

    private int dir = 1; // 1 = direita, -1 = esquerda

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Move no eixo X (mundo)
        Vector3 pos = transform.position;
        pos.x += dir * speed * Time.deltaTime;
        transform.position = pos;

        // Bateu no limite? troca direção e faz flip
        if (pos.x >= xMax)
        {
            dir = -1;
            ApplyFlip();
        }
        else if (pos.x <= xMin)
        {
            dir = 1;
            ApplyFlip();
        }
    }

    private void ApplyFlip()
    {
        if (flipOnlySprite && spriteRenderer != null)
        {
            // Inverte só o visual do sprite
            spriteRenderer.flipX = (dir < 0);
        }
        else
        {
            // Inverte a escala (afeta tudo: sprite, colliders, etc.)
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir > 0 ? 1f : -1f);
            transform.localScale = s;
        }
    }
}