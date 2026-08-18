using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static GameManager;
using DG.Tweening;
using Cinemachine;

public class EnemyManager : MonoBehaviour
{
    //SE用スクリプト格納
    [SerializeField]
    private EnemySoundManager enemySoundManager;

    //アニメーター
    protected private Animator animator;

    //AIの変数
    private NavMeshAgent agent;

    //移動速度
    [SerializeField]
    private float walkSpeed, chaseSpeed;

    //体力
    [SerializeField]
    private int baseHealth;//基礎体力

    public int maxHealth;//最大体力

    public int currentHealth;//現在の体力

    //基礎攻撃力
    [SerializeField]
    private int enemyBaseAttackPower;

    //総攻撃力
    public int enemyAttackPower;

    //基礎防御力
    [SerializeField]
    private int enemyBaseDefensePower;

    //総防御力
    private int enemyDefensePower;

    //通常攻撃時のコライダー
    [SerializeField]
    private BoxCollider hitCollider;

    //通常攻撃時のコライダー2
    [SerializeField]
    private BoxCollider hitCollider2;

    //強攻撃（スキル）攻撃時のコライダー
    [SerializeField]
    private BoxCollider skillHitCollider;

    //ヒットフラグ
    private bool isHit;

    //攻撃実行中判定
    private bool isAttacking;

    //死亡フラグ
    private bool isDead;



    //ヒット可能までの時間　連続ヒット防止
    [SerializeField]
    private float hitBufferTime;

    private float hitBufferTimer;


    //列挙型のEnemyの状態作成
    private enum STATE { Idle, Caution, Attack, SkillAttack, Chase, Damage, Dead , Paralyze};
    private STATE state = STATE.Idle;

    //攻撃対象（プレイヤー）
    private GameObject target;

    //攻撃対象のスクリプト
    private Player targetScript;

    //各ステータスの成長補正値(Waveが進むにつれて強化される)
    [SerializeField]
    private float healthGrowth;

    [SerializeField]
    private float attackGrowth;

    [SerializeField]
    private float defenseGrowth;


    //ノックバックの飛距離
    private float knockBackDistance = 3f;

    //ノックバック後の硬直時間
    private float knockBackColdTime = 1.5f;

    //自身のコライダー
    private BoxCollider boxCollider;

    //UIの位置
    [SerializeField]
    private GameObject hpUI;

    //UI表示時間(敵のHPバーに変化が無い場合は一定時間後に非表示になる)
    [SerializeField]
    private float showUITime;

    private float showUITimer;

    private bool isShowUI;


    //燃焼効果状態フラグ
    private bool isFlameDamage;

    //燃焼効果持続時間(回数)
    private int flameDamageTime = 5;

    //燃焼効果時間間隔
    private float flameEffectTime = 1f;

    //燃焼効果タイマー
    private float flameEffectTimer;

    //燃焼効果中の減少した攻撃力
    private int downAttackPower;

    //減少前の攻撃力
    private int originAttackPower;


    //氷結状態フラグ
    private bool isFreezing;

    //感電状態フラグ
    private bool isParalyzing;

    //感電状態時間
    private float paralyzingTime = 3f;

    //感電状態タイマー
    private float paralyzingTimer;


    //毒状態フラグ
    private bool isPoisoning;

    //毒ダメージ発生間隔
    private float poisonDamageTime = 1f;

    //毒ダメージタイマー
    private float poisonDamageTimer;


    //継続ダメージ(燃焼、毒)
    private int DamageOverTime = 2;

    //状態異常発生確率(20%)
    private float debuffRate = 0.2f;

    //のけぞりモーション発生確率
    [Tooltip("のけぞりモーション発生確率")]
    [SerializeField]
    private float hitMotionRate;


    //燃焼エフェクト
    [SerializeField]
    private GameObject flameEffect;

    //氷結エフェクト
    [SerializeField]
    private GameObject freezeEffect;

    //感電エフェクト
    [SerializeField]
    private GameObject paralyzeEffect;

    //毒エフェクト
    [SerializeField]
    private GameObject poisonEffect;


    //プレイヤーを発見する距離
    private int detectDistance = 30;

    //プレイヤーを見失う距離
    private int lostDistance = 35;

    //スキル発動確率
    private int skillRate = 30;


    private void Start()
    {
        //ステータス決定
        EnemyStatus();

        //ヒットまでの時間
        hitBufferTimer = hitBufferTime;

        //UIの表示時間
        showUITimer = showUITime;

        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();


        //攻撃用コライダー非表示
        hitCollider.enabled = false;

        if (hitCollider2 != null)
        {
            hitCollider2.enabled = false;
        }

        if (skillHitCollider != null)
        {
            skillHitCollider.enabled = false;
        }
        

        isDead = false;

        //ターゲット（プレイヤー）の設定
        if (target == null)
        {
            target = GameObject.FindWithTag("Player");

            targetScript = target.GetComponent<Player>();
        }

        //自身のコライダー設定
        boxCollider = GetComponent<BoxCollider>();

        hpUI.SetActive(false);

        //燃焼効果中の減少した攻撃力
        downAttackPower =  (int)(enemyAttackPower * 0.7);

        //初期攻撃力を保持
        originAttackPower = enemyAttackPower;


        //エフェクトを非表示
        flameEffect.SetActive(false);
        freezeEffect.SetActive(false);
        paralyzeEffect.SetActive(false);
        poisonEffect.SetActive(false);

    }

    //
    //ここから
    //


    private void Update()
    {

        if (GameManager.instance.state == GameState.Battle)
        {
            if (animator.speed == 0)
            {
                animator.speed = 1;
            }

            //HPが0以下の時は死亡状態へ遷移
            if (currentHealth <= 0)
            {
                state = STATE.Dead;
            }

            //燃焼効果処理
            FlameDamege();

            //感電効果処理
            EnemyParalyzing();

            //毒効果処理
            PoisonDamage();

            //状態以上のエフェクト処理
            StatusAbnormalityEffect();


            //攻撃を受けたときの無敵時間
            if (isHit)
            {
                hitBufferTimer -= Time.deltaTime;

                if (hitBufferTimer < 0)
                {
                    isHit = false;
                    hitBufferTimer = hitBufferTime;
                }
            }


            //敵のHPバーの表示
            if (isShowUI)
            {
                if (!hpUI.activeSelf)
                {
                    hpUI.SetActive(true);
                }

                //UIをプレイヤーの方向に向ける
                hpUI.transform.LookAt(targetScript.cam.position);
                hpUI.transform.rotation = UIRotation();


                showUITimer -= Time.deltaTime;

                //一定時間攻撃を受けなければUI非表示
                if (showUITimer < 0)
                {
                    isShowUI = false;
                    hpUI.SetActive(false);
                    showUITimer = showUITime;
                }
            }

            //敵の状態管理
            EnemyStateController();
        }
        else if (GameManager.instance.state == GameState.GameOver)
        {
            //敵の状態管理
            EnemyStateController();

            //HPUI非表示
            hpUI.SetActive(false);

            //エフェクトを非表示
            flameEffect.SetActive(false);
            freezeEffect.SetActive(false);
            paralyzeEffect.SetActive(false);
            poisonEffect.SetActive(false);
        }


    }

    //表示されたUIの向きをプレイヤーに向け続ける
    private Quaternion UIRotation()
    {
        Quaternion hpUIRotation = hpUI.transform.rotation;

        hpUIRotation.x = 0f;

        hpUIRotation.z = 0f;

        return hpUIRotation;
    }

    //敵の状態管理関数
    private void EnemyStateController()
    {
        //状態によって処理を実行する
        switch (state)
        {
            //停止状態
            case STATE.Idle:

                //アニメーターのフラグリセット
                AnimatorBoolReset();

                //プレイヤーを発見した場合追いかける
                if (CanSeePlayer())
                {
                    //追跡状態へ移行
                    state = STATE.Chase;
                }
                else if (Random.Range(0, 10) < 5) //敵を発見していない場合は確率で警戒状態へ
                {
                    //警戒状態へ移行
                    state = STATE.Caution;
                }

                break;

            //警戒状態
            case STATE.Caution:

                //攻撃フラグが立った時は攻撃を最後まで実行する
                if (isAttacking) return;

                //目的地が設定されていない時、新たな目的地を設定
                if (!agent.hasPath)
                {
                    float newX = transform.position.x + Random.Range(-10, 10);
                    float newZ = transform.position.z + Random.Range(-10, 10);

                    Vector3 NextPos = new Vector3(newX, transform.position.y, newZ);

                    //移動処理
                    agent.SetDestination(NextPos);

                    //その目的地までどのくらい近づくのか（この場合は目的地ちょうどを目指す）
                    agent.stoppingDistance = 0;

                    AnimatorBoolReset();

                    agent.speed = walkSpeed;
                    animator.SetBool("Walk", true);

                }

                //一定の割合で移動中でも一定の確率でIdle状態へ戻る（その場で留まる）
                if (Random.Range(0, 500) < 5)
                {
                    state = STATE.Idle;
                    agent.ResetPath();
                }

                //プレイヤーを発見した時は追跡
                if (CanSeePlayer())
                {
                    state = STATE.Chase;
                }

                break;

            //追跡状態
            case STATE.Chase:

                //攻撃フラグが立った時は攻撃を最後まで実行する
                if (isAttacking) return;

                //ゲームオーバー時は追跡を解除
                if (GameManager.instance.state == GameState.GameOver)
                {
                    AnimatorBoolReset();
                    agent.ResetPath();

                    state = STATE.Caution;

                    return;
                }

                //移動処理
                agent.SetDestination(target.transform.position);

                //その目的地までどのくらい近づくのか（この場合はプレイヤーから数値分だけ離れた位置）
                if (this.CompareTag("Boss"))
                {
                    agent.stoppingDistance = 5.0f;
                }
                else
                {
                    agent.stoppingDistance = 2.5f;
                }

                AnimatorBoolReset();

                agent.speed = chaseSpeed;
                animator.SetBool("Run", true);


                //プレイヤーとの距離が十分近い場合攻撃状態へ遷移
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    AttackCheck();
                }

                //プレイヤーを見失った時
                if (ForGetPlayer())
                {
                    agent.ResetPath();
                    state = STATE.Caution;
                }


                break;

            //攻撃状態
            case STATE.Attack:

                //ゲームオーバー時は攻撃を解除
                if (GameManager.instance.state == GameState.GameOver)
                {
                    AnimatorBoolReset();
                    agent.ResetPath();

                    state = STATE.Caution;

                    return;
                }


                AnimatorBoolReset();

                animator.SetBool("Attack", true);
                transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

                //プレイヤーと距離が一定以上離れた場合は追跡状態へ移行
                if (DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.Chase;
                }


                break;

            //スキル攻撃（強攻撃）
            case STATE.SkillAttack:

                //ゲームオーバー時は攻撃を解除
                if (GameManager.instance.state == GameState.GameOver)
                {
                    AnimatorBoolReset();
                    agent.ResetPath();

                    state = STATE.Caution;

                    return;
                }


                AnimatorBoolReset();

                animator.SetBool("SkillAttack", true);
                transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

                //プレイヤーと距離が一定以上離れた場合は追跡状態へ移行
                if (DistanceToPlayer() > agent.stoppingDistance + 1)
                {
                    state = STATE.Chase;
                }

                break;

            //被ダメージ状態
            case STATE.Damage:

                //動きを止めてヒットモーション
                AnimatorBoolReset();
                animator.SetTrigger("TakeDamage");
                agent.ResetPath();
                agent.isStopped = true;

                //プレイヤーとの距離が十分近い場合攻撃状態へ遷移
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    AttackCheck();
                }
                //プレイヤーを見失った時
                else if (ForGetPlayer())
                {
                    agent.ResetPath();
                    state = STATE.Caution;
                }
                else
                {
                    state = STATE.Idle;
                }

                break;

            //死亡状態
            case STATE.Dead:

                //死亡モーショントリガ―
                animator.SetTrigger("Death");
 
                if (!isDead)
                {
                    //死亡状態フラグ更新
                    isDead = true;
                    //ヒットボックスコライダーを消す
                    boxCollider.enabled = false;

                    agent.ResetPath();
                    //移動をキャンセル
                    agent.isStopped = true;

                    AnimatorBoolReset();

                    isShowUI = false;
                    hpUI.SetActive(false);

                    //状態異常フラグ全て消す
                    isFlameDamage = false;
                    isFreezing = false;
                    isParalyzing = false;
                    isPoisoning = false;


                }

                break;

            //感電状態
            case STATE.Paralyze:

                //その場で停止
                AnimatorBoolReset();
                agent.ResetPath();

                if (currentHealth <= 0)
                {
                    isParalyzing = false;
                    state = STATE.Dead;

                }

                break;


        }
    }





    //アニメーター制御フラグリセット
    private void AnimatorBoolReset()
    {
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("SkillAttack", false);
    }

    //プレイヤーとの距離を図る関数
    float DistanceToPlayer()
    {
        if (GameManager.instance.state == GameState.GameOver)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(target.transform.position, transform.position);
    }

    //敵がプレイヤーを発見するかどうか判定する関数
    private bool CanSeePlayer()
    {
        if (DistanceToPlayer() < detectDistance)
        {
            return true;
        }

        return false;
    }

    //プレイヤーを見失う条件を決める関数
    private bool ForGetPlayer()
    {
        if (DistanceToPlayer() > lostDistance)
        {
            return true;
        }

        return false;
    }


    //通常攻撃かスキル攻撃かの判定関数
    private void AttackCheck()
    {
        //30%の確率でスキル発動
        if (Random.Range(0, 100) > skillRate)
        {
            state = STATE.Attack;
        }
        else
        {
            state = STATE.SkillAttack;
        }
    }

    //攻撃用のコライダーを非表示にする
    public void HiddenCollider()
    {
        hitCollider.enabled = false;

        if (hitCollider2 != null)
        {
            hitCollider2.enabled = false;
        }

        if (skillHitCollider != null)
        {
            skillHitCollider.enabled = false;
        }

    }


    //通常攻撃判定用コライダー制御関数（アニメーションで呼び出す）

    //攻撃コライダー表示
    public void ShowAttackCollider()
    {
        hitCollider.enabled = true;
    }

    //攻撃コライダー非表示
    public void HiddenAttackCollider()
    {
        hitCollider.enabled = false;
    }

    //攻撃コライダー2表示（種類によって二つ持つ）
    public void ShowAttackCollider2()
    {
        hitCollider2.enabled = true;
    }

    //攻撃コライダー2非表示
    public void HiddenAttackCollider2()
    {
        hitCollider2.enabled = false;
    }


    //強攻撃（スキル）判定用コライダー制御関数（アニメーションで呼び出す）

    //スキル用コライダー表示
    public void ShowSkillAttackCollider()
    {
        skillHitCollider.enabled = true;
    }

    //スキル用コライダー非表示
    public void HiddenSkillAttackCollider()
    {
        skillHitCollider.enabled = false;
    }


    //倒れたときに削除される（アニメーションで呼び出す）
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    //攻撃状態のフラグ管理（アニメーションで呼び出す）
    public void AttackStart()
    {
        isAttacking = true;
    }

    public void AttackEnd()
    {
        isAttacking = false;
    }



    //ステータス設定関数
    private void EnemyStatus()
    {
        //最大体力
        maxHealth = (int)(baseHealth + GameManager.instance.waveNum * healthGrowth);
        //攻撃力
        enemyAttackPower = (int)(enemyBaseAttackPower + GameManager.instance.waveNum * attackGrowth);
        //防御力
        enemyDefensePower = (int)(enemyBaseDefensePower + GameManager.instance.waveNum * defenseGrowth);

        //敵のステータスにばらつきを持たせる
        float randFactor = Random.Range(0.9f,1.2f);

        //乱数を適応した最終的なステータス
        maxHealth = (int)(maxHealth * randFactor);
        enemyAttackPower = (int)(enemyAttackPower * randFactor);
        enemyDefensePower = (int)(enemyDefensePower * randFactor);



    }

    //ノックバック関数
    private void KnockBack()
    {

        Vector3 backDir = -transform.forward.normalized;
        transform.DOMove(transform.position + backDir * knockBackDistance, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnStart(() => agent.ResetPath())
            .OnStart(() => agent.isStopped = true)
            .OnComplete(() => StartCoroutine("ResetNavi"));

    }

    //ノックバック後の処理
    IEnumerator ResetNavi()
    {
        yield return new WaitForSeconds(knockBackColdTime);
        if (!isDead)
        {
            agent.isStopped = false;
            state = STATE.Idle;
        }
        
        

    }

    private void OnTriggerEnter(Collider other)
    {

        if (!isHit)
        {
            //武器で攻撃された時の処理
            if (other.CompareTag("Weapon") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //燃焼効果付与チェック
                FlameCheck();

                //氷結効果付与チェック
                FreezeCheck();

                //感電効果付与チェック
                ParalyzingCheck();

                //毒効果付与チェック
                PoisonCheck();

                isHit = true;
                isShowUI = true;

                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);
                        
                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }


                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else if (!isAttacking)
                {
                    //ヒットモーションを敵のタイプによって発生確率を変える
                    HitMotionProcess();
                }

                //クリティカルヒット時SEのピッチを変更
                if (result.isCritical)
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.2f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);
                }
                else
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.0f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();

                enemySoundManager.PlaySE(EnemySoundType.hitSword);
            }

            //盾で攻撃された時
            if (other.CompareTag("Shield") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //燃焼効果付与チェック
                FlameCheck();

                //氷結効果付与チェック
                FreezeCheck();

                //感電効果付与チェック
                ParalyzingCheck();

                //毒効果付与チェック
                PoisonCheck();

                isHit = true;
                isShowUI = true;

                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);

                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }

                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else
                {
                    state = STATE.Damage;
                }

                KnockBack();

                //クリティカルヒット時SEのピッチを変更
                if (result.isCritical)
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.2f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);
                }
                else
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.0f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();

                enemySoundManager.PlaySE(EnemySoundType.hitShield);
            }

            //魔法で攻撃された時
            if (other.CompareTag("Magic") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //燃焼効果付与チェック
                FlameCheck();

                //氷結効果付与チェック
                FreezeCheck();

                //感電効果付与チェック
                ParalyzingCheck();

                //毒効果付与チェック
                PoisonCheck();

                isHit = true;
                isShowUI = true;

                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);

                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }


                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else if (!isAttacking)
                {
                    //ヒットモーションを敵のタイプによって発生確率を変える
                    HitMotionProcess();
                }

                //クリティカルヒット時SEのピッチを変更
                if (result.isCritical)
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.2f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);

                }
                else
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.0f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();

                enemySoundManager.PlaySE(EnemySoundType.hitMagic);

            }

            //メイジのスキルQで攻撃を受けた時
            if (other.CompareTag("MageSkillQ") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //燃焼効果付与
                isFlameDamage = true;

                //燃焼効果付与したときにタイマーセット
                flameEffectTimer = flameEffectTime;

                //燃焼効果付与中にもう一度付与された場合効果時間延長
                flameDamageTime = 5;



                isHit = true;
                isShowUI = true;

                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);

                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }

                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else
                {
                    state = STATE.Damage;
                }

                //クリティカルヒット時ヒットストップ変化
                if (result.isCritical)
                {
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);

                }
                else
                {
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();

                KnockBack();


            }

            //メイジのスキルEで攻撃を受けた時
            if (other.CompareTag("MageSkillE") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //氷結効果付与
                isFreezing = true;
                EnemyFreezing();

                isHit = true;
                isShowUI = true;

                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);

                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }

                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else
                {
                    state = STATE.Damage;
                }

                //クリティカルヒット時ヒットストップ変化
                if (result.isCritical)
                {
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);

                }
                else
                {
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();

                KnockBack();



            }

            //メイジのスキルRで攻撃を受けた時
            if (other.CompareTag("MageSkillR") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //感電効果付与
                isParalyzing = true;

                //タイマーセット
                paralyzingTimer = paralyzingTime;

                isHit = true;
                isShowUI = true;

                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);

                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }


                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else
                {
                    state = STATE.Damage;
                }

                //クリティカルヒット時SEのピッチを変更
                if (result.isCritical)
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.5f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);
                }
                else
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.1f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();
                enemySoundManager.PlaySE(EnemySoundType.hitThunder);

            }

            //メイジのスキルFで攻撃を受けた時
            if (other.CompareTag("MageSkillF") && currentHealth > 0)
            {
                //HPバーの表示時間リセット
                showUITimer = showUITime;

                //毒付与
                isPoisoning = true;

                poisonDamageTimer = poisonDamageTime;

                isHit = true;
                isShowUI = true;


                //攻撃を受ける
                DamageResult result = targetScript.EnemyTakeDamage(enemyDefensePower);

                currentHealth -= (int)result.damage;

                //ドレイン効果
                if (targetScript.isAbsorption)
                {
                    targetScript.currentHp += (int)(result.damage * targetScript.absorptionRate);

                    if (targetScript.maxHp < targetScript.currentHp)
                    {
                        targetScript.currentHp = targetScript.maxHp;
                    }
                }

                if (currentHealth <= 0)
                {
                    state = STATE.Dead;
                }
                else
                {
                    state = STATE.Damage;
                }
                //クリティカルヒット時SEのピッチを変更
                if (result.isCritical)
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.2f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.1f);
                }
                else
                {
                    enemySoundManager.audioSource.volume = 0.5f;
                    enemySoundManager.audioSource.pitch = 1.0f;
                    //ヒットストップ
                    HitStopManager.instance.HitStop(0.08f);
                }

                //画面揺れ
                GameManager.instance.OnHit();

                enemySoundManager.PlaySE(EnemySoundType.hitPoison);

            }


        }
    }


    //ヒットモーション（のけぞり）確率処理
    private void HitMotionProcess()
    {
        float rate = Random.Range(0, 100);

        if(rate <= hitMotionRate)
        {
            state = STATE.Damage;
        }

    }


    //燃焼効果付与チェック
    private void FlameCheck()
    {
        if (target.GetComponent<Player>().isFlame)
        {
            isFlameDamage = true;

            //燃焼効果付与したときにタイマーセット
            flameEffectTimer = flameEffectTime;

            //燃焼効果付与中にもう一度付与された場合効果時間延長
            flameDamageTime = 5;

            
        }
    }

    //燃焼効果ダメージ処理
    private void FlameDamege()
    {
        //ダメージ発生回数分発生した場合燃焼効果終了
        if (flameDamageTime <= 0)
        {
            isFlameDamage = false;

            //攻撃力を戻す
            enemyAttackPower = originAttackPower;

        }

        if (isFlameDamage)
        {
            //燃焼効果中攻撃力減少-30％
            if (enemyAttackPower != downAttackPower)
            {
                enemyAttackPower = downAttackPower;
            }
            

            //燃焼ダメージ発生までの時間処理(1s)
            flameEffectTimer -= Time.deltaTime;

            //燃焼ダメージ発生
            if (flameEffectTimer < 0)
            {
                currentHealth -= DamageOverTime;

                //タイマーリセット
                flameEffectTimer = flameEffectTime;

                //ダメージ発生回数減少
                flameDamageTime--;
            }
            
        }

    }


    //氷結状態チェック
    private void FreezeCheck()
    {
        //氷結確率20％(永続)
        if (target.GetComponent<Player>().isFreeze && !isFreezing)
        {
            float freezeRate = Random.Range(0.0f,1.0f);

            if (freezeRate <= debuffRate)
            {
                isFreezing = true;
                EnemyFreezing();
            }

        }
    }

    //氷結効果
    private void EnemyFreezing()
    {
        walkSpeed *= 0.7f;
        chaseSpeed *= 0.7f;

        enemyDefensePower = enemyDefensePower / 2;

    }

    //感電効果チェック
    private void ParalyzingCheck()
    {
        //感電確率は20％
        if (target.GetComponent<Player>().isParalyze && !isParalyzing)
        {
            float paralyzeRate = Random.Range(0.0f,1.0f);

            if(paralyzeRate <= debuffRate)
            {
                isParalyzing = true;

                //攻撃コライダーを非表示
                HiddenCollider();

                //タイマーセット
                paralyzingTimer = paralyzingTime;
            }

        }

    }

    //感電効果処理
    private void EnemyParalyzing()
    {
        if (isParalyzing)
        {
            paralyzingTimer -= Time.deltaTime;

            if (paralyzingTimer < 0)
            {
                isParalyzing = false;

                state = STATE.Idle;
            }
            else
            {
                state = STATE.Paralyze;
            }

        }
    }

    //毒状態チェック
    private void PoisonCheck()
    {
        //毒付与確率20％
        if (target.GetComponent<Player>().isPoison && !isPoisoning)
        {
            float poisonRate = Random.Range(0.0f, 1.0f);

            if (poisonRate <= debuffRate)
            {
                isPoisoning = true;

                poisonDamageTimer = poisonDamageTime;
            }

        }
    }


    //毒ダメージ処理(毒は永続)
    private void PoisonDamage()
    {
        if (isPoisoning)
        {
            poisonDamageTimer -= Time.deltaTime;

            if (poisonDamageTimer < 0)
            {
                //ダメージ処理
                currentHealth -= DamageOverTime;

                //タイマーリセット
                poisonDamageTimer = poisonDamageTime;
            }
        }


    }

    //状態異常エフェクトの処理
    private void StatusAbnormalityEffect()
    {
        if (isFlameDamage && !flameEffect.activeSelf)
        {
            flameEffect.SetActive(true);
        }
        else if(!isFlameDamage)
        {
            flameEffect.SetActive(false);
        }

        if (isFreezing && !freezeEffect.activeSelf)
        {
            freezeEffect.SetActive(true);
        }
        else if(!isFreezing)
        {
            freezeEffect.SetActive(false);
        }


        if (isParalyzing && !paralyzeEffect.activeSelf)
        {
            paralyzeEffect.SetActive(true);
        }
        else if(!isParalyzing)
        {
            paralyzeEffect.SetActive(false);
        }

        if (isPoisoning && !poisonEffect.activeSelf)
        {
            poisonEffect.SetActive(true);
        }
        else if(!isPoisoning)
        {
            poisonEffect.SetActive(false);
        }
    }

    //攻撃を受けた時は移動が止まるので再び動かす関数（アニメーションで呼び出す）
    public void EnemyMove()
    {
        agent.isStopped = false;

        isAttacking = false;

        HiddenCollider();
    }

}
