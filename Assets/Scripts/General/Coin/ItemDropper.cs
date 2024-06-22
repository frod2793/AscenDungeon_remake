using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] private float _GroundOffsetY;

    [Header("Potion Property")]
    [SerializeField, Range(0f, 1f)] private float _CommonPotion = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _RarePotion = 0.05f;

    public bool IsAlreadyCoinDrop   { get; private set; }
    public bool IsAlreadyPotionDrop { get; private set; }

    private void OnEnable()
    {
        IsAlreadyCoinDrop   = false;
        IsAlreadyPotionDrop = false;
    }
    public void CoinDrop(int count = 1)
    {
        if (!IsAlreadyCoinDrop)
        {
            Vector3 offset = Vector3.up * _GroundOffsetY;

            for (int i = 0; i < count; i++)
            {
                Coin coin = CoinPool.Instance.Get();

                coin.Value = 1;

                coin.transform.position = transform.position + offset;
                coin.Enable();
            }
            IsAlreadyCoinDrop = true;
        }
    }
    public void TryPotionDrop(PotionName common, PotionName rare)
    {
        if (!IsAlreadyPotionDrop)
        {
            Vector3 offset = new Vector3(0, _GroundOffsetY + 0.4f, 0);

            float random = Random.value;

            if (random <= _RarePotion)
            {
                PotionPool.Instance.Get(rare).transform.position = transform.position + offset;
            }
            else if (random <= _CommonPotion)
            {
                PotionPool.Instance.Get(common).transform.position = transform.position + offset;
            }
            IsAlreadyPotionDrop = true;
        }
    }
}
