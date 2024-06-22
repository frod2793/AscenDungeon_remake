using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerousBottle : Item
{
    [SerializeField] private Animator _Animator;
    [SerializeField] private BoxCollider2D _Collider;

    [Header("Particle Property")]
    [SerializeField] private GameObject _ParticleObject;
    [SerializeField] private float _ParticleOffsetX;
    [SerializeField, Range(0f, 10f)] private float _ParticleDurate;

    [Header("Poision Property")]
    [SerializeField, Range( 1,  10)] private uint _W_PoisionLevel;
    [SerializeField, Range(0f, 10f)] private float _W_PoisionDurate;

    [Space()]
    [SerializeField, Range( 1,  10)] private uint _A_PoisionLevel;
    [SerializeField, Range(0f, 10f)] private float _A_PoisionDurate;

    private int _AnimatorHash;
    private bool _IsAlreadyInit = false;

    private GameObject _Player;
    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _IsAlreadyInit = true;
            _AnimatorHash = _Animator.GetParameter(0).nameHash;

            _Collider.enabled = false;

            _ParticleObject = Instantiate(_ParticleObject);
            _ParticleObject.SetActive(false);
        }
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        _Player = attacker;
        _Animator.SetBool(_AnimatorHash, true);
    }
    protected override void CameraShake()
    {
        MainCamera.Instance.Shake(0.2f, 0.8f);
    }
    private void UsingParticle()
    {
        _ParticleObject.SetActive(true);

        Vector3 particleOffset = _ParticleOffsetX * (_Player.transform.rotation.eulerAngles.y > 0 ? Vector3.left : Vector3.right);
        _ParticleObject.transform.position = transform.position + particleOffset;

        StartCoroutine(ParticleLife());
    }
    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        _Animator.SetBool(_AnimatorHash, false);
    }
    public override void AttackCancel()
    {
        _Animator.SetBool(_AnimatorHash, false);
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        if (offSlot == SlotType.Accessory)
        {
            Inventory.Instance.AttackEvent -= AttackEvent;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        Init();

        if (onSlot == SlotType.Accessory)
        {
            Inventory.Instance.AttackEvent += AttackEvent;
        }
    }
    private void AttackEvent(GameObject target, ICombatable targetCombatable)
    {
        targetCombatable.CastBuff(Buff.Poision, GetPoisionBuff(_A_PoisionLevel, _A_PoisionDurate, targetCombatable));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent(out ICombatable combatable))
            {
                combatable.CastBuff(Buff.Poision, GetPoisionBuff(_W_PoisionLevel, _W_PoisionDurate, combatable));
                combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);

                Inventory.Instance.OnAttackEvent(_Player, combatable);
            }
        }
    }
    private IEnumerator GetPoisionBuff(uint level, float durate, ICombatable combatable)
    {
        return BuffLibrary.Instance.GetBuff(Buff.Poision, level, durate, combatable.GetAbility);
    }
    private IEnumerator ParticleLife()
    {
        for (float i = 0f; i < _ParticleDurate; i += Time.deltaTime * Time.timeScale)
        {
            yield return null;
        }
        _ParticleObject.SetActive(false);
    }
}
