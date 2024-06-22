using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public interface IPurchasable
{
    void PurchaseSuccess();
    void PurchaseFaild();
    void TryPurchase();
}
public class NoADPurchaser : MonoBehaviour, IPurchasable
{
    [SerializeField] private Button _Button;
    [SerializeField] private ItemStore _ItemStore;

    public void OnClick()
    {
        _ItemStore.PurchaserSelect(this);
    }
    public void PurchaseSuccess()
    {
        IAP.Instance.Reward();
    }
    public void PurchaseFaild()
    {
        IAP.Instance.Faild();
    }
    public void TryPurchase()
    {
        _Button.onClick.Invoke();
    }
}
