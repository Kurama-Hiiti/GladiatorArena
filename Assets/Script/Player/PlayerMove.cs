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
        //float x = Input.GetAxis("Horizontal"); // A/D または ←/→
        //float z = Input.GetAxis("Vertical");   // W/S または ↑/↓

        //カメラの向いている方向のX方向とZ方向の単位ベクトルを取得
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;

        Vector3 velocity = moveDir * moveSpeed * Time.deltaTime;

        // 重力処理
        velocity.y += gravity * Time.deltaTime;


        if (moveDir.magnitude > 0.1f)
        {
            anim.SetBool("Run", true);

            playerScript.UpdateAnimationSpeed("RunForward");

            // キャラの向きを移動方向に揃える
            my.rotation = Quaternion.LookRotation(moveDir);

        }
        else
        {
            anim.SetBool("Run", false);
        }


        cc.Move(velocity);


    }
}
