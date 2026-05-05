using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    //プレイヤーのステータス画面処理

    //ジョブ名テキスト
    [SerializeField]
    private TextMeshProUGUI jobText;

    //HPテキスト
    [SerializeField]
    private TextMeshProUGUI healthText;

    //SPテキスト
    [SerializeField]
    private TextMeshProUGUI spText;

    //攻撃力テキスト
    [SerializeField]
    private TextMeshProUGUI attackText;

    //防御力テキスト
    [SerializeField]
    private TextMeshProUGUI defenseText;

    //クリティカル率テキスト
    [SerializeField]
    private TextMeshProUGUI criticalRateText;


    //剣士のスキルアイコン
    [SerializeField]
    private GameObject swordManSkillIcon;

    [SerializeField]
    private GameObject mageSkillIcon;

    //スキルの詳細テキスト
    [SerializeField]
    private TextMeshProUGUI skillDetailText;

    //武器スキルテキスト
    [SerializeField]
    private TextMeshProUGUI weaponSkillText;


    //発動中武器スキルテキストリスト
    private List<string> activeSkills = new List<string>();



    //スキル詳細表示のためのポインター関係

    //ショップのグラフィックレイキャスター（アイテム操作用）
    private GraphicRaycaster raycaster;

    //マウスポインターのデータ格納
    private PointerEventData pointerEventData;

    private EventSystem eventSystem;


    private void Start()
    {
        // Canvas から Raycaster を取得
        raycaster =　GetComponent<GraphicRaycaster>();

        // EventSystem をシーン内から取得
        eventSystem = EventSystem.current;
    }


    private void Update()
    {
        if (this.enabled)
        {
            if (Input.GetMouseButton(0))
            {
                // マウス位置を基準に PointerEventData を生成
                pointerEventData = new PointerEventData(eventSystem)
                {
                    position = Input.mousePosition
                };

                // Raycast 結果を格納するリスト
                List<RaycastResult> results = new List<RaycastResult>();


                // Raycast 実行
                raycaster.Raycast(pointerEventData, results);

                if (GameManager.instance.state == GameManager.GameState.CharactorSelect)
                {
                    // スキル詳細表示
                    foreach (RaycastResult result in results)
                    {
                        if (result.gameObject.CompareTag("SkillQ"))
                        {
                            skillDetailText.text = UIManager.instance.SelectPlayer.data.SkillDetailQ.ToString();
                        }
                        else if (result.gameObject.CompareTag("SkillE"))
                        {
                            skillDetailText.text = UIManager.instance.SelectPlayer.data.SkillDetailE.ToString();
                        }
                        else if (result.gameObject.CompareTag("SkillR"))
                        {
                            skillDetailText.text = UIManager.instance.SelectPlayer.data.SkillDetailR.ToString();
                        }
                        else if (result.gameObject.CompareTag("SkillF"))
                        {
                            skillDetailText.text = UIManager.instance.SelectPlayer.data.SkillDetailF.ToString();
                        }


                    }
                }
                else if (GameManager.instance.state == GameManager.GameState.Shop || GameManager.instance.state == GameManager.GameState.Menu)
                {
                    // スキル詳細表示
                    foreach (RaycastResult result in results)
                    {
                        if (result.gameObject.CompareTag("SkillQ"))
                        {
                            skillDetailText.text = GameManager.instance.player.GetComponent<Player>().data.SkillDetailQ.ToString();
                        }
                        else if (result.gameObject.CompareTag("SkillE"))
                        {
                            skillDetailText.text = GameManager.instance.player.GetComponent<Player>().data.SkillDetailE.ToString();
                        }
                        else if (result.gameObject.CompareTag("SkillR"))
                        {
                            skillDetailText.text = GameManager.instance.player.GetComponent<Player>().data.SkillDetailR.ToString();
                        }
                        else if (result.gameObject.CompareTag("SkillF"))
                        {
                            skillDetailText.text = GameManager.instance.player.GetComponent<Player>().data.SkillDetailF.ToString();
                        }

                    }
                }


            }
        }
    }






    //ステータス画面更新関数
    public void StatusUIUpdate()
    {
        //ステートによって読み取る先を変更する
        if (GameManager.instance.state == GameManager.GameState.CharactorSelect)
        {
            switch (UIManager.instance.SelectPlayer.data.JobType)
            {
                case JobType.SwordMan:
                    SwordManStatusUI(UIManager.instance.SelectPlayer);
                    break;

                case JobType.Mage:
                    MageStatusUI(UIManager.instance.SelectPlayer);
                    break;
            }
        }
        else if (GameManager.instance.state == GameManager.GameState.Shop || GameManager.instance.state == GameManager.GameState.Menu)
        {
            switch (GameManager.instance.player.GetComponent<Player>().data.JobType)
            {
                case JobType.SwordMan:
                    SwordManStatusUI(GameManager.instance.player.GetComponent<Player>());
                    break;

                case JobType.Mage:
                    MageStatusUI(GameManager.instance.player.GetComponent<Player>());
                    break;

            }
        }

    }


    //剣士用のステータス画面表示処理
    private void SwordManStatusUI(Player player)
    {
        //ジョブタイプテキスト更新
        jobText.text = "剣士";

        //HPテキスト更新
        healthText.text = "HP:" + player.currentHp + "/" + player.maxHp;

        //SPテキスト更新
        spText.text = "SP:" + player.currentSp + "/" + player.maxSp;

        //攻撃力テキスト更新
        attackText.text = "攻撃力:" + player.ROPlayerAttackPower.ToString();

        //防御力テキスト更新
        defenseText.text = "防御力:" + player.ROPlayerDefensePower.ToString();

        //クリティカル率テキスト更新
        if (1 <= player.ROPlayerCriticalRate)
        {
            criticalRateText.text = "クリティカル率:" + 100 + "%";
        }
        else
        {
            criticalRateText.text = "クリティカル率:" + (player.ROPlayerCriticalRate * 100).ToString("0") + "%";
        }

        //剣士のスキルアイコン表示
        swordManSkillIcon.SetActive(true);

        //魔術師のスキルアイコン非表示
        mageSkillIcon.SetActive(false);

        //発動中のスキルの確認
        ActiveSkillCheck(player);


        //発動中スキル表示
        UpdateWeaponSkillTextUI();


        skillDetailText.text = string.Empty;
    }

    //メイジ用のステータス画面表示処理
    private void MageStatusUI(Player player)
    {
        //ジョブタイプテキスト更新
        jobText.text = "魔術師";

        //HPテキスト更新
        healthText.text = "HP:" + player.currentHp + "/" + player.maxHp;

        //SPテキスト更新
        spText.text = "SP:" + player.currentSp + "/" + player.maxSp;

        //攻撃力テキスト更新
        attackText.text = "攻撃力:" + player.ROPlayerAttackPower.ToString();

        //防御力テキスト更新
        defenseText.text = "防御力:" + player.ROPlayerDefensePower.ToString();

        //クリティカル率テキスト更新
        if (1 <= player.ROPlayerCriticalRate)
        {
            criticalRateText.text = "クリティカル率:" + 100 + "%";
        }
        else
        {
            criticalRateText.text = "クリティカル率:" + (player.ROPlayerCriticalRate * 100).ToString("0") + "%";
        }
        

        //魔術師のスキルアイコン表示
        mageSkillIcon.SetActive(true);

        //剣士のスキルアイコン非表示
        swordManSkillIcon.SetActive(false);

        //発動中のスキルの確認
        ActiveSkillCheck(player);


        //発動中スキル表示
        UpdateWeaponSkillTextUI();


        skillDetailText.text = string.Empty;
    }


    private void UpdateWeaponSkillTextUI()
    {
        if (activeSkills.Count == 0)
        {
            weaponSkillText.text = "スキルなし";
        }
        else
        {
            weaponSkillText.text = string.Join("\n", activeSkills);
        }


        
    }


    private void AddSkill(string skillName)
    {
        if (!activeSkills.Contains(skillName))
        {
            activeSkills.Add(skillName);
        }
    }



    private void ActiveSkillCheck(Player player)
    {
        activeSkills.Clear();
        weaponSkillText.text = string.Empty;


        if (player.isFlame)
        {
            AddSkill("燃焼");
        }


        if (player.isFreeze)
        {
            AddSkill("氷結");
        }


        if (player.isParalyze)
        {
            AddSkill("感電");
        }

        if (player.isPoison)
        {
            AddSkill("毒");
        }

        if (player.isAbsorption)
        {
            AddSkill("ドレイン");
        }

        if (player.IsAutoHeal)
        {
            AddSkill("自動回復");
        }

        if (player.AttackUp != 0)
        {
            AddSkill("攻撃力+" + player.AttackUp * 100 + "%");
        }


        if (player.DefenseUp != 0)
        {
            AddSkill("防御力+" + player.DefenseUp * 100 + "%");
        }

        if (player.SpCostDownRate != 0)
        {
            AddSkill("SP消費-" + player.SpCostDownRate * 100 + "%");
        }

        if (player.AttackSpeedChangeRate < 0)
        {
            AddSkill("攻撃速度" + player.AttackSpeedChangeRate * 100 + "%");
        }
        else if (0 < player.AttackSpeedChangeRate)
        {
            AddSkill("攻撃速度+" + player.AttackSpeedChangeRate * 100 + "%");
        }


        if (player.PlayerMoveSpeedChangeRate < 0)
        {
            AddSkill("移動速度" + player.PlayerMoveSpeedChangeRate * 100 + "%");
        }
        else if (0 < player.PlayerMoveSpeedChangeRate)
        {
            AddSkill("移動速度+" + player.PlayerMoveSpeedChangeRate * 100 + "%");
        }

    }



}
