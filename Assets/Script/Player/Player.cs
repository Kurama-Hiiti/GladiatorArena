using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEditor;
using UnityEngine;

public struct DamageResult
{
    public float damage;
    public bool isCritical;
}


public class Player : MonoBehaviour
{
    //最大体力
    public int maxHp;

    //現在体力
    public int currentHp;

    //最大SP
    public int maxSp;

    //現在SP
    public int currentSp;

    //プレイヤーの総攻撃力
    protected private int attackPower;

    //読み取り用攻撃力
    public int ROPlayerAttackPower => attackPower;

    //プレイヤーの総防御力
    protected private int defensePower;

    //読み取り専用防御力
    public int ROPlayerDefensePower => defensePower;

    //プレイヤーの総クリティカル率
    protected private float criticalRate;

    //読み取り専用クリティカル率
    public float ROPlayerCriticalRate => criticalRate;

    //クリティカル倍率
    protected private float criticalDamageRate = 1.2f;

    //プレイヤーの攻撃速度変更割合
    protected private float attackSpeedChangeRate;

    //読取り用　攻撃速度変更割合
    public float AttackSpeedChangeRate => attackSpeedChangeRate;


    //回避フラグ
    protected private bool isDodge;

    //ゲームオーバーフラグ
    protected private bool isDead;

    //所持金
    public int money;

    //初期所持金
    //[SerializeField]
    //protected private int baseMoney;

    //カメラ
    public Transform cam;

    //アニメーターの定義(継承先で取得)
    protected private Animator animator;

    //キャラクターコントローラ
    [SerializeField]
    protected private CharacterController cc;

    //キャラクターの移動速度
    protected private float playerMoveSpeed;

    //キャラの初期移動速度
    [SerializeField]
    protected private float basePlayerMoveSpeed;

    //移動速度変更割合
    private float playerMoveSpeedChangeRate;

    //読取り用　移動速度変更割合
    public float PlayerMoveSpeedChangeRate => playerMoveSpeedChangeRate;

    //重力
    [SerializeField]
    protected private float gravity;

    //連続攻撃可能フラグ
    protected private bool nextAttackFlag;

    //攻撃中フラグ
    protected private bool isAttacking;

    //スキル発動中フラグ
    protected private bool isSkillAttacking;

    //走行中フラグ
    //protected private bool isRunning;

    //idol状態フラグ
    //protected private bool isIdle;

    //アニメーションの状態格納
    protected private AnimatorStateInfo stateInfo;

    //読取り用アニメーションの状態
    public AnimatorStateInfo ROStateInfo => stateInfo;

    //猶予時間設定
    [SerializeField]
    protected private float bufferTime;

    //プレイヤーの操作スクリプト
    protected private PlayerMove move;

    //プレイヤーの回避スクリプト
    protected private PlayerDodge dodge;

    //プレイヤーの通常攻撃スクリプト
    protected private PlayerNormalAttack attack;

    protected private PlayerSkillAttack skillAttack;

    //回避に必要なSP
    [SerializeField]
    protected int dodgeCostSp;

    //SP自然回復時間
    [SerializeField]
    private float recoverySpTime;

    //SP自然回復タイマー
    private float recoverySpTimer;

    //HP自然回復時間
    [SerializeField]
    private float recoveryHpTime;

    //HP自然回復タイマー
    private float recoveryHpTimer;


    //SP自然回復量
    [SerializeField]
    private int recoverySp;

    //HP自然回復量
    [SerializeField]
    private int recoveryHp;

    [SerializeField]
    protected private CommonSoundManager commonSoundManager;

    //プレイヤーにダメージクールタイム　連続ヒット防止　ヒットアニメーション時間の+0.1～0.2s
    [SerializeField]
    private float damageBufferTime;

    private float damageBufferTimer;

    protected private bool isHit;


    //装備品データ格納リスト
    public List<ItemData> itemList = new List<ItemData>();

    //ポーションのデータ格納リスト
    public GameObject[] potionList = new GameObject[3];

    //装備品画像データ格納リスト
    public List<GameObject> itemImageList = new List<GameObject>();

    //プレイヤージョブデータ（ScriptableObject）
    public PlayerJobData data;


    //ポーションのデータ
    public ItemData potion1, potion2, potion3;

    //ポーションの識別を格納された位置で判別する
    [SerializeField]
    private Transform potion1Pos, potion2Pos, potion3Pos;

    //アニメーションのスピード変更のための変数

    //アニメーションスピードデータ
    [SerializeField]
    private AnimationSpeedData playerAnimationData;

    // 倍率（外部からセットする：1.0 = 100%）
    protected float speedMultiplier = 1.0f;

    // 最後に決定したベース速度（キャッシュから取得）
    protected float baseSpeed = 1.0f;

    // コルーチン参照（重なり防止のため保持）
    private Coroutine currentSpeedCoroutine;

    // AnimatorのSpeedパラ名（必要に応じて変更）
    [SerializeField] private string animatorSpeedParam = "Speed";



    //特殊効果フラグ

    //燃焼効果
    public bool isFlame;

    //氷結効果
    public bool isFreeze;

    //感電(麻痺)効果
    public bool isParalyze;

    //毒効果
    public bool isPoison;


    //スキルによる攻撃力上昇率
    private float attackUp;

    //読取り用　攻撃力上昇率
    public float AttackUp => attackUp;


    //スキルによる防御力上昇率
    private float defenseUp;

    //読取り用　防御力上昇率
    public float DefenseUp => defenseUp;


    //ドレイン効果
    public bool isAbsorption;

    //吸収率
    public float absorptionRate = 0.2f;

    //自動回復効果
    protected private bool isAutoHeal;

    //自動回復効果フラグ　読取り用
    public bool IsAutoHeal => isAutoHeal;

    //SP消費量軽減率
    private float spCostDownRate;

    //SP消費軽減率　読取り用
    public float SpCostDownRate => spCostDownRate;


    //スキル連続発動防止カウント
    public int keyDownNum;


    //最大体力増加量
    private int overHealth;

    //最大SP増加量
    private int overSp;


    //ポーション効果スクリプト
    protected private PotionEffect potionScript;

    //バトル開始時のステータス(バトル中の能力上昇する前のステータス)
    private int originAttack;
    private int originDefense;


    //ポーション使用時のエフェクト
    [SerializeField]
    private GameObject healEffect;

    [SerializeField]
    private GameObject enhanceEffect;

    //防御しているかの判定
    protected private bool isBlocking;

    //防御成功時SP消費
    protected private int blockCost = 5;


    //被ダメージ時フラグ
    protected private bool isTakeDamege;

    //スキルレベルとスキルの攻撃力上昇率
    protected private int skillLevelQ;
    protected private int skillLevelE;
    protected private int skillLevelR;
    protected private int skillLevelF;

    //読取り用スキルレベル
    public int SkillLevelQ => skillLevelQ;
    public int SkillLevelE => skillLevelE;
    public int SkillLevelR => skillLevelR;
    public int SkillLevelF => skillLevelF;

    //上昇率
    protected private float skillRateQ;
    protected private float skillRateE;
    protected private float skillRateR;
    protected private float skillRateF;

    //最大スキルレベル
    private int maxSkillLevel = 5;
    public int MaxSkillLevel => maxSkillLevel;




    //初期化
    protected private void Init()
    {
        //初期値代入
        currentHp = maxHp;
        currentSp = maxSp;
        isDodge = false;
        isDead = false;
        money = data.FirstMoney;
        playerMoveSpeed = basePlayerMoveSpeed;

        //プレイヤーのスクリプトを参照する
        move = new PlayerMove(this);

        //プレイヤースクリプトを参照する
        dodge = new PlayerDodge(this);

        //プレイヤーのスクリプトを参照する
        attack = new PlayerNormalAttack(this);

        //プレイヤーのスクリプトを参照する
        skillAttack = new PlayerSkillAttack(this);

        //プレイヤーのスクリプトを参照する
        potionScript = new PotionEffect(this);

        attack.comboStep = 0;
        nextAttackFlag = false;

        isAttacking = false;

        cam = Camera.main.transform;
        animator = GetComponent<Animator>();

        //スキル使用に必要なコスト代入
        skillAttack.CostSpQ = data.SkillCostQ;
        skillAttack.CostSpE = data.SkillCostE;
        skillAttack.CostSpR = data.SkillCostR;
        skillAttack.CostSpF = data.SkillCostF;

        recoverySpTimer = recoverySpTime;

        damageBufferTimer = damageBufferTime;

        isHit = false;

        //基礎ステータス反映
        attackPower = data.BaseAttackPower;
        defensePower = data.BaseDefensePower;
        criticalRate = data.BaseCriticalRate;

        //スキルレベル設定
        skillLevelQ = 1;
        skillLevelE = 1;
        skillLevelR = 1;
        skillLevelF = 1;

        //スキルダメージ上昇率
        skillRateQ = 0;
        skillRateE = 0;
        skillRateR = 0;
        skillRateF = 0;


        //初期武器を格納
        AddStartItemList();

        //武器こみのステータス更新
        StartStatus();

        if (GameManager.instance.state != GameManager.GameState.Battle)
        {
            cc.enabled = false;
        }

    }



    //回避フラグ変更関数(回避アニメーションの終わりに呼び出す)
    protected private void DodgeFlagChange()
    {
        isDodge = false;
    }


    //通常攻撃コンボリセット(アニメーションでも呼び出す)
    protected private void ComboReset()
    {
        attack.comboStep = 0;

        nextAttackFlag = false;

        animator.SetInteger("ComboStep", attack.comboStep);

    }

    //通常攻撃のコンボ入力受付可能フラグ(アニメーションで呼び出す)
    public void NextAttackFlag()
    {
        nextAttackFlag = true;
    }



    //スキルのフラグを変更(アニメーションで呼び出す)
    protected private void SkillFlagFalse()
    {
        animator.SetBool("Skill1", false);
        animator.SetBool("Skill2", false);
        animator.SetBool("Skill3", false);
        animator.SetBool("Skill4", false);

        keyDownNum = 0;

    }




    //アニメーションの状態取得 + フラグ管理
    protected private void AnimationStateInfo()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        isAttacking = stateInfo.IsTag("Attack");

        isSkillAttacking = stateInfo.IsTag("Skill");

        //isRunning = stateInfo.IsTag("Run");

        //isIdle = stateInfo.IsTag("Idle");

        //モーションによるフラグが増えたらここに追加していく
    }


    //SP自然回復関数
    protected private void NaturalRecoverySP()
    {
        if (currentSp < maxSp)
        {
            recoverySpTimer -= Time.deltaTime;

            if (recoverySpTimer <= 0)
            {
                currentSp += recoverySp;

                recoverySpTimer = recoverySpTime;
            }
        }

    }


    //HP自動回復関数
    protected private void NaturalRecoveryHP()
    {
        //自動回復アイテムを持っている時
        if (isAutoHeal)
        {
            if (currentHp < maxHp)
            {
                recoveryHpTimer -= Time.deltaTime;

                if (recoveryHpTimer <= 0)
                {
                    currentHp += recoveryHp;

                    recoveryHpTimer = recoveryHpTime;
                }
            }
        }


    }

    //回避SE(アニメーションで使用)
    public void DodgeSE()
    {
        commonSoundManager.audioSource.volume = 0.5f;
        commonSoundManager.PlaySE(CommonSoundType.Dodge);
    }


    protected private void TakeDamageInterval()
    {
        if (isHit)
        {
            damageBufferTimer -= Time.deltaTime;

            if (damageBufferTimer < 0)
            {
                isHit = false;
                damageBufferTimer = damageBufferTime;
            }
        }
    }

    //プレイヤーのアイテムリストに初期装備を格納する
    private void AddStartItemList()
    {
        itemList.AddRange(data.StartWeaponDatas);

    }

    //プレイヤーに装備の初期ステータスを反映する
    private void StartStatus()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            attackPower += itemList[i].AttackPower;
            defensePower += itemList[i].DefensePower;
            criticalRate += itemList[i].CriticalRate;
        }
    }

    //プレイヤーステータス更新
    public void UpdatePlyaerStatus()
    {
        //一度初期に戻す
        attackPower = data.BaseAttackPower;
        defensePower = data.BaseDefensePower;
        criticalRate = data.BaseCriticalRate;

        //攻撃速度初期化
        speedMultiplier = 1.0f;
        attackSpeedChangeRate = 0;
        //移動速度初期化
        playerMoveSpeed = basePlayerMoveSpeed;
        playerMoveSpeedChangeRate = 0;

        //燃焼効果初期化
        isFlame = false;

        //氷結効果初期化
        isFreeze = false;

        //感電効果初期化
        isParalyze = false;

        //毒効果初期化
        isPoison = false;

        //攻撃力上昇率初期化
        attackUp = 0;

        //防御力上昇率初期化
        defenseUp = 0;

        //ドレイン効果初期化
        isAbsorption = false;

        //体力自動回復効果初期化
        isAutoHeal = false;

        //消費SP軽減率初期化
        spCostDownRate = 0;

        //消費SP初期化
        skillAttack.CostSpQ = data.SkillCostQ;
        skillAttack.CostSpE = data.SkillCostE;
        skillAttack.CostSpR = data.SkillCostR;
        skillAttack.CostSpF = data.SkillCostF;

        //最大体力増加量初期化
        overHealth = 0;

        //最大SP増加量初期化
        overSp = 0;

        //SP上限リセット
        maxSp = 100;
        currentSp = maxSp;

        //現在の体力の割合
        float healthRate = 1;

        //もしすでに最大体力が増加状態の場合
        if (100 < maxHp)
        {
            healthRate = (float)currentHp / (float)maxHp;

            maxHp = 100;

            currentHp = (int)(maxHp * healthRate);
        }
        else
        {
            maxHp = 100;
            healthRate = (float)currentHp / (float)maxHp;

        }

        if (potion1 == null)
        {
            potionList[0] = null;
        }
        if (potion2 == null)
        {
            potionList[1] = null;
        }
        if (potion3 == null)
        {
            potionList[2] = null;
        }


        for (int i = 0; i < itemList.Count; i++)
        {

            attackPower += itemList[i].AttackPower;
            defensePower += itemList[i].DefensePower;
            criticalRate += itemList[i].CriticalRate;
            attackSpeedChangeRate += itemList[i].SpeedUpRate;
            playerMoveSpeedChangeRate += itemList[i].MoveSpeedChangeRate;
            attackUp += itemList[i].AttackUpRate;
            defenseUp += itemList[i].DefenseUpRate;
            spCostDownRate += itemList[i].SpCostDown;

            overHealth += itemList[i].OverLimitHealth;

            overSp += itemList[i].OverLimitSp;

            if (itemList[i].IsFlameCheck)
            {
                isFlame = true;
            }

            if (itemList[i].IsFreezeCheck)
            {
                isFreeze = true;
            }

            if (itemList[i].IsParalyzeCheck)
            {
                isParalyze = true;
            }

            if (itemList[i].IsPoisonCheck)
            {
                isPoison = true;
            }

            if (itemList[i].IsAbsorptionCheck)
            {
                isAbsorption = true;
            }

            if (itemList[i].IsAutoHealCheck)
            {
                isAutoHeal = true;
            }


        }


        //攻撃速度が変更された場合
        if (attackSpeedChangeRate != 0)
        {
            if (1.0f < attackSpeedChangeRate)
            {
                attackSpeedChangeRate = 1.0f;
            }
            SetSpeedMultiplier(attackSpeedChangeRate);
        }

        //移動速度が変更された場合
        if (playerMoveSpeedChangeRate != 0)
        {
            ChangeMoveSpeed(playerMoveSpeedChangeRate);
        }

        //攻撃力上昇
        if (attackUp != 0)
        {
            float atUp = (1 + attackUp) * attackPower;

            attackPower = (int)atUp;

        }

        //防御力上昇
        if (defenseUp != 0)
        {
            float defUp = (1 + defenseUp) * defensePower;

            defensePower = (int)defUp;
        }


        //消費SP軽減
        if (spCostDownRate != 0)
        {

            skillAttack.CostSpQ = (int)(skillAttack.CostSpQ * (1 - spCostDownRate));
            skillAttack.CostSpE = (int)(skillAttack.CostSpE * (1 - spCostDownRate));
            skillAttack.CostSpR = (int)(skillAttack.CostSpR * (1 - spCostDownRate));
            skillAttack.CostSpF = (int)(skillAttack.CostSpF * (1 - spCostDownRate));


        }

        //最大体力増加
        if (overHealth != 0)
        {
            maxHp += overHealth;

            //現在のHPの割合計算
            float calculationHealth = (float)maxHp * healthRate;

            currentHp = (int)(calculationHealth);
        }

        //最大SP増加
        if (overSp != 0)
        {
            maxSp += overSp;

            currentSp = maxSp;
        }


    }

    //ポーションデータ格納処理
    public void PotionDataAddProcess(ItemData addPotion)
    {
        for (int i = 0; i < itemImageList.Count; i++)
        {
            if (itemImageList[i].name == addPotion.name)
            {

                if (itemImageList[i].GetComponent<ItemIndividualData>().itemSetPos == potion1Pos.position)
                {
                    potion1 = addPotion;

                    potionList[0] = itemImageList[i];


                }
                else if (itemImageList[i].GetComponent<ItemIndividualData>().itemSetPos == potion2Pos.position)
                {
                    potion2 = addPotion;

                    potionList[1] = itemImageList[i];

                }
                else if (itemImageList[i].GetComponent<ItemIndividualData>().itemSetPos == potion3Pos.position)
                {
                    potion3 = addPotion;

                    potionList[2] = itemImageList[i];

                }

            }
        }
    }



    //敵にヒット時に呼び出すダメージ計算関数
    public virtual DamageResult EnemyTakeDamage(int enemyDefense)
    {
        DamageResult result;

        float criticalJudge = Random.Range(0, 1f);

        //敵の防御率
        float defenseMultiplier = 100f / (100f + enemyDefense);

        //クリティカル
        if (criticalJudge <= criticalRate)
        {
            result.damage = attackPower * defenseMultiplier * criticalDamageRate;
            result.isCritical = true;

            
        }
        else //通常
        {
            result.damage = attackPower * defenseMultiplier;

            result.isCritical = false;
        }

        return  result;

    }


    //プレイヤーの死亡関数
    protected private void PlayerDeath()
    {
        if (currentHp <= 0 && !isDead)
        {
            currentHp = 0;
            isDead = true;
            animator.SetBool("isDead", true);
            GameManager.instance.state = GameManager.GameState.GameOver;
        }
    }

    public void GameOver()
    {
        UIManager.instance.ShowGameOverUI();
    }


    //キャラクターコントローラ管理
    protected private void CharacterConntrollerManager()
    {
        if (GameManager.instance.state != GameManager.GameState.Battle)
        {
            cc.enabled = false;
        }
        else
        {
            if (!cc.enabled)
            {
                cc.enabled = true;
            }
        }
    }

    //アニメーションのリセット(クリアしてショップへ戻る際にidol状態へ遷移する)
    public void AnimationReset()
    {
        //全てのブール値をfalseにする
        animator.SetBool("Run", false);
        animator.SetBool("isBlock", false);
        animator.SetInteger("ComboStep", 0);
        SkillFlagFalse();

        DodgeFlagChange();

        //idol状態
        animator.Play("Idle");

        

    }


    //-----------------------------------------
    //アニメーションのスピード変更処理


    //アニメーション再生の際に呼ぶ関数
    public void UpdateAnimationSpeed(string stateName)
    {

        if (animator == null) return;

        // ScriptableObjectから基礎速度を取得
        baseSpeed = playerAnimationData.GetBaseSpeed(stateName);

        //最終的な速度を計算
        float finalSpeed = baseSpeed * speedMultiplier;

        // 実際のアニメーション再生速度を反映
        animator.SetFloat(animatorSpeedParam, finalSpeed);
    }


    // 外部からスピード倍率を変更　攻撃速度が上がるアイテムを所持している時にこの関数で変数を変更する
    public void SetSpeedMultiplier(float multiplier)
    {
        //速度の増加倍率を代入（10％増 → 1.1f）
        speedMultiplier += multiplier;

        if (animator == null) return;

        // 現在再生中のアニメーションに即反映
        float finalSpeed = baseSpeed * speedMultiplier;
        animator.SetFloat(animatorSpeedParam, finalSpeed);
    }


    //移動速度変更関数
    private void ChangeMoveSpeed(float changeRate)
    {
        playerMoveSpeed *= (1 + changeRate);
    }


    //waveクリア時SP完全回復HP割合回復
    public void WaveClearHeal()
    {
        //SP回復
        currentSp = maxSp;

        //HP割合回復　2割
        float healHP = maxHp * 0.2f;

        currentHp += (int)healHP;

        if (maxHp < currentHp)
        {
            currentHp = maxHp;
        }

    }

    //ポーションの効果発動関数
    protected private void PotionActivation()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (potion1 != null)
            {


                potionScript.UsePotion(potion1);

                potion1 = null;

                Destroy(potionList[0]);

                potionList[0] = null;

            }
            else
            {
                //発動しないSE
                commonSoundManager.PlaySE(CommonSoundType.Beep);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (potion2 != null)
            {

                potionScript.UsePotion(potion2);

                potion2 = null;

                Destroy(potionList[1]);

                potionList[1] = null;


            }
            else
            {
                //発動しないSE
                commonSoundManager.PlaySE(CommonSoundType.Beep);
            }

        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (potion3 != null)
            {

                potionScript.UsePotion(potion3);

                potion3 = null;

                Destroy(potionList[2]);

                potionList[2] = null;

            }
            else
            {
                //発動しないSE
                commonSoundManager.PlaySE(CommonSoundType.Beep);
            }

        }


    }


    //ポーションでの能力変更関数
    public void PotionUpdateStatus(float attackUpRate, float defenseUpRate)
    {

        attackPower = (int)(attackPower * (1 + attackUpRate));

        defensePower = (int)(defensePower * (1 + defenseUpRate));


    }
    
    //ポーションで上昇したステータスをリセットする
    public void PotionEffectReset()
    {
        attackPower = originAttack;

        defensePower = originDefense;

        itemImageList.RemoveAll(x => x == null);
    }

    public void OriginStatusSet()
    {
        originAttack = attackPower;

        originDefense = defensePower;
    }


    //ポーションエフェクト処理(回復)
    public void HealEffect()
    {
        commonSoundManager.PlaySE(CommonSoundType.Heal);
        healEffect.SetActive(true);
        Invoke("HiddenEffectHeal", 1.5f);
    }

    //ポーションエフェクト処理(強化)
    public void EnhanceEffect()
    {
        commonSoundManager.PlaySE(CommonSoundType.Buff);
        enhanceEffect.SetActive(true);
        Invoke("HiddenEffectEnhance", 1.5f);
    }

    private void HiddenEffectHeal()
    {
        healEffect.SetActive(false);
    }

    private void HiddenEffectEnhance()
    {
        enhanceEffect.SetActive(false);
    }


    //ガードするための関数
    protected private void Blocking()
    {
        //右クリックが押されていても攻撃モーション中はガード判定はない
        if (Input.GetMouseButton(1) && !isAttacking && !isSkillAttacking && blockCost < currentSp)
        {
            isBlocking = true;
            animator.SetBool("isBlock", true);
        }
        else
        {
            isBlocking = false;
            animator.SetBool("isBlock", false);
        }

    }

    //被ダメージ状態解除
    public void ReleaseTakeDamage()
    {
        isTakeDamege = false;
    }

    protected private  void PlayerDamageSE()
    {
        commonSoundManager.PlaySE(CommonSoundType.TakeDamage);
    }

    //ビープ音発生関数（スキル使用時にSPがたりない時）
    public void BeepSE()
    {
        commonSoundManager.PlaySE(CommonSoundType.Beep);
    }



    //スキルレベルアップ処理
    public void SkillQLevelUp()
    {
        if (skillLevelQ < maxSkillLevel)
        {
            skillLevelQ++;
            skillRateQ += 0.1f;
        }

    }

    public void SkillELevelUp()
    {
        if (skillLevelE < maxSkillLevel)
        {
            skillLevelE++;
            skillRateE += 0.1f;
        }

    }

    public void SkillRLevelUp()
    {
        if (skillLevelR < maxSkillLevel)
        {
            skillLevelR++;
            skillRateR += 0.1f;
        }

    }

    public void SkillFLevelUp()
    {
        if (skillLevelF < maxSkillLevel)
        {
            skillLevelF++;
            skillRateF += 0.1f;
        }

    }

}



