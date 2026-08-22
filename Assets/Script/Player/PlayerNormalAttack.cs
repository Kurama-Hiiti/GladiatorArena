using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalAttack
{
    //現在の攻撃回数（通常攻撃）
    public int comboStep;

    //連撃可能時間猶予
    private float bufferTimer;

    //プレイヤーのスクリプト
    private Player playerScript;

    //コンストラクタ
    public PlayerNormalAttack(Player player)
    {
        this.playerScript = player;
    }

    //アニメーションステート
    private AnimatorStateInfo stateInfo;

    
    // 通常攻撃関数
    public void NormalAttack(Animator anim, float bufferTime, ref bool nextAttackFlag, bool isAttacking)
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        //攻撃モーションの速度更新
        if (stateInfo.IsName("NormalAttack1"))
        {
            playerScript.UpdateAnimationSpeed("NormalAttack1");
        }
        else if (stateInfo.IsName("NormalAttack2"))
        {
            playerScript.UpdateAnimationSpeed("NormalAttack2");
        }
        else if (stateInfo.IsName("NormalAttack3"))
        {
            playerScript.UpdateAnimationSpeed("NormalAttack3");
        }


        if (Input.GetMouseButtonDown(0))
        {
            bufferTimer = bufferTime;

            if (comboStep == 0)
            {
                //初撃のコンボ数
                comboStep = 1;
                //攻撃トリガー
                anim.SetTrigger("Attack");

            }
            else if (comboStep < 3 && nextAttackFlag)　//2，3撃目
            {
                //コンボ数加算
                comboStep++;
                //攻撃トリガー
                anim.SetTrigger("Attack");
                //次の攻撃受付フラグ更新
                nextAttackFlag = false;
            }

            //コンボ数更新
            anim.SetInteger("ComboStep", comboStep);

        }

        //一定時間攻撃が無い場合はコンボ数リセット
        if (!isAttacking)
        {
            bufferTimer -= Time.deltaTime;

            if (bufferTimer < 0)
            {
                comboStep = 0;

                nextAttackFlag = false;

                anim.SetInteger("ComboStep", comboStep);
            }
        }
    }
}
