using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove
{
    //プレイヤーのスクリプト
    private Player playerScript;

    //コンストラクタ
    public PlayerMove(Player player)
    {
        this.playerScript = player;
    }

    //移動関数
    public void PlayerMoveMethod(Vector2 inputDir, Transform cam ,float moveSpeed ,float gravity, Animator anim, CharacterController cc, Transform my)
    {

        //カメラの向いている方向のX方向とZ方向の単位ベクトルを取得
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;

        Vector3 velocity = moveDir * moveSpeed * Time.deltaTime;

        // 重力処理
        velocity.y += gravity * Time.deltaTime;


        //インプットの判定
        if (moveDir.magnitude > 0.1f)
        {
            //走る（アニメーション）
            anim.SetBool("Run", true);

            //走りのアニメーションの速度更新
            playerScript.UpdateAnimationSpeed("RunForward");

            // キャラの向きを移動方向に揃える
            my.rotation = Quaternion.LookRotation(moveDir);

        }
        else
        {
            anim.SetBool("Run", false);
        }

        //移動処理
        cc.Move(velocity);


    }
}
