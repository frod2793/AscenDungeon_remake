using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundName
{
    None, ButtonClick1, ButtonClick2, ErrorWindow, OpenInventory, EquipItem, UnlockItem, PlayerDash,
    ItemBoxOpen, DungeonResult, MysteriousBulletHit, ArrowHit, BowPull, BowShoot, SummonFrozenPillar, NormalSwing,
    ShieldDefence, ThrowShuriken, GreatSword,
    BossAppear_Forest, BossAppear_DeepSea, BossAppear_Factory, RepairBot_Attack, CannonBot_Attack, HammerBot_Attack,
    GoblinAssassin_Dash, GoblinChief_SummonTotem, GoblinChief_SwingAttack, DartHit, DartShoot, GoblinNormal_Attack,
    BuffTotem, ExplosionTotem, LightningTotem, Golem_Attack,
    SharkTheSpearman_Attack, LatentMonkfish_Attack, LatentMonkfish_Swoop, PufferFish_Explosion,
    PickUpCoin, Resurrection,
    ShowDungeonSelection, BuyItem
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private SoundPack[] _SoundPacks;

    private Dictionary<SoundName, SoundPack> _SoundLib;

    private void Reset()
    {
        _SoundPacks = FindObjectsOfType<SoundPack>();
    }

    private void Awake()
    {
        if (FindObjectsOfType<SoundManager>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            // [해결]: 부모가 있는 상태에서 DontDestroyOnLoad를 호출하면 경고가 발생하므로, 부모를 해제하여 루트로 만듭니다.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _SoundLib = new Dictionary<SoundName, SoundPack>();
            for (int i = 0; i < _SoundPacks.Length; ++i)
            {
                _SoundLib.Add(_SoundPacks[i].Name, _SoundPacks[i]);
            }
        }
    }
    public void PlaySound(SoundName name)
    {
        _SoundLib[name].PlaySound();
    }
}
