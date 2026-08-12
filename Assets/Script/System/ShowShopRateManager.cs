using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowShopRateManager : MonoBehaviour
{
    //ショップに陳列されるアイテムのレアリティのレート画面表示処理

    //シングルトン化
    public static ShowShopRateManager instance { get; private set; }

    [SerializeField]
    private ShopManager shopManager;

    //レート表示UI
    [SerializeField]
    private GameObject popUpUI;

    //レートテキスト
    [SerializeField]
    private TextMeshProUGUI commonRateText;
    [SerializeField]
    private TextMeshProUGUI rareRateText;
    [SerializeField]
    private TextMeshProUGUI epicRateText;
    [SerializeField]
    private TextMeshProUGUI legendaryRateText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    //アイテム出現確率表示
    public void ShowRatePopUp()
    {
        //ポップアップ表示　ポップアップ表示を要素の最後（最前）にする
        popUpUI.transform.SetAsLastSibling();
        popUpUI.SetActive(true);

        //テキスト更新
        commonRateText.text = "コモン:"+ shopManager.NowRate.common + "%";
        rareRateText.text = "レア:" + shopManager.NowRate.rare + "%";
        epicRateText.text = "エピック:" + shopManager.NowRate.epic + "%";
        legendaryRateText.text = "レジェンダリー:" + shopManager.NowRate.legendary + "%";

    }

    //アイテム出現確率非表示
    public void HiddenRatePopUp()
    {
        //ポップアップ非表示
        popUpUI.SetActive(false);
    }

}
