using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    [SerializeField] 
    private string TargetTag;

    private float AttackPower;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(TargetTag))
        {
            if (collision.TryGetComponent(out ICombatable combat))
                combat.Damaged(AttackPower, gameObject);
        }
    }

    private void Thundering() {
        MainCamera.Instance.Shake(0.3f, 1f);
    }

    private void Disabe() {
        gameObject.SetActive(false);
    }

    public void SetAttackPower(float damage) {
        AttackPower = damage;
    }
}
