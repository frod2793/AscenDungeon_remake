using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kimbokchi;
using Assets.Scripts.Ad;

public class ItemPackmanNPC : NPC
{
    [SerializeField] private DropItem DropItem;
    [SerializeField] private Transform DropItemHolder;

    [Header("ItemDrop Curve")]
    [SerializeField] private float _CurveSpeed;[Space()]
    [SerializeField] private Vector2[] _Points;

    public override void Interaction()
    {
        SystemMessage.Instance.ShowCheckMessage("광고를 시청하고\n아이템을 획득하시겠습니까?", result =>
        {
            if (result)
            {
                // [안전 장치]: 광고 서비스 미초기화 시 광고 생략
                if (AdsManager.Service != null)
                {
                    AdsManager.Service.ShowRewardedAd(() =>
                    {
                        var drop = Instantiate(DropItemHolder.gameObject, transform);

                        if (drop.transform.GetChild(0).TryGetComponent(out DropItem dropItem))
                        {
                            dropItem.Init(ItemLibrary.Instance.GetRandomItem());
                            dropItem.gameObject.SetActive(true);

                            StartCoroutine(ItemDropRoutine(drop.transform));
                        }
                        EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position);
                    });
                }
                else
                {
                    Debug.LogWarning("[ItemPackmanNPC] 광고 서비스가 준비되지 않았습니다.");
                }
            }
            SystemMessage.Instance.CloseMessage();
        });
    }
    private IEnumerator ItemDropRoutine(Transform holder)
    {
        Vector2 offset = Vector2.right * Random.value;

        Vector2 pointB = _Points[1] + offset;
        Vector2 pointC = _Points[2] + offset;
        Vector2 pointD = _Points[3] + offset;

        for (float ratio = 0f; ratio < 1; ratio += Time.deltaTime * Time.timeScale * _CurveSpeed)
        {
            ratio = ratio > 1 ? 1 : ratio;

            holder.localPosition = Utility.BezierCurve3(_Points[0], pointB, pointC, pointD, ratio);
            yield return null;
        }
        yield return null;
    }
}
