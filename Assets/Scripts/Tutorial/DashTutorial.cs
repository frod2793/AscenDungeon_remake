using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashTutorial : TutorialBase
{
    public override void StartTutorial()
    {
        base.StartTutorial();

        // 플레이어의 대쉬가 끝났을 때
        var player = FindObjectOfType<Player>();
        player.OnceDashEndEvent += p =>
        {
            EndTutorial();
        };
    }
}
