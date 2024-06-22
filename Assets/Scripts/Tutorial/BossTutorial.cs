using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossTutorial : TutorialBase
{
    [SerializeField] private GoblinNormal   _TutorialBoss;

    [Header("Boss Healthbar")]
    [SerializeField] private GameObject     _RootOfBossHealthbar;
    [SerializeField] private Image          _ImageOfBossHealthbar;

    [Header("Window Property")]
    [SerializeField] private GameObject     _RootOfTutorialClear;

    public override void StartTutorial()
    {
        base.StartTutorial();
        Castle.Instance.CanPlayerFloorNotify = false;

        Inventory.Instance.MoveUpDownEvent += (pos, dir) =>
        {
            // 다음 층으로 이동한다면
            if (dir == Direction.Up && pos == UnitizedPosV.BOT)
            {
                StageClearNotice.Instance.Hide();
            }
        };
        Inventory.Instance.PlayerEnterFloorEvent += BossInit;
    }

    public override void EndTutorial()
    {
        base.EndTutorial();
        _RootOfTutorialClear.SetActive(true);
    }

    private void BossInit()
    {
        HealthBarPool.Instance.UnUsingHealthBar(_TutorialBoss.transform);
        _RootOfBossHealthbar.SetActive(true);

        StartCoroutine(BossWatching());
        Inventory.Instance.PlayerEnterFloorEvent -= BossInit;
    }

    // 보스 관찰...
    private IEnumerator BossWatching()
    {
        do
        {
            _ImageOfBossHealthbar.fillAmount = 
                _TutorialBoss.GetAbility[Ability.CurHealth]/
                _TutorialBoss.GetAbility[Ability.MaxHealth];
            
            yield return null;
        }
        while (_TutorialBoss.GetAbility[Ability.CurHealth] > 0f);
        
        _ImageOfBossHealthbar.fillAmount = 0f;
        EndTutorial();
    }
}
