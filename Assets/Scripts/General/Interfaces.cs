using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* IObject
 * 플레이어나 적같은 오브젝트들은 해당 인터페이스를 구현하여 초기화/업데이트를 할때 사용한다.
 * 
 * 이렇게 하는 이유는, 방안에 있는 오브젝트들의 초기화와 업데이트를 제어할 수 있도록 하기 위해서그렇다.
 * 
 * 사용방법 : 
 * - 플레이어가 층에 진입->층에 존재하는 모든 IObject의 IInit함수를 실행
 * - 플레이어가 존재하는 층에 모든 IObject의 IUpdate실행
 */
public interface IObject
{
    void IInit();
    void IUpdate();

    void PlayerEnter(MESSAGE message, Player enterPlayer);
    void PlayerExit (MESSAGE message);

    bool IsActive();

    GameObject ThisObject();
}
public interface ICombatable
{
    AbilityTable GetAbility
    {
        get;
    }

    void Damaged(float damage, GameObject attacker);

    void CastBuff(Buff buffType, IEnumerator castedBuff);
}

public enum AnimState
{
    Idle, Move, Attack, Damaged, Death, AttackBegin, AttackAfter
}

public interface IAnimEventReceiver
{
    void AnimationPlayOver(AnimState anim);
}