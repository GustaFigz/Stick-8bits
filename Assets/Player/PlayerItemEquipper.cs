using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItemEquipper : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Referência para o ponto da mão do Player onde o item deve grudar")]
    public Transform handTransform; // Transform da mão
    public float pickupRange = 1.5f;

    private ItemPickup nearbyItem;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        DetectNearestItem();
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            var interactAction = playerInput.actions["Player/Interact"];
            if (interactAction != null)
                interactAction.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            var interactAction = playerInput.actions["Player/Interact"];
            if (interactAction != null)
                interactAction.performed -= OnInteract;
        }
    }

    private void DetectNearestItem()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        nearbyItem = null;
        float closest = float.MaxValue;
        foreach (var hit in hits)
        {
            var item = hit.GetComponent<ItemPickup>();
            if (item != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    nearbyItem = item;
                }
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (nearbyItem != null)
        {
            EquipItem(nearbyItem);
        }
    }

    private void EquipItem(ItemPickup item)
    {
        Transform slot = handTransform != null ? handTransform : transform;
        item.transform.SetParent(slot);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        var rb = item.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        Debug.Log($"Item {item.itemName} equipado na mão!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}