using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/PlayerJobData")]
public class PlayerJobData : ScriptableObject
{
    //ジョブの名前
    public string JobName => _jobName;
    [SerializeField]
    private string _jobName = "";

    //ジョブのタイプ
    public JobType JobType => _jobType;

    [SerializeField]
    private JobType _jobType = JobType.None;

    //最初の所持金
    public int FirstMoney => _firstMoney;

    [Header("初期所持金")]
    [SerializeField]
    private int _firstMoney = 0;


    //スキルの名称
    public string SkillNameQ => _skillNameQ;
    public string SkillNameE => _skillNameE;
    public string SkillNameR => _skillNameR;
    public string SkillNameF => _skillNameF;

    [Header("スキル名")]
    [SerializeField]
    private string _skillNameQ = "";
    [SerializeField]
    private string _skillNameE = "";
    [SerializeField]
    private string _skillNameR = "";
    [SerializeField]
    private string _skillNameF = "";


    //スキルの詳細
    public string SkillDetailQ => _skillDetailQ;
    public string SkillDetailE => _skillDetailE;
    public string SkillDetailR => _skillDetailR;
    public string SkillDetailF => _skillDetailF;

    [Header("スキル詳細")]
    [SerializeField]
    private string _skillDetailQ = "";
    [SerializeField]
    private string _skillDetailE = "";
    [SerializeField]
    private string _skillDetailR = "";
    [SerializeField]
    private string _skillDetailF = "";


    //基礎ステータス
    public int BaseAttackPower => _baseAttackPower;
    public int BaseDefensePower => _baseDefensePower;
    public float BaseCriticalRate => _baseCriticalRate;

    [Header("基礎ステータス")]
    [SerializeField]
    private int _baseAttackPower = 0;
    [SerializeField]
    private int _baseDefensePower = 0;
    [SerializeField]
    private float _baseCriticalRate = 0;

    //スキル攻撃による攻撃威力倍率
    public float SkillDamageRateQ => _skillDamageRateQ;
    public float SkillDamageRateE => _skillDamageRateE;
    public float SkillDamageRateR => _skillDamageRateR;
    public float SkillDamageRateF => _skillDamageRateF;


    [Header("スキルダメージ倍率")]
    [SerializeField]
    private float _skillDamageRateQ = 0f;
    [SerializeField]
    private float _skillDamageRateE = 0f;
    [SerializeField]
    private float _skillDamageRateR = 0f;
    [SerializeField]
    private float _skillDamageRateF = 0f;


    //スキル攻撃にかかるSP
    public int SkillCostQ => _skillCostQ;
    public int SkillCostE => _skillCostE;
    public int SkillCostR => _skillCostR;
    public int SkillCostF => _skillCostF;


    [Header("スキルコストSP")]
    [SerializeField]
    private int _skillCostQ = 0;
    [SerializeField]
    private int _skillCostE = 0;
    [SerializeField]
    private int _skillCostR = 0;
    [SerializeField]
    private int _skillCostF = 0;



    //初期装備格納
    public List<ItemData> StartWeaponDatas => _startWeaponDatas;

    [Header("初期装備")]
    [SerializeField]
    private List<ItemData> _startWeaponDatas = null;

}
