using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTutorial : TutorialBase
{
    [SerializeField] private HitBoxObject _HitBoxObject;

    private void Reset()
    {
        _HitBoxObject = FindObjectOfType<HitBoxObject>();
    }

    public override void StartTutorial()
    {
        base.StartTutorial();
        
        if (!_HitBoxObject.gameObject.activeSelf) 
        {
            _HitBoxObject.gameObject.SetActive(true);
        }
        _HitBoxObject.HitEvent += HitEventOfHitBoxObject;
    }

    private void HitEventOfHitBoxObject(GameObject attacker)
    {
        EndTutorial();
        
        _HitBoxObject.Break();
        _HitBoxObject.HitEvent -= HitEventOfHitBoxObject;
    }
}
