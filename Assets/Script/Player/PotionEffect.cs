using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionEffect
{
    //ポーションの効果処理まとめ

    //プレイヤーのスクリプト
    private Player playerScript;

    //コンストラクタ
    public PotionEffect(Player player)
    {
        this.playerScript = player;
    }


    public void UsePotion(ItemData potion)
    {

        switch (potion.name)
        {
            case "HealthPotion":
                //体力全回復
                playerScript.currentHp = playerScript.maxHp;
                playerScript.HealEffect();
                break;

            case "DefensePotion":
                //このバトル中防御力上昇

                //防御力上昇処理
                playerScript.PotionUpdateStatus(potion.PotionAttackUpRate, potion.PotionDefenseUpRate);
                playerScript.EnhanceEffect();

                break;

            case "AttackPotion":
                //このバトル中攻撃力上昇

                //攻撃力上昇処理
                playerScript.PotionUpdateStatus(potion.PotionAttackUpRate, potion.PotionDefenseUpRate);
                playerScript.EnhanceEffect();


                break;

            case "EnhancePotion":
                //このバトル中攻防上昇

                playerScript.PotionUpdateStatus(potion.PotionAttackUpRate, potion.PotionDefenseUpRate);
                playerScript.EnhanceEffect();

                break;

            case "AllPotion":
                //このバトル中攻防上昇、体力全回復
                playerScript.currentHp = playerScript.maxHp;

                playerScript.PotionUpdateStatus(potion.PotionAttackUpRate, potion.PotionDefenseUpRate);
                //playerScript.HealEffect();
                playerScript.EnhanceEffect();


                break;
        }


    }

}
