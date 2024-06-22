using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kimbokchi;

public class ItemPackmanNPC : MonoBehaviour
{
    [SerializeField] private AttackButtonHider _Hider;

    [SerializeField] private DropItem DropItem;
    [SerializeField] private Transform DropItemHolder;

    [Header("ItemDrop Curve")]
    [SerializeField] private float CurveSpeed; [Space()]
    [SerializeField] private Vector2 PointA;
    [SerializeField] private Vector2 PointB;
    [SerializeField] private Vector2 PointC;
    [SerializeField] private Vector2 PointD;

    private bool mHasPlayer;
    private bool mIsGivingItem;

    public void Interaction()
    {
        if (!mHasPlayer)
        {
            SystemMessage.Instance.ShowMessage("NPC와의 거리가\n너무 멉니다!");
        }
        else
        {
            SystemMessage.Instance.ShowCheckMessage("광고를 시청하고\n아이템을 획득하시겠습니까?", result => 
            {
                if (result)
                {
                    Ads.Instance.ShowRewardAd();
                    Ads.Instance.UserEarnedRewardEvent(() =>
                    {
                        var drop = Instantiate(DropItemHolder.gameObject, transform);

                        if (drop.transform.GetChild(0).TryGetComponent(out DropItem dropItem))
                        {
                            dropItem.Init(ItemLibrary.Instance.GetRandomItem());
                            dropItem.gameObject.SetActive(true);

                            StartCoroutine(ItemDrop(drop.transform));
                        }
                        EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position);
                    });
                }
                SystemMessage.Instance.CloseMessage();
            });
        }
    }

    private IEnumerator ItemDrop(Transform holder)
    {
        Vector2 offset = Vector2.right * Random.value;

        Vector2 pointB = PointB + offset;
        Vector2 pointC = PointC + offset;
        Vector2 pointD = PointD + offset;

        for (float ratio = 0f; ratio < 1; ratio += Time.deltaTime * CurveSpeed)
        {
            ratio = ratio > 1 ? 1 : ratio;

            holder.localPosition = Utility.BezierCurve3(PointA, pointB, pointC, pointD, ratio);
            yield return null;
        }
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // _Hider.HideOrShow();
            mHasPlayer = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // _Hider.HideOrShow();
            mHasPlayer = false;
        }
    }
}
