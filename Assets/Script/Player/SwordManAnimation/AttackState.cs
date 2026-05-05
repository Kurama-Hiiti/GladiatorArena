using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    //攻撃用のコライダーを管理する

    // ステートに入ったとき攻撃用のコライダーを無くす
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<SwordMan>().HiddenWeaponCollider();

        animator.GetComponent<SwordMan>().HiddenShieldCollider();
    }


}
