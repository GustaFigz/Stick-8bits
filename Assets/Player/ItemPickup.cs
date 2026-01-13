using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Tooltip("Nome do item a ser mostrado ou logado na coleta")]
    public string itemName = "Novo Item";

    [Tooltip("Ponto de fixação no item para quando o player equipar")]
    public Transform attachPoint;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}