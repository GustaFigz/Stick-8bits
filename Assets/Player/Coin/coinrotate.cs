using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class coinrotate : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Fall Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float fallGravityScale = 2f;
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceSpeed = 5f;
    [SerializeField] private float bounceDuration = 0.5f;

    [Header("Bobbing After Landing")]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float bobbingAmount = 0.15f;

    [Header("Collection")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    private Vector3 _groundPosition;
    private float _bobbingTime;
    private Rigidbody2D _rb;
    private bool _hasLanded;
    private bool _isBouncing;
    private float _bounceTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = fallGravityScale;

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        // Sempre rotaciona
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);

        // Comportamento após pousar
        if (_hasLanded)
        {
            if (_isBouncing)
            {
                // Efeito de bounce
                _bounceTimer += Time.deltaTime * bounceSpeed;
                float bounce = Mathf.Sin(_bounceTimer * Mathf.PI) * bounceHeight;
                transform.position = new Vector3(_groundPosition.x, _groundPosition.y + bounce, _groundPosition.z);

                if (_bounceTimer >= bounceDuration)
                {
                    _isBouncing = false;
                    _groundPosition = transform.position;
                }
            }
            else if (enableBobbing)
            {
                // Efeito de bobbing suave
                _bobbingTime += Time.deltaTime * bobbingSpeed;
                float newY = _groundPosition.y + Mathf.Sin(_bobbingTime) * bobbingAmount;
                transform.position = new Vector3(_groundPosition.x, newY, _groundPosition.z);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detecta chão
        if (!_hasLanded && IsInLayerMask(other.gameObject.layer, groundLayer))
        {
            Land();
        }

        // Detecta player
        if (other.CompareTag("Player"))
        {
            var coinCollector = other.GetComponent<PlayerCoinCollector>();
            if (coinCollector != null)
            {
                coinCollector.AddCoins(coinValue);
                CollectCoin();
            }
        }
    }

    private void Land()
    {
        _hasLanded = true;
        _isBouncing = true;
        _bounceTimer = 0f;
        _groundPosition = transform.position;

        // Para a física
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;
    }

    private void CollectCoin()
    {
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask) != 0;
    }
}
