using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEditor;
using TMPro;
using DG.Tweening;
using System.Linq;

public class PlayerUI : MonoBehaviour
{
    //プレイヤー取得
    private GameObject player;

    //プレイヤーのスクリプト取得
    private Player playerScript;

    //HP取得
    private IntReactiveProperty playerHp = new IntReactiveProperty(0);

    //SP取得
    private IntReactiveProperty playerSp = new IntReactiveProperty(0);

    //ポーションの所持数取得
    private StringReactiveProperty potionState = new StringReactiveProperty("");

    //スキル使用時のアニメーションの変化
    private IntReactiveProperty animatorHash = new IntReactiveProperty(0); 


    //HPバーの画像
    [SerializeField]
    private Image hpBar;

    //SPバーの画像
    [SerializeField]
    private Image spBar;

    //HP、SPバーの遷移時間
    [SerializeField]
    private float barDuration;

    //HPのテキスト
    [SerializeField]
    private TextMeshProUGUI hpText;

    //SPのテキスト
    [SerializeField]
    private TextMeshProUGUI spText;

    //ポーションの画像リスト
    [SerializeField]
    private GameObject[] potionImage;

    //持っているポーションの表示位置
    [SerializeField]
    private GameObject potionPos1, potionPos2, potionPos3;

    //生成したポーション
    private GameObject potion1, potion2, potion3;

    //生成したポーションのサイズ
    private Vector3 potionSize = new Vector3(0.8f, 0.8f, 0.8f);

    //生成したポーションの位置
    private Vector3 potionResetPos = new Vector3(0, 0, 0);

    //剣士のバトル中に表示するスキルアイコン
    [SerializeField]
    private List<GameObject> swordManSkillIcon;

    [SerializeField]
    private List<GameObject> mageSkillIcon;

    //スキル使用時にスキルを隠す画像
    [SerializeField]
    private Image useSkillIcon;

    //ハッシュ値の定義
    private static readonly int skillQHash = Animator.StringToHash("SkillQ");
    private static readonly int skillEHash = Animator.StringToHash("SkillE");
    private static readonly int skillRHash = Animator.StringToHash("SkillR");
    private static readonly int skillFHash = Animator.StringToHash("SkillF");

    private void Awake()
    {
         player = GameObject.FindWithTag("Player");

        playerScript = player.GetComponent<Player>();


    }

    private void Start()
    {
        if (GameManager.instance.state == GameManager.GameState.Battle)
        {
            Init();

            playerHp
                .ObserveEveryValueChanged(hp => playerHp.Value = playerScript.currentHp)
                .Subscribe(hp => HpUIManagement(hp));

            playerSp
                .ObserveEveryValueChanged(sp => playerSp.Value = playerScript.currentSp)
                .Subscribe(sp => SpUIManagement(sp));

            potionState
                .ObserveEveryValueChanged(name => GetPotionState())
                .Subscribe(name => HavePotionDisplay());

            animatorHash
                .ObserveEveryValueChanged(currentHash => animatorHash.Value = playerScript.ROStateInfo.shortNameHash)
                .Subscribe(currentHash => ShowUseSkillIcon(currentHash));
        }


    }

    private void Init()
    {
        int maxHp = playerScript.maxHp;

        int currentHp = playerScript.currentHp;

        int maxSp = playerScript.maxSp;

        int currentSp = playerScript.currentSp;

        hpText.text = currentHp.ToString() + "/" + maxHp.ToString();

        hpBar.fillAmount = (float)currentHp / maxHp;

        spText.text = currentSp.ToString() + "/" + maxSp.ToString();

        spBar.fillAmount = (float)currentSp / maxSp;

        //スキルアイコン表示
        ShowSkillIcon();



    }

    private void HpUIManagement(int hp)
    {
        int maxHp = playerScript.maxHp;

        hpText.text = hp + "/" + maxHp.ToString();

        float targetValue = (float)hp / (float)maxHp;

        hpBar.DOFillAmount(targetValue, barDuration);

    }

    private void SpUIManagement(int sp)
    {
        int maxSp = playerScript.maxSp;

        spText.text = sp + "/" + maxSp.ToString();

        float targetValue = (float)sp / (float)maxSp;

        spBar.DOFillAmount(targetValue, barDuration);
    }


    //potionList の内容を文字として取得（監視用）
    private string GetPotionState()
    {
        // null は "none" として扱う
        return string.Join(",", playerScript.potionList
            .Select(x => x == null ? "none" : x.name));
    }



    //持っているポーションの画像を表示
    private void HavePotionDisplay()
    {

        //ポーション所持リストないをチェック
        for (int i = 0; i < playerScript.potionList.Length; i++)
        {
            //リスト内にある場合
            if (playerScript.potionList[i] != null)
            {
                //生成するポーションを決定
                GameObject item = potionImage.FirstOrDefault(x => x.name == playerScript.potionList[i].name);

                //リストの1番目の時
                if (i == 0)
                {
                    //すでに格納されている場合は一度削除
                    if (potion1 != null)
                    {
                        Destroy(potion1);
                        potion1 = null;
                    }

                    //生成
                    potion1 = Instantiate(item);

                    //生成したオブジェクトの位置、サイズ、生成位置（子要素にする）の調整
                    potion1.transform.position = potionPos1.transform.position;

                    potion1.transform.SetParent(potionPos1.transform,false);

                    potion1.transform.localScale = potionSize;

                    potion1.transform.localPosition = potionResetPos;

                }
                //リストの2番目の時
                else if (i == 1)
                {
                    if (potion2 != null)
                    {
                        Destroy(potion2);
                        potion2 = null;
                    }

                    potion2 = Instantiate(item);

                    potion2.transform.SetParent(potionPos2.transform, false);

                    potion2.transform.localScale = potionSize;

                    potion2.transform.localPosition = potionResetPos;
                }
                //リストの3番目の時
                else if (i == 2)
                {
                    if (potion3 != null)
                    {
                        Destroy(potion3);
                        potion3 = null;
                    }

                    potion3 = Instantiate(item);

                    potion3.transform.SetParent(potionPos3.transform, false);

                    potion3.transform.localScale = potionSize;

                    potion3.transform.localPosition = potionResetPos;
                }

            }
            else//リスト内にない場合（使用した時など）オブジェクトを削除する
            {
                if (i == 0)
                {
                    if (potion1 != null)
                    {
                        Destroy(potion1);
                        potion1 = null;
                    }
                }

                else if (i == 1)
                {
                    if (potion2 != null)
                    {
                        Destroy(potion2);
                        potion2 = null;
                    }
                }

                else if (i == 2)
                {
                    if (potion3 != null)
                    {
                        Destroy(potion3);
                        potion3 = null;
                    }
                }
            }
        }
    }


    //スキルアイコン表示
    private void ShowSkillIcon()
    {
        switch (playerScript.data.JobType)
        {
            case JobType.SwordMan:

                foreach (GameObject icon in swordManSkillIcon)
                {
                    icon.SetActive(true);

                }

                break;

            case JobType.Mage:

                foreach (GameObject icon in mageSkillIcon)
                {
                    icon.SetActive(true);
                }

                break;
        }
    }

    //プレイヤーのスキルアニメーションの最初に実行
    private void ShowUseSkillIcon(int hash)
    {

        switch (hash)
        {
            case var _ when hash == skillQHash:

                useSkillIcon.transform.position = swordManSkillIcon[0].transform.position;
                useSkillIcon.fillAmount = 1;
                useSkillIcon.enabled = true;

                DecreaseFill(useSkillIcon,playerScript.ROStateInfo.length);


                break;
            case var _ when hash == skillEHash:

                useSkillIcon.transform.position = swordManSkillIcon[1].transform.position;
                useSkillIcon.fillAmount = 1;
                useSkillIcon.enabled = true;

                DecreaseFill(useSkillIcon, playerScript.ROStateInfo.length);

                break;
            case var _ when hash == skillRHash:

                useSkillIcon.transform.position = swordManSkillIcon[2].transform.position;
                useSkillIcon.fillAmount = 1;
                useSkillIcon.enabled = true;

                DecreaseFill(useSkillIcon, playerScript.ROStateInfo.length);

                break;
            case var _ when hash == skillFHash:

                useSkillIcon.transform.position = swordManSkillIcon[3].transform.position;
                useSkillIcon.fillAmount = 1;
                useSkillIcon.enabled = true;

                DecreaseFill(useSkillIcon,playerScript.ROStateInfo.length);

                break;
        }
    }

    private void DecreaseFill(Image targetImage, float time)
    {
        // 2秒かけて fillAmount を 0 にする
        targetImage.DOFillAmount(0f, time).SetLink(targetImage.gameObject)
            .OnComplete(() => targetImage.enabled = false);
    }


}
