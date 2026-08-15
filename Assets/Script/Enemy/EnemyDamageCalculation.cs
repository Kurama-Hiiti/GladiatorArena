using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageCalculation : MonoBehaviour
{
    //敵がプレイヤーに攻撃するときのダメージ処理関数(敵の攻撃判定の場所にアタッチ)

    //敵の強攻撃の場合の攻撃倍率
    private float SkillDamageRate = 1.2f;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private EnemyManager enemyManager;

    //ダメージ計算関数
    public int PlayerTakeDamage(int playerDefensePower)
    {
        float damage = 0;

        //現在のアニメーションを取得
        AnimatorClipInfo clipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];

        //アニメーションの名前を取得
        string animationName = clipInfo.clip.name;

        //アニメーションの名前で場合分け
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
