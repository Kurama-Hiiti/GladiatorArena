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
    private AnimatorClipInfo clipInfo;

    /// <summary>
    /// ジョブ個別に実装する通常攻撃関数
    /// </summary>
    public void NormalAttack(Animator anim, float bufferTime, ref bool nextAttackFlag, bool isAttacking)
    {
        clipInfo = anim.GetCurrentAnimatorClipInfo(0)[0];

        if (Input.GetMouseButtonDown(0))
        {
            bufferTimer = bufferTime;

            if (comboStep == 0)
            {
                comboStep = 1;
                anim.SetTrigger("Attack");
                playerScript.UpdateAnimationSpeed("NormalAttack1");

            }
            else if (comboStep < 3 && nextAttackFlag)
            {
                comboStep++;
                anim.SetTrigger("Attack");
                nextAttackFlag = false;
            }

            anim.SetInteger("ComboStep", comboStep);

            switch (clipInfo.clip.name)
            {
                case "NormalAttack1":
                    playerScript.UpdateAnimationSpeed("NormalAttack1");
                    break;

                case "NormalAttack2":
                    playerScript.UpdateAnimationSpeed("NormalAttack2");

                    break;

                case "NormalAttack3":
                    playerScript.UpdateAnimationSpeed("NormalAttack3");
                    break;

            }

        }

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
