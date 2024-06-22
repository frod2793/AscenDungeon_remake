using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorite : Item
{
    [Header("Meteorh Property")]
    [SerializeField] private float _Speed;
    [SerializeField] private float _Radius;
    [Space()]
    [SerializeField] private Area _Meteore;

    private GameObject _Player;

    public override void AttackCancel()
    { }
    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                {
                    Galaxy.Instance.RemoveMeteorite();
                }
                break;
            case SlotType.Weapon:
                {
                    Galaxy.Instance.RemoveMeteorite(3);
                }
                break;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        if (!Galaxy.Instance.IsAlreadyInit)
        {
            _Player = GameObject.FindGameObjectWithTag("Player");

            Galaxy.Instance.LazyInit(_Meteore, meteo =>
            {
                meteo.SetEnterAction(enter => {

                    float damage = StatTable[ItemStat.AttackPower];
                    if (enter.TryGetComponent(out ICombatable combatable))
                    {
                        combatable.Damaged(damage, _Player);

                        Inventory.Instance.OnAttackEvent(_Player, combatable);
                        Inventory.Instance.ProjectionHit(enter, damage);
                    }
                });
            });
            Galaxy.Instance.Resolving_Radius = _Radius;
            Galaxy.Instance.Resolving_Speed  = _Speed;
        }

        switch (onSlot)
        {
            case SlotType.Accessory:
                {
                    Galaxy.Instance.AddMeteorite();
                }
                break;
            case SlotType.Weapon:
                {
                    Galaxy.Instance.AddMeteorite(3);
                }
                break;
        }
    }
}
