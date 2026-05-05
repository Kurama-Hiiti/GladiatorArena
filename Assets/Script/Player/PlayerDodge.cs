using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodge
{
    //プレイヤーのスクリプト
    private Player playerScript;

    //コンストラクタ
    public PlayerDodge(Player player)
    {
        this.playerScript = player;
    }

    //回避関数
    public void Dodge(Animator anim, ref bool isDodge ,ref int currentSp, int costSp)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDodge && costSp <= currentSp)
        {

            anim.SetTrigger("Dodge");
            isDodge = true;

            anim.SetBool("Run", false);

            //SP消費する
            currentSp -= costSp;

        }else if(Input.GetKeyDown(KeyCode.LeftShift) && !isDodge && currentSp < costSp)
        {
            //SPがたりない時のSEを鳴らす
            playerScript.BeepSE();
        }
    }


}
