using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeTutorial : TutorialBase
{
    [SerializeField] private HitBoxObject[] _HitBoxObjects;

    private LinkedList<HitBoxObject> _OnHitBoxObjects = new LinkedList<HitBoxObject>();

    public override void StartTutorial()
    {
        base.StartTutorial();

        for (int i = 0; i < _HitBoxObjects.Length; ++i)
        {
            if (!_HitBoxObjects[i].gameObject.activeSelf) 
            {
                _HitBoxObjects[i].gameObject.SetActive(true);
            }
        }
        Finger.Instance.Gauge.OnChargeEvent += OnChargeEvent;
        Inventory.Instance.ProjectionHitEvent += OnProjectionHitEvent;
    }

    public override void EndTutorial()
    {
        base.EndTutorial();

        for (int i = 0; i < _HitBoxObjects.Length; ++i) 
        {
            _HitBoxObjects[i].Break();
        }
        Finger.Instance.Gauge.OnChargeEvent -= OnChargeEvent;
        Inventory.Instance.ProjectionHitEvent -= OnProjectionHitEvent;
    }

    private void OnChargeEvent()
    {
        _OnHitBoxObjects.Clear();
        for (int i = 0; i < _HitBoxObjects.Length; ++i) 
        {
            _OnHitBoxObjects.AddLast(_HitBoxObjects[i]);
        }
    }

    private void OnProjectionHitEvent(GameObject v, float dmg)
    {
        foreach (var hitBox in _OnHitBoxObjects)
        {
            if (v.Equals(hitBox.gameObject))
            {
                _OnHitBoxObjects.Remove(hitBox);
                break;
            }
        }
        if (_OnHitBoxObjects.Count == 0)
        {
            EndTutorial();
        }
    }
}
