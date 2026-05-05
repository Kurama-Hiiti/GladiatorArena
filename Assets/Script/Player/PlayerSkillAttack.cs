using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillAttack
{

    private PlayerNormalAttack attack;

    //プレイヤーのスクリプト
    private Player playerScript;

    //コンストラクタ
    public PlayerSkillAttack(Player player)
    {
        this.playerScript = player;

        attack = new PlayerNormalAttack(player);
    }

    

    public int CostSpQ, CostSpE, CostSpR, CostSpF;


    //スキル攻撃関数
    public void SkillAttack(ref bool isSkillAttacking, ref bool isDodge, Animator anim, ref int currentSp, ref bool nextAttackFlag)
    {
        if (!isSkillAttacking && !isDodge)
        {

            if (Input.GetKeyDown(KeyCode.Q) && CostSpQ < currentSp)
            {
                anim.SetBool("Run", false);
                playerScript.keyDownNum++;

                if (playerScript.keyDownNum == 1)
                {
                    anim.SetBool("Skill1", true);
                    playerScript.UpdateAnimationSpeed("SkillQ");
                    //コンボリセット
                    attack.comboStep = 0;

                    nextAttackFlag = false;

                    anim.SetInteger("ComboStep", attack.comboStep);

                    //SP消費
                    currentSp -= CostSpQ;
                }




            }
            else if (Input.GetKeyDown(KeyCode.E) && CostSpE < currentSp)
            {

                playerScript.keyDownNum++;

                anim.SetBool("Run", false);

                if (playerScript.keyDownNum == 1)
                {
                    anim.SetBool("Skill2", true);
                    playerScript.UpdateAnimationSpeed("SkillE");
                    //コンボリセット
                    attack.comboStep = 0;

                    nextAttackFlag = false;

                    anim.SetInteger("ComboStep", attack.comboStep);

                    //SP消費
                    currentSp -= CostSpE;
                }


            }
            else if (Input.GetKeyDown(KeyCode.R) && CostSpR < currentSp)
            {
                playerScript.keyDownNum++;

                anim.SetBool("Run", false);

                if (playerScript.keyDownNum == 1)
                {
                    anim.SetBool("Skill3", true);
                    playerScript.UpdateAnimationSpeed("SkillR");
                    //コンボリセット
                    attack.comboStep = 0;

                    nextAttackFlag = false;

                    anim.SetInteger("ComboStep", attack.comboStep);

                    //SP消費
                    currentSp -= CostSpR;
                }


            }
            else if (Input.GetKeyDown(KeyCode.F) && CostSpF < currentSp)
            {
                playerScript.keyDownNum++;


                anim.SetBool("Run", false);

                if (playerScript.keyDownNum == 1)
                {
                    anim.SetBool("Skill4", true);
                    playerScript.UpdateAnimationSpeed("SkillF");
                    //コンボリセット
                    attack.comboStep = 0;

                    nextAttackFlag = false;

                    anim.SetInteger("ComboStep", attack.comboStep);

                    //SP消費
                    currentSp -= CostSpF;
                }


            }
            else if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.F))
            {
                //SPがたりない時のSEを鳴らす
                playerScript.BeepSE();
            }
        }
    }
}
