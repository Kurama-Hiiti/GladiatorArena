using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.Progress;
#endif

public class RarityRate
{
    public float common;
    public float rare;
    public float epic;
    public float legendary;

}

//public struct HoldItem
//{
//    public int listNum;
//    public ItemData holdItemData;
//    public GameObject holdItemImageObj;
//}

[System.Serializable]
public class ShopSlot
{
    public Transform pos;         // 生成場所
    public TextMeshProUGUI priceText;       // 価格表示テキスト
    public ItemData currentItem;  // 今置かれているデータ
    public GameObject currentObj; // 今表示されているオブジェクト
    public GameObject holdIcon;   //ホールド時に表示されるアイコン
    public bool isHold;           // ホールド中か
}

public class SaveShopSlotData
{
    public ItemData saveItem;  // セーブされた置かれているデータ
    public GameObject saveObj; // セーブされた表示されているオブジェクト
    public bool isSaveHold;    // ホールド中か
}

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private ItemDataBase itemDataBase;

    //ショップスロットデータのリスト
    public List<ShopSlot> shopSlots = new List<ShopSlot>();

    //セーブしたショップスロットデータ
    private List<SaveShopSlotData> saveSlots = new List<SaveShopSlotData>();

    //アイテムの出現確率
    private List<RarityRate> rates = new()
    {
        new RarityRate { common = 90, rare = 10, epic =  0,  legendary =  0},
        new RarityRate { common = 80, rare = 18, epic =  2,  legendary =  0},
        new RarityRate { common = 70, rare = 25, epic =  5,  legendary =  0},
        new RarityRate { common = 60, rare = 30, epic =  8,  legendary =  2},
        new RarityRate { common = 45, rare = 40, epic = 10,  legendary =  5},
        new RarityRate { common = 40, rare = 35, epic = 15,  legendary = 10},
        new RarityRate { common = 35, rare = 30, epic = 20,  legendary = 15},
        new RarityRate { common = 30, rare = 25, epic = 25,  legendary = 20},
        new RarityRate { common = 25, rare = 25, epic = 25,  legendary = 25},
    };


    //現在の出現確率格納
    private RarityRate nowRate;

    //読み込み用アイテム出現確率
    public RarityRate NowRate => nowRate;

    //出現確率チェック
    private bool isRateCheck;

    //ショップスロット位置
    [SerializeField]
    private Transform[] shopSlotPos;

    //ショップに陳列されているアイテムのデータリスト
    private List<ItemData> shopItems = new();

    //ショップに陳列されているアイテムの画像データ
    //private List<GameObject> shopItemImage = new();

    //プレイヤーのジョブタイプ
    private JobType playerJobType;

    //ショップにアイテムを陳列するフラグ
    private bool isShowItem;

    //ショップのキャンバス
    [SerializeField]
    private Canvas shopCanvas;

    //ショップのグラフィックレイキャスター（アイテム操作用）
    private GraphicRaycaster raycaster;

    //マウスポインターのデータ格納
    private PointerEventData pointerEventData;

    private EventSystem eventSystem;

    //アイテムクリック判定
    private bool isItemCatch;

    //クリックしたアイテムを格納
    private GameObject catchItem;

    //選択したアイテムの初期位置
    private Vector3 catchItemOriginPos;

    //プレイヤーのスクリプト
    private Player playerScript;


    //ショップに陳列されているアイテムの価格表示
    [SerializeField]
    private List<TextMeshProUGUI> shopMoneyTextList;

    //アイテム詳細テキスト
    [SerializeField]
    private TextMeshProUGUI itemDetailText;

    //ラウンドテキスト
    [SerializeField]
    private TextMeshProUGUI waveText;

    //売却価格テキスト
    [SerializeField]
    private TextMeshProUGUI sellValueText;

    //ユニークアイテム出現ラウンド
    private int uniqueItemWave = 5;

    //ユニークアイテム出現フラグ
    private bool isUniqueShop;

    //ユニークアイテムのみが陳列されている時のフラグ
    private bool isShowUniqueItem;

    //ショップ入店ごとのリロール回数
    private int rerollCount;

    //リロール価値が増加するリロール回数
    private int limitRerollCount = 4;

    //初期のリロールに必要なお金
    private int rerollMoney = 1;

    //所定の回数以上の場合のリロールに必要なお金
    private int increasedRerollMoney = 2;

    //リロールテキスト
    [SerializeField]
    private TextMeshProUGUI rerollText;

    //サウンドマネージャー
    [SerializeField]
    private CommonSoundManager soundManager;

    //ユニークアイテム用追加マネー
    private int uniqueItemMoney = 10;

    //ホールドしたアイテムのデータ格納リスト
    //private List<HoldItem> holdItems = new();

    //ホールドしたいアイテムを格納
    private GameObject holdItem;

    //ホールドしたいアイテムのデータ格納
    //private HoldItem holdItemStruct;

    //ショップに並んでいるアイテムデータリスト
    private HashSet<ItemData> selectedItems = new();

    //スキルレベルアップボタン
    [SerializeField]
    private Button skillLevelUpButtonQ;
    [SerializeField]
    private Button skillLevelUpButtonE;
    [SerializeField]
    private Button skillLevelUpButtonR;
    [SerializeField]
    private Button skillLevelUpButtonF;

    //スキルレベルテキスト
    [SerializeField]
    private TextMeshProUGUI skillLevelTextQ;
    [SerializeField]
    private TextMeshProUGUI skillLevelTextE;
    [SerializeField]
    private TextMeshProUGUI skillLevelTextR;
    [SerializeField]
    private TextMeshProUGUI skillLevelTextF;

    //スキルレベルアップ価格
    private int skillUpValue = 5;

    //スキルアップ詳細表示フラグ
    private bool isShowSkillUpText;


    void Start()
    {
        // Canvas から Raycaster を取得
        raycaster = shopCanvas.GetComponent<GraphicRaycaster>();

        // EventSystem をシーン内から取得
        eventSystem = EventSystem.current;

        rerollText.text = "リロール(-" + rerollMoney + ")";

        skillLevelTextQ.text = "Lv.1";
        skillLevelTextE.text = "Lv.1";
        skillLevelTextR.text = "Lv.1";
        skillLevelTextF.text = "Lv.1";
    }

    private void Update()
    {
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            //ジョブタイプチェック
            PlayerJobTypeCheck();


            //ショップに陳列されるレアリティのチェック
            if (!isRateCheck)
            {
                nowRate = GetRate(GameManager.instance.waveNum);
                isRateCheck = true;

                //現在のラウンド数表示
                waveText.text =  "Round " + GameManager.instance.waveNum.ToString() + " / " + GameManager.instance.maxWave.ToString();

                //リロールテキスト変更
                rerollText.text = "リロール(-" + rerollMoney + ")";

            }
           
            //ショップに装備陳列
            if (!isShowItem)
            {
                if (GameManager.instance.waveNum == uniqueItemWave && !isUniqueShop)
                {
                    //ユニークアイテム出現
                    GenerateUniqueItems();

                    //ユニークアイテム分のお金を加算
                    GameManager.instance.player.GetComponent<Player>().money += uniqueItemMoney;
                    //お金のUI変更
                    UIManager.instance.ChangeMoney();


                    isUniqueShop = true;
                    isShowUniqueItem = true;

                }
                else 
                {
                    GenerateShopItems();
                }
                
                isShowItem = true;

            }



            //ショップ内のアイテム操作

            if (Input.GetMouseButton(0) && !isItemCatch && GameManager.instance.isClick)
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

                // アイテム操作
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag("Item"))
                    {
                        catchItem = result.gameObject;

                        //クリックしたアイテムを最前面に表示
                        catchItem.transform.SetParent(shopCanvas.transform);
                        catchItem.transform.SetAsLastSibling();

                        //クリックしたアイテムの初期位置
                        catchItemOriginPos = catchItem.transform.position;
                        isItemCatch = true;

                        //クリックしたアイテムの詳細表示
                        ShowItemDetailText();

                    }
                    else if (result.gameObject.CompareTag("SkillUp"))
                    {
                        //一度詳細テキストをクリアする
                        itemDetailText.text = string.Empty;
                        itemDetailText.text = "スキルの攻撃力を上昇させる　最大レベル5";
                        isShowSkillUpText = true;
                    }

                }

                //クリックしたときになんのアイテムも選択されていなければ詳細文を消す
                if (!isItemCatch && !isShowSkillUpText)
                {
                    itemDetailText.text = string.Empty;

                }

                isShowSkillUpText = false;
            }
            else if (Input.GetMouseButton(0) && isItemCatch && catchItem != null)//クリックしている間はマウスポインターにアイテムが追従する
            {
                catchItem.transform.position = Input.mousePosition;

                //アイテムを所持している且つ売却スペースへドラックした場合
                if (catchItem.GetComponent<ItemIndividualData>().isSell && IsHaveItem())
                {
                    //売却価格表示
                    SellItemValueText();
                }
                else
                {
                    sellValueText.text = string.Empty;
                }
            }
            else if (Input.GetMouseButtonUp(0) && isItemCatch)//クリックを離したとき
            {
                isItemCatch = false;

                if (catchItem != null)
                {

                    //アイテムのクリックを離したときに配置するのか判定
                    if (IsHaveItem() && catchItem.GetComponent<ItemIndividualData>().isSell)//所持していて売却スペースにある場合
                    {
                        ItemSell();

                    }
                    else if (IsHaveItem())//アイテムを所持している時
                    {
                        HaveItemMove();

                    }
                    else if (!IsHaveItem() && IsHaveMoney())//所持していないアイテムかつ選択したアイテムの値段以上のお金を所持している場合
                    {
                        ItemBuy();
                    }
                    else
                    {
                        catchItem.transform.position = catchItemOriginPos;
                    }
                    
                    catchItem = null;
                }

            }


            //アイテムのホールド処理
            if (Input.GetMouseButtonDown(1) && !isItemCatch && GameManager.instance.isClick)
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




                // アイテム操作
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag("Item"))
                    {
                        //ホールドしたいアイテムのオブジェクト
                        holdItem = result.gameObject;

                        // Raycastで当たったGameObjectから、どのスロットか特定する
                        var clickedSlot = shopSlots.Find(s => s.currentObj == holdItem);

                        if (clickedSlot != null)
                        {
                            //ユニークアイテムはホールドできない
                            if (clickedSlot.currentItem.Rarity != ItemRarity.Unique)
                            {
                                clickedSlot.isHold = !clickedSlot.isHold; // ホールド状態を反転
                                // ホールド用のエフェクト表示などの処理
                                if (clickedSlot.isHold) clickedSlot.holdIcon.SetActive(true);
                                if (!clickedSlot.isHold) clickedSlot.holdIcon.SetActive(false);


                                //ホールドSE
                                soundManager.PlaySE(CommonSoundType.Hold);
                            }

                        }

                        ////ホールドしたいアイテムがショップに陳列されているアイテムのリストの何番目かを判定
                        //for (int i = 0; shopItemImage.Count > i; i++) 
                        //{
                        //    if (shopItemImage[i].name == holdItem.name)
                        //    {
                        //        //ホールドしたいアイテムのリストの番号設定
                        //        holdItemStruct.listNum = i;
                        //    }
                        //}

                        ////ホールドしたいアイテムのItemDataを取得
                        //holdItemStruct.holdItemData = GetItemData(holdItem);

                        ////ホールドしたいアイテムのImage
                        //holdItemStruct.holdItemImageObj = holdItem;

                        ////ここでホールドしたいアイテムのリストに追加するかどうかを判定
                        //if (holdItems.Count == 0)
                        //{
                        //    holdItems.Add(holdItemStruct);

                        //    foreach (var item in holdItems)
                        //    {
                        //        Debug.Log($"中身を確認 -> Num: {item.listNum}, Data: {item.holdItemData}, Image: {item.holdItemImageObj}");
                        //    }

                        //    //ホールド表示をショップに示す

                        //    //ホールドSE

                        //}
                        //else
                        //{
                        //    //ホールドしたいアイテムのリストに同じものが無いとき追加
                        //    if (!holdItems.Contains(holdItemStruct))
                        //    {
                        //        holdItems.Add(holdItemStruct);

                        //        //ホールド表示をショップに示す

                        //        //ホールドSE


                        //        foreach (var item in holdItems)
                        //        {
                        //            Debug.Log($"中身を確認 -> Num: {item.listNum}, Data: {item.holdItemData}, Image: {item.holdItemImageObj}");
                        //        }
                        //    }
                        //    else　//リストに含まれていた場合は除外
                        //    {
                        //        holdItems.Remove(holdItemStruct);

                        //        //ホールド表示をショップから消す

                        //        //ホールド解除SE

                        //        foreach (var item in holdItems)
                        //        {
                        //            Debug.Log($"中身を確認 -> Num: {item.listNum}, Data: {item.holdItemData}, Image: {item.holdItemImageObj}");
                        //        }
                        //        Debug.Log(holdItems.Count);
                        //    }

                        //}



                    }

                }

            }


        }


    }

    //プレイヤーのジョブタイプチェック
    private void PlayerJobTypeCheck()
    {
        if(playerJobType == JobType.None)
        {
            playerJobType = GameManager.instance.player.GetComponent<Player>().data.JobType;

            playerScript = GameManager.instance.player.GetComponent<Player>();
        }
     
    }


    private void GenerateShopItems()
    {
        selectedItems.Clear();

        //リロールテキスト変更
        if (limitRerollCount <= rerollCount)
        {
            rerollText.text = "リロール(-" + increasedRerollMoney + ")";
        }

        foreach (var slot in shopSlots)
        {
            // ホールド中なら何もしない（維持）
            if (slot.isHold)
            {
                if (slot.currentObj != null) slot.currentObj.SetActive(true);
                continue;
            }

            // --- ここから新しいアイテムの抽選・生成 ---

            // 1. 古いアイテムを消す
            if (slot.currentObj != null) Destroy(slot.currentObj);

            // 2. 新しいアイテムを抽選（重複なしロジックは別途）
            ItemData nextItem = GetRandomItem();

            // 3. 生成してスロット情報を更新
            slot.currentItem = nextItem;
            slot.currentObj = Instantiate(nextItem.WeaponImage, slot.pos);
            slot.currentObj.name = nextItem.name;

            // 4. 価格表示の更新
            slot.priceText.text = nextItem.Value.ToString();

        }


    }

    private ItemData GetRandomItem()
    {
        float rateCheck = Random.Range(0, 100);

        List<ItemData> itemList = new();

        ItemRarity nowRarity;

        if (rateCheck < nowRate.common)
        {
            nowRarity = ItemRarity.Common;
        }
        else if (rateCheck < nowRate.common + nowRate.rare)
        {
            nowRarity = ItemRarity.Rare;
        }
        else if (rateCheck < nowRate.common + nowRate.rare + nowRate.epic)
        {
            nowRarity = ItemRarity.Epic;
        }
        else
        {
            nowRarity = ItemRarity.Legendary;
        }

        itemList = itemDataBase.GetItems(nowRarity, playerJobType);

        List<ItemData> availableItems = itemList
            .Where(item => !selectedItems.Contains(item))
            .ToList();

        ItemData selected = availableItems[Random.Range(0, availableItems.Count)];

        selectedItems.Add(selected);

        return selected;
    }


    //ショップアイテムの生成
    //private void GenerateShopItems()
    //{
    //    //ショップに陳列されているアイテムがある場合は削除する
    //    DestroyShopItem();

    //    //ショップに陳列されているアイテムのデータリストリセット
    //    shopItems.Clear();

    //    //ショップに陳列されているアイテムの価格表示リセット
    //    ShopMoneyTextReset();

    //    //リロールテキスト変更
    //    if (limitRerollCount <= rerollCount)
    //    {
    //        rerollText.text = "Reroll(-" + increasedRerollMoney + ")";
    //    }

    //    HashSet<ItemData> selectedItems = new();

    //    //デバッグ用レート
    //    //nowRate.common = 0;
    //    //nowRate.rare = 0;
    //    //nowRate.epic = 0;
    //    //nowRate.legendary = 100;

    //    for (int i = 0; i < shopSlotPos.Length; i++)
    //    {
    //        float rateCheck = Random.Range(0,100);

    //        List<ItemData> itemList = new();

    //        GameObject itemImage;

    //        ItemRarity nowRarity;

    //        bool isProcessed = false;

    //        if (rateCheck < nowRate.common)
    //        {
    //            nowRarity = ItemRarity.Common;
    //        }
    //        else if (rateCheck < nowRate.common + nowRate.rare)
    //        {
    //            nowRarity = ItemRarity.Rare;
    //        }
    //        else if (rateCheck < nowRate.common + nowRate.rare + nowRate.epic)
    //        {
    //            nowRarity = ItemRarity.Epic;
    //        }
    //        else
    //        {
    //            nowRarity = ItemRarity.Legendary;
    //        }

    //        itemList = itemDataBase.GetItems(nowRarity, playerJobType);

    //        List<ItemData> availableItems = itemList
    //            .Where(item => !selectedItems.Contains(item))
    //            .ToList();

    //        if (availableItems.Count == 0)
    //        {
    //            Debug.Log("要素なし");
    //            break;
    //        }

    //        ItemData selected = availableItems[Random.Range(0, availableItems.Count)];

    //        //ホールドしているアイテムか判定
    //        if (holdItems.Count != 0)
    //        {
    //            for (int k = 0; holdItems.Count > k; k++)
    //            {
    //                if (holdItems[k].listNum == i)
    //                {
    //                    shopItems.Add(holdItems[k].holdItemData);
    //                    shopItemImage.Add(holdItems[k].holdItemImageObj);

    //                    holdItems[k].holdItemImageObj.SetActive(true);
    //                    isProcessed = true;
    //                    break;
    //                }
    //            }
    //        }

    //        if (isProcessed)
    //        {
    //            continue;
    //        }

    //        itemImage = Instantiate(selected.WeaponImage, shopSlotPos[i]);
    //        itemImage.name = itemImage.name.Replace("(Clone)", "");

    //        shopItems.Add(selected);
    //        selectedItems.Add(selected);
    //        shopItemImage.Add(itemImage);
    //    }


    //    //ショップに陳列されているアイテムの価格表示
    //    for (int i = 0; i < shopItems.Count; i++)
    //    {
    //        Debug.Log($"リストの数: {shopItems.Count}");
    //        shopMoneyTextList[i].text = shopItems[i].Value.ToString();
    //    }
    //}

    //ユニークアイテム出現ショップ
    private void GenerateUniqueItems()
    {
        selectedItems.Clear();

        //ここでユニークアイテム陳列前の情報をセーブする
        //一度空にする
        saveSlots.Clear();

        foreach (var slot in shopSlots)
        {
            saveSlots.Add(new SaveShopSlotData { 
                saveItem = slot.currentItem,
                saveObj = slot.currentObj,
                isSaveHold = slot.isHold,
                });

            // 2. 表示中のオブジェクトを一旦消す（または非アクティブにする）
            if (slot.currentObj != null) slot.currentObj.SetActive(false);

            //一旦ホールドアイコンを非表示にする
            slot.holdIcon.SetActive(false);

            //セーブした後にホールドを解除
            slot.isHold = false;

        }

        foreach (var slot in shopSlots)
        {

            // --- ここから新しいアイテムの抽選・生成 ---

            // 新しいアイテムを抽選（重複なしロジックは別途）
            ItemData nextItem = GetUniqueItem();

            //データがない場合は空にする
            if (nextItem == null)
            {
                slot.currentItem = null;
                slot.currentObj = null;

                slot.priceText.text = "--";

                continue;
            }

            //生成してスロット情報を更新
            slot.currentItem = nextItem;
            slot.currentObj = Instantiate(nextItem.WeaponImage, slot.pos);
            slot.currentObj.name = nextItem.name;

            //価格表示の更新
            slot.priceText.text = nextItem.Value.ToString();

        }

    }

    private ItemData GetUniqueItem()
    {
        float rateCheck = Random.Range(0, 100);

        List<ItemData> itemList = new();

        ItemRarity nowRarity = ItemRarity.Unique;

        itemList = itemDataBase.GetItems(nowRarity, playerJobType);

        List<ItemData> availableItems = itemList
            .Where(item => !selectedItems.Contains(item))
            .ToList();

        ItemData selected;

        if (availableItems.Count == 0)
        {
            selected = null;
        }
        else
        {
            selected = availableItems[Random.Range(0, availableItems.Count)];
        }
        

        selectedItems.Add(selected);

        return selected;
    }


    //ユニークアイテムショップから通常ショップへ戻る処理
    private void ReturnToNormalShop()
    {
        for (int i = 0; i < shopSlots.Count; i++)
        {
            if (i >= saveSlots.Count) break;

            var slot = shopSlots[i];
            var saved = saveSlots[i];

            // 1. ユニークアイテムのオブジェクトを削除
            if (slot.currentObj != null) Destroy(slot.currentObj);

            // 2. 保存していたデータを復元
            slot.currentItem = saved.saveItem;
            slot.currentObj = saved.saveObj;
            slot.isHold = saved.isSaveHold;

            //ホールド状態の場合はアイコンを表示
            if (slot.isHold)
            {
                slot.holdIcon.SetActive(true);
            }

            // 3. オブジェクトを再表示
            if (slot.currentItem != null)
            {
                slot.currentObj.SetActive(true);
                slot.priceText.text = slot.currentItem.Value.ToString();

            }
        }

        GenerateShopItems();
    }



    //ショップに陳列されているアイテムの価格表示リセット
    private void ShopMoneyTextReset()
    {

        for (int i = 0; i < shopMoneyTextList.Count; i++)
        {
            shopMoneyTextList[i].text = "--";
        }
    }


    //ショップに並んでいるアイテムを削除
    //private void DestroyShopItem()
    //{
    //    if (shopItemImage.Count != 0)
    //    {
    //        for (int i = 0; i < shopItemImage.Count; i++)
    //        {
    //            //nullチェック
    //            if (shopItemImage[i] == null) continue;

    //            // ホールドされているかチェック
    //            bool isHold = false;
    //            foreach (var hold in holdItems)
    //            {
    //                if (shopItemImage[i] == hold.holdItemImageObj)
    //                {
    //                    isHold = true;
    //                    break;
    //                }
    //            }

    //            if (isHold)
    //            {
    //                // ホールドなら非表示にするだけ
    //                shopItemImage[i].SetActive(false);
    //            }
    //            else
    //            {
    //                // ホールドでないなら削除
    //                Destroy(shopItemImage[i]);
    //            }

    //        }

    //        shopItemImage.Clear();
    //    }
    //}

    //リロール関数 ボタンに設定
    public void ShopReroll()
    {


        //ここで所持金を減らす
        if (rerollCount < limitRerollCount && rerollMoney <= playerScript.money)
        {
            isShowItem = false;
            //お金計算
            playerScript.money -= rerollMoney;

            //お金のUI変更
            UIManager.instance.ChangeMoney();

            rerollCount++;

            //SE
            soundManager.PlaySE(CommonSoundType.Buy);
        }
        else if (increasedRerollMoney <= playerScript.money)
        {
            isShowItem = false;
            //お金計算
            playerScript.money -= increasedRerollMoney;

            //お金のUI変更
            UIManager.instance.ChangeMoney();
            rerollCount++;

            //SE
            soundManager.PlaySE(CommonSoundType.Buy);
        }
        else
        {
            //リロール不可SE
            soundManager.PlaySE(CommonSoundType.Beep);

        }

        

    }


    private RarityRate GetRate(int waveNum)
    {
        if (rates.Count <= waveNum - 1)
        {
            return rates[rates.Count - 1];
        }
        

        return  rates[waveNum - 1];
    }




    //バトル開始(ボタンに設定)
    public void StartBattle()
    {

        if (GameManager.instance.isClick)
        {
            GameManager.instance.state = GameManager.GameState.Battle;

            isRateCheck = false;

            isShowItem = false;

            UIManager.instance.shopCanvas.SetActive(false);

            UIManager.instance.playerCanvas.SetActive(true);

            //プレイヤーのスポーン
            GameManager.instance.PlayerWarpShopToBattleField();

            //カメラ変更
            GameManager.instance.ChangeBattleCmera();

            //リロール回数リセット
            rerollCount = 0;

            //プレイヤーのステータス保持
            playerScript.OriginStatusSet();

            //SE
            soundManager.PlaySE(CommonSoundType.BattleStart);

            //BGM変更
            if (GameManager.instance.waveNum == GameManager.instance.maxWave)
            {
                BGMManager.instance.PlayBGM(BGM.boss);
            }
            else
            {
                BGMManager.instance.PlayBGM(BGM.battle);
            }
            

        }


    }


    //プレイヤーのアイテムリストに格納されているのかチェック
    private bool IsHaveItem()
    {
        //ここでプレイヤーが所持しているのか判定
        foreach (GameObject list in playerScript.itemImageList)
        {
            if (list == catchItem)
            {
                return true;
            }
        }

        return false;

    }

    //選択されたアイテムの値段以上のお金を持っているのか判定
    private bool IsHaveMoney()
    {
        foreach (ItemData list in itemDataBase.allItems)
        {
            if(list.name == catchItem.name)
            {
                if (list.Value <= playerScript.money)
                {
                    return true;
                }
            }
        }

        return false;
    }


    //アイテムを売却したときの処理
    private void ItemSell()
    {
        //アイテム削除に当たりリストから排除　画像
        playerScript.itemImageList.Remove(catchItem);


        //アイテムデータ排除
        for (int i = 0; i < playerScript.itemList.Count; i++)
        {
            if (playerScript.itemList[i].name == catchItem.name)
            {
                playerScript.itemList.Remove(playerScript.itemList[i]);

            }
        }

        //お金を還元
        for (int i = 0; i < itemDataBase.allItems.Count; i++)
        {
            if (itemDataBase.allItems[i].name == catchItem.name)
            {
                int addValue = itemDataBase.allItems[i].Value / 2;
                //お金計算
                playerScript.money += addValue;

                //お金のUI変更
                UIManager.instance.ChangeMoney();

                //還元価格表示非表示
                sellValueText.text = string.Empty;
            }
        }

        //プレイヤーのステータス更新
        playerScript.UpdatePlyaerStatus();

        Destroy(catchItem);

        //売却SE
        soundManager.PlaySE(CommonSoundType.Sell);
    }

    //所持しているアイテムを移動させる処理
    private void HaveItemMove()
    {
        //クリックしたアイテムの初期位置と移動先の位置に変更が無ければ初期位置に戻す
        if (catchItemOriginPos == catchItem.GetComponent<ItemIndividualData>().itemSetPos)
        {
            catchItem.transform.position = catchItemOriginPos;

            //ポーションのデータはフレームから外れたときに無くなるので元に戻った時に戻す処理をする
            for (int i = 0; i < itemDataBase.allItems.Count; i++)
            {
                if (itemDataBase.allItems[i].name == catchItem.name)
                {
                    //ポーションだけ別枠処理
                    if (itemDataBase.allItems[i].ItemType == ItemType.Potion)
                    {
                        playerScript.PotionDataAddProcess(itemDataBase.allItems[i]);
                    }
                }
            }

        }
        else//位置の変更があった場合
        {
            //位置更新
            catchItem.transform.position = catchItem.GetComponent<ItemIndividualData>().itemSetPos;

            //アイテムセット音
            soundManager.PlaySE(CommonSoundType.ItemSet);

            //スペアフレームにアイテムがおかれた場合プレイヤーが所持しているアイテムの場合リストから外す
            if (catchItem.GetComponent<ItemIndividualData>().isSpare)
            {
                for (int i = 0; i < playerScript.itemList.Count; i++)
                {
                    if (playerScript.itemList[i].name == catchItem.name)
                    {
                        playerScript.itemList.Remove(playerScript.itemList[i]);

                    }
                }

            }
            else//指定の場所に配置された場合リストに追加
            {
                for (int i = 0; i < itemDataBase.allItems.Count; i++)
                {
                    if (itemDataBase.allItems[i].name == catchItem.name)
                    {
                        //ポーションだけ別枠処理
                        if (itemDataBase.allItems[i].ItemType == ItemType.Potion)
                        {
                            playerScript.PotionDataAddProcess(itemDataBase.allItems[i]);
                        }
                        else
                        {
                            playerScript.itemList.Add(itemDataBase.allItems[i]);
                        }
                        
                    }
                }
            }

            //プレイヤーのステータス更新
            playerScript.UpdatePlyaerStatus();
        }
    }

    

    //アイテムを購入する処理
    private void ItemBuy()
    {
        //選択したアイテムの位置が変更された場合（ちゃんとフレームにセットできた場合）
        if (catchItemOriginPos != catchItem.GetComponent<ItemIndividualData>().itemSetPos)
        {
            // Raycastで当たったGameObjectから、どのスロットか特定する
            var clickedSlot = shopSlots.Find(s => s.currentObj == catchItem);

            //価格表示を変更する
            clickedSlot.priceText.text = "--";

            //スロット内の画像データをnullにする
            clickedSlot.currentObj = null;


            //画像データをショップから所持データへ移行する
            playerScript.itemImageList.Add(catchItem);

            //ポーションだけ別枠処理
            if (clickedSlot.currentItem.ItemType == ItemType.Potion)
            {
                playerScript.PotionDataAddProcess(clickedSlot.currentItem);
            }
            else
            {
                playerScript.itemList.Add(clickedSlot.currentItem);
            }

            //お金計算
            playerScript.money -= clickedSlot.currentItem.Value;

            //お金のUI変更
            UIManager.instance.ChangeMoney();


            //もしスペアフレームへの格納の場合は一度データを外す
            if (catchItem.GetComponent<ItemIndividualData>().isSpare)
            {
                for (int i = 0; i < playerScript.itemList.Count; i++)
                {
                    if (playerScript.itemList[i].name == catchItem.name)
                    {
                        playerScript.itemList.Remove(playerScript.itemList[i]);

                    }
                }
            }


            //プレイヤーのステータス更新
            playerScript.UpdatePlyaerStatus();

            catchItem.transform.position = catchItem.GetComponent<ItemIndividualData>().itemSetPos;

            //スロット内のデータを空にする
            clickedSlot.currentItem = null;

            //ホールドアイテムの場合はホールドを解除する
            if (clickedSlot.isHold)
            {
                clickedSlot.isHold = false;

                //ホールド表示を非表示にする
                clickedSlot.holdIcon.SetActive(false);
            }
            

            //購入SE
            soundManager.PlaySE(CommonSoundType.Buy);


            //ユニークアイテムを購入した際強制リロール
            if (isShowUniqueItem)
            {
                isShowUniqueItem = false;

                //通常ショップに戻す処理
                ReturnToNormalShop();
            }


        }
        else
        {
            catchItem.transform.position = catchItemOriginPos;
        }
    }


    //UIに詳細文表示
    private void ShowItemDetailText()
    {
        //一度詳細テキストをクリアする
        itemDetailText.text = string.Empty;

        //クリックしたアイテムのデータへアクセス
        for (int i = 0; i < itemDataBase.allItems.Count; i++)
        {
            if (itemDataBase.allItems[i].name == catchItem.name)
            {
                itemDetailText.text = itemDataBase.allItems[i].DetailText;
            }
        }

    }

    //売却価格表示
    private void SellItemValueText()
    {
        int addValue = 0;

        //還元価格表
        for (int i = 0; i < itemDataBase.allItems.Count; i++)
        {
            if (itemDataBase.allItems[i].name == catchItem.name)
            {
                addValue = itemDataBase.allItems[i].Value / 2;
            }
        }

        sellValueText.text = "+" + addValue.ToString();
    }


    //選択したアイテムのアイテムデータ取得
    private ItemData GetItemData(GameObject item)
    {
        ItemData data = null;

        //クリックしたアイテムのデータへアクセス
        for (int i = 0; i < itemDataBase.allItems.Count; i++)
        {
            if (itemDataBase.allItems[i].name == item.name)
            {
                data =  itemDataBase.allItems[i];
                break;
            }
        }

        return data;
    }


    //スキルレベルアップボタン
    public void SkillLevelUpButtonQ()
    {
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            if (skillUpValue <= playerScript.money)
            {
                playerScript.SkillQLevelUp();

                //お金計算
                playerScript.money -= skillUpValue;

                //お金のUI変更
                UIManager.instance.ChangeMoney();

                if (playerScript.SkillLevelQ == playerScript.MaxSkillLevel)
                {
                    skillLevelTextQ.text = "Lv.Max";
                    skillLevelUpButtonQ.interactable = false;
                    skillLevelUpButtonQ.GetComponentInChildren<TextMeshProUGUI>().text = "Mastered";
                }
                else
                {
                    skillLevelTextQ.text = "Lv." + playerScript.SkillLevelQ.ToString();
                }

                //購入SE
                soundManager.PlaySE(CommonSoundType.Buy);

            }
            else
            {
                soundManager.PlaySE(CommonSoundType.Beep);
            }
        }
        else
        {
            soundManager.PlaySE(CommonSoundType.Beep);
        }



    }

    public void SkillLevelUpButtonE()
    {
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            if (skillUpValue <= playerScript.money)
            {
                playerScript.SkillELevelUp();

                //お金計算
                playerScript.money -= skillUpValue;

                //お金のUI変更
                UIManager.instance.ChangeMoney();

                if (playerScript.SkillLevelE == playerScript.MaxSkillLevel)
                {
                    skillLevelTextE.text = "Lv.Max";
                    skillLevelUpButtonE.interactable = false;
                    skillLevelUpButtonE.GetComponentInChildren<TextMeshProUGUI>().text = "Mastered";
                }
                else
                {
                    skillLevelTextE.text = "Lv." + playerScript.SkillLevelE.ToString();
                }

                //購入SE
                soundManager.PlaySE(CommonSoundType.Buy);

            }
            else
            {
                soundManager.PlaySE(CommonSoundType.Beep);
            }
        }




    }

    public void SkillLevelUpButtonR()
    {

        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            if (skillUpValue <= playerScript.money)
            {
                playerScript.SkillRLevelUp();

                //お金計算
                playerScript.money -= skillUpValue;

                //お金のUI変更
                UIManager.instance.ChangeMoney();

                if (playerScript.SkillLevelR == playerScript.MaxSkillLevel)
                {
                    skillLevelTextR.text = "Lv.Max";
                    skillLevelUpButtonR.interactable = false;
                    skillLevelUpButtonR.GetComponentInChildren<TextMeshProUGUI>().text = "Mastered";
                }
                else
                {
                    skillLevelTextR.text = "Lv." + playerScript.SkillLevelR.ToString();
                }

                //購入SE
                soundManager.PlaySE(CommonSoundType.Buy);

            }
            else
            {
                soundManager.PlaySE(CommonSoundType.Beep);
            }
        }
        else
        {
            soundManager.PlaySE(CommonSoundType.Beep);
        }



    }

    public void SkillLevelUpButtonF()
    {
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {

            if (skillUpValue <= playerScript.money)
            {
                playerScript.SkillFLevelUp();

                //お金計算
                playerScript.money -= skillUpValue;

                //お金のUI変更
                UIManager.instance.ChangeMoney();

                if (playerScript.SkillLevelF == playerScript.MaxSkillLevel)
                {
                    skillLevelTextF.text = "Lv.Max";
                    skillLevelUpButtonF.interactable = false;
                    skillLevelUpButtonF.GetComponentInChildren<TextMeshProUGUI>().text = "Mastered";
                }
                else
                {
                    skillLevelTextF.text = "Lv." + playerScript.SkillLevelF.ToString();
                }

                //購入SE
                soundManager.PlaySE(CommonSoundType.Buy);

            }
            else
            {
                soundManager.PlaySE(CommonSoundType.Beep);
            }

        }
        else
        {
            soundManager.PlaySE(CommonSoundType.Beep);
        }


    }





}

