using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageCalculation : MonoBehaviour
{
    //敵がプレイヤーに攻撃するときのダメージ処理関数(敵の攻撃判定の場所にアタッチ)

    private float SkillDamageRate = 1.2f;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private EnemyManager enemyManager;

    public int PlayerTakeDamage(int playerDefensePower)
    {
        float damage = 0;

        AnimatorClipInfo clipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];

        string animationName = clipInfo.clip.name;

        if (animationName == "SkillAttack")
        {
            damage = (enemyManager.enemyAttackPower * SkillDamageRate) * (100f / (100f + playerDefensePower));
        }
        else
        {
            damage = enemyManager.enemyAttackPower * (100f / (100f + playerDefensePower));
        }
        

        return (int)damage;

    }

}
