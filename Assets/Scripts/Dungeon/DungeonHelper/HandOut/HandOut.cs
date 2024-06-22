using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandOut : MonoBehaviour
{
    public void Throw()
    {
        transform.DOMove(transform.position + Vector3.up, 0.5f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
    }
}
