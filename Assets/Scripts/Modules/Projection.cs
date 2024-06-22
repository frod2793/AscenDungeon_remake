using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projection : MonoBehaviour
{
    public Action<GameObject> HitAction;
    public Action<Projection> LifeOverAction;

    [Header("Target Info")]
    [SerializeField] private string _TargetTag;

    [Header("Projection Info")]
    [SerializeField] private float _LifeTime;

    [SerializeField] private int _MaxHitCount;
                     private int _CurHitCount;
    
    private IEnumerator _EShoot;

    public void SetAction(Action<GameObject> hitAction, Action<Projection> lifeOverAction)
    {
        HitAction = hitAction;

        LifeOverAction = lifeOverAction;
    }

    public void Shoot(Vector2 position, Vector2 dir, float speed)
    {
        _CurHitCount = 0;

        transform.position = position;

        if (_EShoot != null)
        {
            StopCoroutine(_EShoot);
            
            _EShoot = null;
        }
        StartCoroutine(_EShoot = EShoot(dir, speed));
    }

    private float DeltaTime()
        => Time.deltaTime * Time.timeScale;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(_TargetTag))
        {
            _CurHitCount++;
            HitAction?.Invoke(collision.gameObject);
        }
    }

    private IEnumerator EShoot(Vector3 dir, float speed)
    {
        for (float i = 0; i < _LifeTime; i += DeltaTime())
        {
            if (_CurHitCount >= _MaxHitCount)
            {
                break;
            }
            transform.position += dir * speed * DeltaTime();

            yield return null;
        }
        LifeOverAction?.Invoke(this);

        _EShoot = null;
    }
}
