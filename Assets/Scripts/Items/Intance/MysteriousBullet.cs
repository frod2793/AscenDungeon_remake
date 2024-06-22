using System.Collections;
using UnityEngine;
using Kimbokchi;

public class MysteriousBullet : MonoBehaviour
{
    public System.Action<MysteriousBullet> DisableAction;

    [SerializeField] private float LifeTime;

    private Transform TargetTransform;
    private IEnumerator ETarceTarget;

    private System.Action<GameObject> TargetHitAction;

    public bool Shoot(Transform target, System.Action<GameObject> hitAction)
    {
        bool canShoot = ETarceTarget == null;

        if (canShoot)
        {
            TargetTransform = target;
            TargetHitAction = hitAction;

            StartCoroutine(ETarceTarget = TarceTarget());
        }
        return canShoot;
    }

    private IEnumerator TarceTarget()
    {
        Vector2 RandomVector(float minX, float maxX, float minY, float maxY)
        {
            return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
        Vector2 pointA = transform.position;
        Vector2 pointB = Castle.Instance.PointToFloorPoint(RandomVector(-4, 4, -7, 7));
        Vector2 pointC = Castle.Instance.PointToFloorPoint(RandomVector(-4, 4, -7, 7));

        if (TargetTransform.position.y < transform.position.y)
        {
            pointB += new Vector2(2, -3);
        }
        else
            pointB += new Vector2(2, +3);

        float DeltaTime()
        {
            return Time.deltaTime * Time.timeScale;
        }
        for (float i = 0f; i < LifeTime; i += DeltaTime())
        {
            if (!TargetTransform.gameObject.activeSelf)
            {
                break;
            }
            var targetPoint = TargetTransform.position;

            transform.BezierCurve3
                (pointA, pointB, pointC, targetPoint, i / LifeTime);

            yield return null;
        }
        ETarceTarget = null;
        
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.Equals(TargetTransform))
        {
            TargetHitAction.Invoke(collision.gameObject);

            if (ETarceTarget != null)
            {
                StopCoroutine(ETarceTarget);

                ETarceTarget = null;
            }
            SoundManager.Instance.PlaySound(SoundName.MysteriousBulletHit);
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        DisableAction?.Invoke(this);
    }
}
