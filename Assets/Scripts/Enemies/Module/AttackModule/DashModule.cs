using System;
using System.Collections;
using UnityEngine;

public class DashModule : AttackModule
{
    public event Action DashBeginEvent;
    public event Action DashEndEvent;

    [SerializeField] private Area _Range;
    [SerializeField] private Area _AttackArea;

    [Header("DashSection")]
    [SerializeField] [Range(0.1f, 7f)] private float _DashDistance;
    [SerializeField] [Range(0.1f, 7f)] private float _DashSpeedScale;
    [SerializeField] private GameObject _DashEffect;

    private Coroutine _DashRoutine;

    public bool RangeHasAny()
    {
        return _Range.HasAny();
    }
    public void RunningDrive()
    {
        _AttackArea.SetActiveCollider(false);

        _AttackArea.SetEnterAction(o =>
        {
            if (o.TryGetComponent(out ICombatable combatable))
            {

                combatable.Damaged(_AbilityTable.AttackPower, _User);
            }
        });
        _Range.SetScale(_AbilityTable.Range);

        _DashRoutine = new Coroutine(this);
    }
    public void Dash(Vector2 direction)
    {
        _DashRoutine.StartRoutine(EDash(direction));
    }
    private IEnumerator EDash(Vector2 direction)
    {
        Vector2 force = direction * _DashDistance;
        _AttackArea.SetActiveCollider(true);

        Vector2 dashPoint = (Vector2)_User.transform.localPosition + force;
        dashPoint.x.Range(-3.5f, 3.5f);

        if (_DashEffect != null)
        {
            _DashEffect.SetActive(true);
            _DashEffect.transform.parent = null;
        }
        float ratio = 0;

        while (ratio < 1 && _AbilityTable[Ability.CurHealth] > 0f)
        {
            float Speed()
            {
                return Time.timeScale * Time.deltaTime * _DashSpeedScale * _AbilityTable.MoveSpeed;
            }
            ratio = Mathf.Min(1f, ratio + Speed());

            _User.transform.localPosition = Vector2.Lerp(_User.transform.localPosition, dashPoint, ratio);

            yield return null;
        }
        _DashRoutine.Finish();

        if (_DashEffect != null)
        {
            _DashEffect.SetActive(false);
            _DashEffect.transform.parent = _User.transform;
            _DashEffect.transform.localPosition = Vector2.zero;
        }
        _AttackArea.SetActiveCollider(false);
        DashEndEvent?.Invoke();
    }
}
