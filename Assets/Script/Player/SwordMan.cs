using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

public class SwordMan : Player
{
    //武器と盾のコライダー
    [SerializeField]
    private BoxCollider weapon;

    [SerializeField]
    private BoxCollider shield;

    //サウンドマネージャー
    [SerializeField]
    private SwordManSoundManager swordManSoundManager;

    //初期の武器コライダーサイズ
    private Vector3 weaponColliderSize;

    //剣の軌跡
    [SerializeField]
    private TrailRenderer swordTrail;


    private void Awake()
    {
        Init();

        weaponColliderSize = weapon.size;

        weapon.enabled = false;
        shield.enabled = false;
        swordTrail.enabled = false;

    }

    private void Start()
    {
        //Init();

    }

    private void Update()
    {

        //キャラクターコントローラのオンオフ
        CharacterConntrollerManager();

        //Waveクリアした瞬間にスキル等の攻撃をしてコライダーが表示されている場合はコライダーを非表示にする
        if (GameManager.instance.state == GameManager.GameState.GameClear)
        {
            AnimationReset();

            if (weapon.enabled || shield.enabled)
            {
                weaponColliderSize = weapon.size;

                weapon.enabled = false;
                shield.enabled = false;
            }
        }


        //バトル中のみ行動可能
        if (GameManager.instance.state == GameManager.GameState.Battle)
        {
            

            PlayerDeath();

            //連続ヒット防止関数
            TakeDamageInterval();

            //アニメーションからの状態取得
            AnimationStateInfo();



            if (!isDead)
            {
                //ガード
                Blocking();
                //ガード中は他の行動はできない
                if (isBlocking) return;

                //SP自然回復
                NaturalRecoverySP();

                //HP自然回復
                NaturalRecoveryHP();



                //回避
                dodge.Dodge(animator, ref isDodge, ref currentSp, dodgeCostSp);

                //通常攻撃
                attack.NormalAttack(animator, bufferTime, ref nextAttackFlag, isAttacking);


                //スキル攻撃
                skillAttack.SkillAttack(ref isSkillAttacking, ref isDodge, animator, ref currentSp, ref nextAttackFlag);


                //移動
                if (!isDodge && !isAttacking && !isSkillAttacking && keyDownNum == 0 && !isTakeDamege)
                {
                    //移動可能な場合はブロックトリガーはオフにする
                    animator.ResetTrigger("Block");
                    //ポーション発動関数
                    PotionActivation();

                    Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                    move.PlayerMoveMethod(input, cam, playerMoveSpeed, gravity, animator, cc, this.transform);

                }

            }
        }


    }


    //コライダー管理(AttackStateで使用)
    public void ShowWeaponCollider()
    {
        AnimatorClipInfo skillClipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];

        string skillName = skillClipInfo.clip.name;

        if (skillName == "SkillF")
        {
            weapon.size = weaponColliderSize + new Vector3(0, 0, 0.5f);
        }

        weapon.enabled = true;
        swordTrail.enabled = true;
    }

    public void ShowShieldCollider()
    {
        shield.enabled = true;
    }

    public void HiddenWeaponCollider()
    {
        //コライダーサイズリセット
        weapon.size = weaponColliderSize;
        //非表示
        weapon.enabled = false;
        swordTrail.enabled = false;
    }

    public void HiddenShieldCollider()
    {
        shield.enabled = false;
    }


    //攻撃SEをアニメーションで呼び出す
    public void SwordManAttackSE()
    {
        swordManSoundManager.audioSource.volume = 0.2f;
        swordManSoundManager.PlaySE(SwordManSoundType.attack);
    }




    //敵にヒット時に呼び出すダメージ計算関数
    public override DamageResult EnemyTakeDamage(int enemyDefense)
    {
        //親の結果を取得
        DamageResult result = base.EnemyTakeDamage(enemyDefense);

        // ダメージ値だけにスキル倍率を掛ける
        result.damage *= SkillAttackDamageRate();

        return result;

    }


    //スキルによって攻撃力変化
    private float SkillAttackDamageRate()
    {
        float rate = 1.0f;

        AnimatorClipInfo skillClipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];

        string skillName = skillClipInfo.clip.name;

        if (isSkillAttacking)
        {
            switch (skillName)
            {
                case "SkillQ": 
                    rate = data.SkillDamageRateQ + skillRateQ;
                    break;

                case "SkillE":
                    rate = data.SkillDamageRateE + skillRateE;
                    break;

                case "SkillR":
                    rate = data.SkillDamageRateR + skillRateR;
                    break;

                case "SkillF":
                    rate = data.SkillDamageRateF + skillRateF;
                    break;
            }
        }

        return rate;
    }
    


    private void OnTriggerEnter(Collider other)
    {
        if (!isDead)
        {
            //連続ヒット防止
            if (isHit) return;

            if (other.gameObject.CompareTag("Enemy") && !isHit && !isBlocking &&  !isDodge)
            {
                isHit = true;
                if (!isAttacking && !isSkillAttacking)
                {
                    animator.SetTrigger("Damage");
                    isTakeDamege = true;
                }


                //ダメージ計算
                currentHp -= other.gameObject.GetComponent<EnemyDamageCalculation>().PlayerTakeDamage(defensePower);

                //画面揺れ
                GameManager.instance.OnHit();

                //画面の淵を赤くするエフェクト
                DamageEffect.instance.PlayDamageEffect();

                PlayerDamageSE();

            }
            else if (isBlocking)
            {
                isHit = true;
                animator.SetTrigger("Block");
                currentSp -= blockCost;

                //防御SE
                commonSoundManager.PlaySE(CommonSoundType.Guard);
            }
        }


    }
}
