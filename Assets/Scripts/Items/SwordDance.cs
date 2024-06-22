using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDance : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private float Damage;
    [SerializeField] private float LifeTime;
    [SerializeField] private float Speed;

    [Header("Traget")]
    [SerializeField] private string TargetTag;

    private float DeltaTime {
        get => Time.deltaTime * Time.timeScale;
    }

    public void Launch(Vector2 direction)
    {
        gameObject.SetActive(true);

        StartCoroutine(EUpdate(direction));
    }

    private IEnumerator EUpdate(Vector3 direction)
    {
        for (float i = 0f; i < LifeTime; i += DeltaTime)
        {
            transform.position += direction * Speed;

            yield return null;
        }
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(TargetTag)) {
            if (collision.TryGetComponent(out ICombatable combatable))
            {
                combatable.Damaged(Damage, gameObject);
            }
        }
    }
}
