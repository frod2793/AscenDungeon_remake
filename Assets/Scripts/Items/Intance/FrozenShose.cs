using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenShose : Item
{
    [SerializeField] private Animator Animator;

    [Header("Pillar Section")]
    [SerializeField] private Projection FrozenPillar;
    [SerializeField] private float PillarOffsetY;

    private Pool<Projection> _PillarPool;
    private bool _IsAlreadyInit = false;
    private int _AnimControlKey;

    private GameObject _Player;

    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        Animator.SetBool(_AnimControlKey, false);
    }

    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        Animator.SetBool(_AnimControlKey, true);
    }

    public override void OffEquipThis(SlotType offSlot)
    {
        if (offSlot == SlotType.Accessory)
        {
            Inventory.Instance.MoveUpDownEvent -= MoveUpDownEvent;
        }
    }

    public override void OnEquipThis(SlotType onSlot)
    {
        Init();

        if (onSlot == SlotType.Accessory)
        {
            Inventory.Instance.MoveUpDownEvent += MoveUpDownEvent;
        }
    }

    private void MoveUpDownEvent(UnitizedPosV room, Direction direction)
    {
        MainCamera.Instance.Shake(0.35f, 0.7f);

        var playerMovePoints = Castle.Instance.PlayerFloor.GetRooms()[(int)room].GetMovePoints();

        float pointX = Random.Range(playerMovePoints[0].x, playerMovePoints[2].x);

        _PillarPool.Get().Shoot(new Vector3(pointX, playerMovePoints[1].y + PillarOffsetY, 0f), Vector2.zero, 0f);
        SoundManager.Instance.PlaySound(SoundName.SummonFrozenPillar);
    }

    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            if (_Player == null)
            {
                _Player = GameObject.FindGameObjectWithTag("Player");
            }
            _PillarPool = new Pool<Projection>();
            _PillarPool.Init(4, FrozenPillar, pillar =>
            {
                pillar.SetAction(
                hit => {
                    if (hit.TryGetComponent(out ICombatable combatable))
                    {
                        combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
                        Inventory.Instance.OnAttackEvent(_Player, combatable);
                    }
                },
                fro => {
                    _PillarPool.Add(pillar);
                }); 
            });
            _AnimControlKey = Animator.GetParameter(0).nameHash;

            _IsAlreadyInit = true;
        }
    }

    // 애니메이션 이벤트로 실행된다
    private void SummonFrozenPillar()
    {
        MainCamera.Instance.Shake(0.35f, 1.4f);

        var playerMovePoints = Castle.Instance.GetPlayerRoom().GetMovePoints();

        for (int i = 0; i < 2; i++) {

            for (int j = 0; j < 2; j++)
            {
                float pointX = Random.Range(playerMovePoints[i].x, playerMovePoints[i + 1].x);

                _PillarPool.Get().Shoot(new Vector3(pointX, _Player.transform.position.y + PillarOffsetY, 0f), Vector2.zero, 0f);
            }
        }
        SoundManager.Instance.PlaySound(SoundName.SummonFrozenPillar);
    }

    public override void AttackCancel()
    {
        Animator.SetBool(_AnimControlKey, false);
    }
}
