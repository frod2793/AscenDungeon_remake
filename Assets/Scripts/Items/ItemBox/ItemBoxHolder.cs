using UnityEngine;

public class ItemBoxHolder : MonoBehaviour
{
    [SerializeField] private ItemBox ItemBox;
                      public ItemBox HoldingItemBox => ItemBox;
}
