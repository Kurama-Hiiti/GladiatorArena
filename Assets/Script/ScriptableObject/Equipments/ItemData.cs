using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//アイテムのタイプ
public enum ItemType
{
    Weapon, Armor, Secondary, Helm, Glove, Boots,
    Accessory, Potion
}


//アイテムのレアリティ
public enum ItemRarity
{
    Common, Rare, Epic, Legendary, Unique, None
}

//アイテムを使用できるジョブタイプ
public enum JobType
{
    None, Normal,SwordMan, Mage,
}


[CreateAssetMenu(menuName = "ScriptableObject/ItemData")]
public class ItemData : ScriptableObject
{
    //装備品の種別
    public ItemType ItemType => _itemType;

    [Header("アイテムの種類")]
    [SerializeField]
    private ItemType _itemType = ItemType.Weapon;

    //武器の名称
    public string WeaponName => _weaponName;

    [SerializeField]
    private string _weaponName = "";

    //武器の攻撃力
    public int AttackPower => _attackPower;

    [SerializeField]
    private int _attackPower = 0;

    //防具の防御力
    public int DefensePower => _defensePower;

    [SerializeField]
    private int _defensePower = 0;

    //クリティカル率
    public float CriticalRate => _criticalRate;

    [SerializeField]
    private float _criticalRate = 0;

    //価値
    public int Value => _value;

    [SerializeField]
    private int _value = 0;


    //画像データ
    public GameObject WeaponImage => _weaponImage;

    [SerializeField]
    private GameObject _weaponImage = null;

    //レアリティ
    public ItemRarity Rarity => _rarity;

    [SerializeField]
    private ItemRarity _rarity = ItemRarity.Common;


    //ジョブの指定
    public JobType JobType => _jobType;

    [SerializeField]
    private JobType _jobType = JobType.Normal;

    //詳細テキスト
    public string DetailText => _detailText;

    [SerializeField]
    [TextArea(3, 10)]
    private string _detailText = "";


    //特殊効果系

    //攻撃速度向上率
    public float SpeedUpRate => _speedUpRate;

    [SerializeField]
    [Tooltip("攻撃速度")]
    private float _speedUpRate = 0;

    //移動速度変更率
    public float MoveSpeedChangeRate => _moveSpeedChangeRate;

    [SerializeField]
    [Tooltip("移動速度")]
    private float _moveSpeedChangeRate = 0;


    //燃焼効果フラグ
    public bool IsFlameCheck => _isFlameCheck;

    [SerializeField]
    [Tooltip("燃焼効果の有無")]
    private bool _isFlameCheck = false;


    //氷結効果フラグ
    public bool IsFreezeCheck => _isFreezeCheck;

    [SerializeField]
    [Tooltip("氷結効果の有無")]
    private bool _isFreezeCheck = false;

    //感電効果フラグ
    public bool IsParalyzeCheck => _isParalyzeCheck;

    [SerializeField]
    [Tooltip("感電効果の有無")]
    private bool _isParalyzeCheck = false;


    //毒効果フラグ
    public bool IsPoisonCheck => _isPoisonCheck;

    [SerializeField]
    [Tooltip("毒効果の有無")]
    private bool _isPoisonCheck = false;


    //スキルによる攻撃力上昇率
    public float AttackUpRate => _attackUpRate;

    [SerializeField]
    [Tooltip("攻撃力上昇率")]
    private float _attackUpRate = 0;

    //スキルによる防御力上昇率
    public float DefenseUpRate => _defenseUpRate;

    [SerializeField]
    [Tooltip("防御力上昇率")]
    private float _defenseUpRate = 0;


    //ドレイン効果
    public bool IsAbsorptionCheck => _isAbsorptionCheck;

    [SerializeField]
    private bool _isAbsorptionCheck = false;


    //自動回復効果
    public bool IsAutoHealCheck => _isAutoHealCheck;

    [SerializeField]
    private bool _isAutoHealCheck = false;


    //消費SP軽減率
    public float SpCostDown => _spCostDown;

    [SerializeField]
    private float _spCostDown = 0;

    //最大体力増加量
    public int OverLimitHealth => _overLimitHealth;

    [SerializeField]
    private int _overLimitHealth = 0;

    //最大SP増加量
    public int OverLimitSp => _overLimitSp;

    [SerializeField]
    private int _overLimitSp = 0;


    //ポーションでのステータス変更変数
    public float PotionDefenseUpRate => _potionDefenseUpRate;


    [Header("ポーション関係")]
    [SerializeField]
    private float _potionDefenseUpRate = 0;


    public float PotionAttackUpRate => _potionAttackUpRate;

    [SerializeField]
    private float _potionAttackUpRate = 0;


}
