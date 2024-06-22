using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int Value;

    [SerializeField] private Collider2D _Collider;

    [Header("PopAnimation")]
    [SerializeField] private float _PopAnimationTime;
    [Range(0f, 1f)]
    [SerializeField] private float _PopRange;

    [Header("Animator")]
    [SerializeField] private Animator _Animator;
    
    private int _AnimationHash;
    private Coroutine _PopRoutine;

    public void Init()
    {
        if (_PopRoutine == null)
        {
            _PopRoutine = new Coroutine(this);
        }
        _AnimationHash = _Animator.GetParameter(0).nameHash;
    }
    public void Enable()
    {
        _PopRoutine.StartRoutine(PopAnimation());
    }
    private void OnDisable()
    {
        transform.localScale = Vector3.one;
        _Animator.SetBool(_AnimationHash, false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            MoneyManager.Instance.AddMoney(Value);
            _Animator.SetBool(_AnimationHash, true);

            SoundManager.Instance.PlaySound(SoundName.PickUpCoin);
            _PopRoutine.StopRoutine();
        }
    }
    private void Disable()
    {
        gameObject.SetActive(false);

        CoinPool.Instance.Add(this);
    }
    private IEnumerator PopAnimation()
    {
        _Collider.enabled = false;

        Vector2 start = transform.position;

        Vector2 target = start + Vector2.right * Random.Range(-_PopRange, _PopRange);
                target.x.Range(-4, 4);

        for (float i = 0; i < _PopAnimationTime; i += Time.deltaTime * Time.timeScale)
        {
            transform.position = Vector2.Lerp(start, target, i / _PopAnimationTime);
            yield return null;
        }
        _PopRoutine.Finish();

        _Collider.enabled = true;
    }
}
