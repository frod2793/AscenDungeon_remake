using System.Linq;
using UnityEngine;

public class MysteriousMace : Item
{
    [Range(0f, 1f)]
    [SerializeField] private float Probablity;
    [SerializeField] private MysteriousBullet TracerBullet;

    [SerializeField] private Animator Animator;
    [SerializeField] private Area CollisionArea;

    private Pool<MysteriousBullet> mPool;

    private int _AnimControlKey;

    private GameObject mPlayer;

    private void Reset()
    {
        Debug.Assert(TryGetComponent(out Animator));
    }

    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        Animator.SetBool(_AnimControlKey, true);

        mPlayer = attacker;
    }

    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        Animator.SetBool(_AnimControlKey, false);
    }

    public override void OffEquipThis(SlotType offSlot)
    {
        
    }

    public override void OnEquipThis(SlotType onSlot)
    {
        if (onSlot.Equals(SlotType.Weapon))
        {
            if (mPool == null)
            {
                mPool = new Pool<MysteriousBullet>();
                mPool.Init(2, TracerBullet, o => o.DisableAction = b =>
                {
                    mPool.Add(b);
                });
            }
            CollisionArea.SetEnterAction(HitAction);
            
            _AnimControlKey = Animator.GetParameter(0).nameHash;

            mPlayer = transform.parent.parent.gameObject;
        }
    }

    private void HitAction(GameObject target)
    {
        if (target.TryGetComponent(out ICombatable combatable))
        {
            if (combatable.GetAbility.Table[Ability.CurHealth] <= 0)
            {
                return;
            }
            combatable.Damaged(StatTable[ItemStat.AttackPower], mPlayer);

            Inventory.Instance.OnAttackEvent(mPlayer, combatable);

            if (Random.value <= Probablity)
            {
                Vector2 playerPos = mPlayer.transform.position;

                var targetIObject = Castle.Instance.GetLongestIObject(playerPos);

                if (targetIObject != null)
                {
                    var bullet = mPool.Get();

                    bullet.transform.position = playerPos;
                    bullet.Shoot(targetIObject.ThisObject().transform, TargetHitAction);
                }
            }
        }
    }

    private void TargetHitAction(GameObject target)
    {
        if (target.TryGetComponent(out ICombatable combat))
        {
            // 2/3의 피해
            combat.Damaged(StatTable[ItemStat.AttackPower] * 0.666f, mPlayer);
            Inventory.Instance.ProjectionHit(target, StatTable[ItemStat.AttackPower]);

        }
    }
    protected override void CameraShake()
    {
        MainCamera.Instance.Shake(0.25f, 1.8f);
    }

    public override void AttackCancel()
    {
        Animator.SetBool(_AnimControlKey, false);
    }
}
