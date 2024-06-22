using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownNPC : MonoBehaviour
{
    private const int Idle = 0;
    private const int Move = 1;

    [SerializeField] private SpriteRenderer _Renderer;
    [SerializeField] private Animator _Animator;
    private int _AnimControlHash;

    [Header("Move Property")]
    [SerializeField] private float _MoveRangeX;
    [SerializeField] private float _MoveSpeed;

    [Header("Movement Time")]
    [SerializeField] private float _MoveTimeMin;
    [SerializeField] private float _MoveTimeMax;

    [Header("Waiting Time")]
    [SerializeField] private float _MoveWaitMin;
    [SerializeField] private float _MoveWaitMax;

    private IEnumerator _MoveRoutine;
    private bool _IsAlreadyInit = false;

    private void Reset()
    {
        TryGetComponent(out _Renderer);
        TryGetComponent(out _Animator);
    }
    private void Start() 
    {
        StartCoroutine(_MoveRoutine = MoveRoutine());
    }
    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _AnimControlHash = _Animator.GetParameter(0).nameHash;
            _IsAlreadyInit = true;
        }
    }
    private void StopMoveRoutine()
    {
        StopCoroutine(_MoveRoutine);
        _MoveRoutine = null;
    }
    private IEnumerator MoveRoutine()
    {
        Init();

        while (_MoveRoutine != null)
        {
            float wait = Random.Range(_MoveWaitMin, _MoveWaitMax);

            _Animator.SetInteger(_AnimControlHash, Idle);
            yield return new WaitForSeconds(wait);
            _Animator.SetInteger(_AnimControlHash, Move);
            
            float move = Random.Range(_MoveTimeMin, _MoveTimeMax);
            Vector3 direction = new Vector3(Mathf.Sign(Random.Range(-1, 1 +1)), 0, 0);

            // 더 이상 direction의 방향으로 움직일 수 없다면, 방향을 반대로 바꾼다.
            if (transform.localPosition.x >= +_MoveRangeX && direction.x > 0 ||
                transform.localPosition.x <= -_MoveRangeX && direction.x < 0)
            {
                direction *= -1f;
            }
            // 이동하는 방향으로 스프라이트를 뒤집는다.
            _Renderer.flipX = direction.x > 0;

            float deltaTime = Time.deltaTime * Time.timeScale;
            
            for (float i = 0f; i < move; i += deltaTime)
            {
                transform.localPosition += direction * deltaTime * _MoveSpeed;

                if (transform.localPosition.x > +_MoveRangeX ||
                    transform.localPosition.x < -_MoveRangeX )
                {
                    transform.localPosition.Set
                        (Mathf.Clamp(transform.localPosition.x, -_MoveRangeX, _MoveRangeX), transform.localPosition.y, 0);

                    break;
                }
                yield return null;
                deltaTime = Time.deltaTime * Time.timeScale;
            }
        }
    }
}
