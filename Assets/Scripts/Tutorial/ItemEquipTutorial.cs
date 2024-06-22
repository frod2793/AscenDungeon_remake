using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kimbokchi;

public class ItemEquipTutorial : TutorialBase
{
    [Header("ItemDrop")]
    [SerializeField] private DropItem   _DropItem;
    [SerializeField] private Transform  _DropItemHolder;

    [Header("ItemDrop Curve")]
    [SerializeField] private float      _DropCurveTime;
    [SerializeField] private Vector3[]  _DropCurvePoints;

    private IEnumerator                 _WaitRoutine = null;

    public override void StartTutorial()
    {
        base.StartTutorial();
        gameObject.SetActive(true);
        
        _DropItem.Init(ItemLibrary.Instance.GetItemObject(ItemID.LongSword));
        Inventory.Instance.EnterItemSlotEvent += (item, slotType) => 
        {
            _DropItemHolder.gameObject.SetActive(false);
            
            if (slotType == SlotType.Weapon)
            {
                if (_WaitRoutine == null) {
                    _WaitRoutine = WaitForCloseInventory();
                    StartCoroutine(_WaitRoutine);
                }
            }
        };
        StartCoroutine(ItemDropRoutine());
    }

    private IEnumerator WaitForCloseInventory()
    {
        while (Inventory.Instance.InventoryWindow.activeSelf) {
            yield return null;
        }
        EndTutorial();
    }

    private IEnumerator ItemDropRoutine()
    {
        for (float i = 0f; i < _DropCurveTime; i += Time.deltaTime * Time.timeScale)
        {
            float ratio = Mathf.Min(i / _DropCurveTime, 1f);

            var position = Utility.BezierCurve3(_DropCurvePoints[0], 
                                                _DropCurvePoints[1],
                                                _DropCurvePoints[2],
                                                _DropCurvePoints[3], ratio);
            
            _DropItemHolder.localPosition = position;
            yield return null;
        } 
    }
}
