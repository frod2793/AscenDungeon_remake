using System;
using System.Collections;
using UnityEngine;

public class MovementModule : MonoBehaviour
{
    #region
    /// <summary>
    /// parameter : 이동지점이 왼쪽에 있는지의 여부
    /// </summary>
    #endregion
    public event Action<bool> BeginOfMovementEvent;
    public event Action         EndOfMovementEvent;

    public Func<bool> CanMovement;
    public Func<bool> IsLookAtPlayer;
    public Func<bool> IsLookAtLeft;

    [Header("Movement Module")]
    [SerializeField] private Vector2 _BasePosition;
    [SerializeField] private float _WaitForMovementMin;
    [SerializeField] private float _WaitForMovementMax;

    private IEnumerator _EMove;
    private AbilityTable _AbilityTable;

    public void SetMovementEvent(Action<bool> begin, Action end)
    {
          EndOfMovementEvent += end;
        BeginOfMovementEvent += begin;
    }
    public void SetMovementEvent(RecognitionModule recognition, Action begin, Action end)
    {
          EndOfMovementEvent += end;
        BeginOfMovementEvent += o =>
        {
            recognition.SetLookingLeft(o);

            begin.Invoke();
        };
    }
    public void SetMovementLogic(Func<bool> canMovement, Func<bool> lookAtPlayer, Func<bool> lookAtLeft)
    {
        CanMovement = canMovement;
        IsLookAtLeft = lookAtLeft;
        IsLookAtPlayer = lookAtPlayer;
    }
    public void SetMovementLogic(RecognitionModule recognition, Func<bool> canMovement)
    {
        CanMovement = canMovement;

        IsLookAtLeft = () => recognition.IsLookAtLeft;
        IsLookAtPlayer = recognition.IsLookAtPlayer;
    }
    public void RunningDrive(AbilityTable abilityTable)
    {
        _AbilityTable = abilityTable;

        StartCoroutine(MoveRoutine());
    }
    public bool IsMoving()
    {
        return _EMove != null;
    }
    public void MoveStop()
    {
        if (_EMove != null)
        {
            StopCoroutine(_EMove);
            _EMove = null;

            EndOfMovementEvent?.Invoke();
        }
    }
    private IEnumerator MoveRoutine()
    {
        while (_AbilityTable[Ability.CurHealth] > 0)
        {
            yield return new WaitUntil(CanMovement);
            float waitTime = UnityEngine.Random.Range(_WaitForMovementMin, _WaitForMovementMax);

            yield return new WaitForSeconds(waitTime);
            yield return new WaitUntil(CanMovement);
            Vector2 movePoint = _BasePosition;

            if (IsLookAtPlayer())
            {
                if (IsLookAtLeft())
                {
                    movePoint.x = -3.5f;
                }
                else
                    movePoint.x = +3.5f;
            }
            else
            {
                movePoint.x += UnityEngine.Random.Range(-3.5f, 3.5f);
            }
            StartCoroutine(_EMove = Move(movePoint));

            BeginOfMovementEvent?.Invoke(movePoint.x < transform.localPosition.x);
        }
    }
    private IEnumerator Move(Vector2 movePoint)
    {
        Vector3 direction = (movePoint.x > transform.localPosition.x)
            ? Vector3.right : Vector3.left;

        float DeltaTime()
        {
            return Time.deltaTime * Time.timeScale;
        }
        bool CanMoving()
        {
            return direction.x > 0 && transform.localPosition.x < movePoint.x ||
                   direction.x < 0 && transform.localPosition.x > movePoint.x;
        }
        do
        {
            transform.localPosition += direction * _AbilityTable.MoveSpeed * DeltaTime();

            yield return null;

        } while (CanMoving());
        transform.localPosition = movePoint;

        EndOfMovementEvent?.Invoke();
        _EMove = null;
    }
}
