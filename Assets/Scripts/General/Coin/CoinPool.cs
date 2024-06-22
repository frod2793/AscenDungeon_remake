using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPool : Singleton<CoinPool>
{
    [SerializeField] private Coin _Origin;
    [SerializeField] private  int _HoldingCount;

    private Pool<Coin> _Pool;

    private void Awake()
    {
        _Pool = new Pool<Coin>();
        _Pool.Init(_HoldingCount, _Origin, coin => coin.Init());
    }

    public Coin Get()
    {
        return _Pool.Get();
    }
    public void Add(Coin coin)
    {
        _Pool.Add(coin);
    }
}
