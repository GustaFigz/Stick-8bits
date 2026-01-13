using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HandFollower2D : MonoBehaviour
{
    [Header("Referências")]
    public SpriteRenderer playerSpriteRenderer; // SpriteRenderer do Player
    public Transform handTransform; // Objeto da mão a seguir (ex: Hand)

    [Header("Offsets por frame")] 
    [Tooltip("Offsets de posição para cada frame da animação baseado no nome do sprite.")]
    public SpriteOffset[] spriteOffsets;

    [System.Serializable]
    public struct SpriteOffset
    {
        public string spriteName;
        public Vector2 offset;
    }

    private void LateUpdate()
    {
        if (playerSpriteRenderer == null || handTransform == null)
            return;

        Sprite currentSprite = playerSpriteRenderer.sprite;
        if (currentSprite == null)
            return;

        foreach (var s in spriteOffsets)
        {
            if (s.spriteName == currentSprite.name)
            {
                Vector3 worldOffset = new Vector3(s.offset.x, s.offset.y, 0f);
                handTransform.localPosition = worldOffset;
                return;
            }
        }
    }
}