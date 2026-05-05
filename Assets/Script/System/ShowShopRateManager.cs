using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowShopRateManager : MonoBehaviour
{
    //シングルトン化
    public static ShowShopRateManager instance { get; private set; }

    [SerializeField]
    private ShopManager shopManager;

    [SerializeField]
    private GameObject popUpUI;

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
        //ポップアップ表示
        popUpUI.transform.SetAsLastSibling();
        popUpUI.SetActive(true);

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
