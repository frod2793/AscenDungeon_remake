using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorAnimator : MonoBehaviour
{
    private LinkedList<GameObject> _MeteorList;

    private void Awake()
    {
        _MeteorList = new LinkedList<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            _MeteorList.AddLast(transform.GetChild(i).gameObject);
            _MeteorList.Last.Value.SetActive(false);
        }
        StartCoroutine(MeteorOnableRoutine());
    }
    private IEnumerator MeteorOnableRoutine()
    {
        while (_MeteorList.Count > 0)
        {
            _MeteorList.First.Value.SetActive(true);
            _MeteorList.RemoveFirst();

            float waitTime = Random.Range(6f, 12f);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
