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

public class Mage : Player 
{
    //魔法の弾
    [SerializeField]
    private GameObject magicBullet;

    //魔法発射位置
    [SerializeField]
    private Transform shootPos;

    //放たれた魔法の親オブジェクト
    [SerializeField]
    private GameObject magicPool;

    //通常攻撃の処理関数
    private MagicPool magic;


    //スキルQのコライダーとエフェクトを含んだオブジェクト
    [SerializeField]
    private GameObject skillQCollider;

    //スキルEのコライダーとエフェクトを含んだオブジェクト
    [SerializeField]
    private GameObject skillECollider;

    //スキルEのコライダーを表示する位置
    [SerializeField]
    private GameObject skillEPosObj;

    //スキルRの魔法オブジェクト
    [SerializeField]
    private GameObject skillRMagic;

    //スキルRの発生位置
    [SerializeField]
    private Transform skillRMagicPos;

    //スキルFの魔法オブジェクト
    [SerializeField]
    private GameObject skillFMagic;

    [SerializeField]
    private Transform skillFMagicPos;

    [SerializeField]
    private GameObject mageShieldEffect;

    //サウンドマネージャー
    [SerializeField]
    private MageSoundManager mageSoundManager;



    private void Awake()
    {
        Init();

        magic = GetComponent<MagicPool>();
    }

    private void Update()
    {

        //キャラクターコントローラのオンオフ
        CharacterConntrollerManager();

        if (GameManager.instance.state == GameManager.GameState.GameClear)
        {
            AnimationReset();
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
                if (isBlocking)
                {
                    mageShieldEffect.SetActive(true);
                    return;
                }
                else
                {
                    mageShieldEffect.SetActive(false);
                }
                    

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

            if (other.gameObject.CompareTag("Enemy") && !isHit && !isBlocking && !isDodge)
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


    //魔法発射関数
    public void Shoot()
    {
        //魔法発射
        magic.MagicShoot(magicBullet, shootPos, magicPool.transform);

        //魔法発射SE
        mageSoundManager.PlaySE(MageSoundType.NormalAttack);

    }


    //スキルQのコライダー表示関数
    public void MageSkillQ()
    {

        skillQCollider.SetActive(true);

        //爆発音
        mageSoundManager.PlaySE(MageSoundType.SkillQ);
    }

    //スキルQのコライダー非表示
    public void MageSkillQHidden()
    {
        skillQCollider.SetActive(false);

    }


    //スキルEのコライダー
    public void MageSkillE()
    {

        skillECollider.transform.position = skillEPosObj.transform.position;

        skillECollider.transform.rotation = skillEPosObj.transform.rotation;

        skillECollider.SetActive(true);

        //SE
        mageSoundManager.PlaySE(MageSoundType.SkillE);

    }

    //スキルEを非表示
    public void MageSkillEHidden()
    {
        skillECollider.SetActive(false);
    }


    //スキルR発射関数
    public void MageSkillRShoot()
    {
        magic.SkillRShoot(skillRMagic, skillRMagicPos, magicPool.transform);

        //SE
        mageSoundManager.PlaySE(MageSoundType.SkillR);
    }

    //スキルF発射関数
    public void MageSkillFShoot()
    {
        Instantiate(skillFMagic, skillFMagicPos.position, Quaternion.identity);

        //SE
        mageSoundManager.PlaySE(MageSoundType.SkillF);
    }



}
