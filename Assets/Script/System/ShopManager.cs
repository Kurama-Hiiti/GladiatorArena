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

//レアリティ定義
public class RarityRate
{
    public float common;
    public float rare;
    public float epic;
    public float legendary;

}


//ショップスロット定義
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

//ホールドしたアイテムの定義
public class SaveShopSlotData
{
    public ItemData saveItem;  // セーブされた置かれているデータ
    public GameObject saveObj; // セーブされた表示されているオブジェクト
    public bool isSaveHold;    // ホールド中か
}

// スキルの種類を定義
public enum SkillType
{
    Q, E, R, F
}

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private ItemDataBase itemDataBase;

    //ショップスロットデータのリスト
    public List<ShopSlot> shopSlots = new List<ShopSlot>();

    //セーブしたショップスロットデータ
    private List<SaveShopSlotData> saveSlots = new List<SaveShopSlotData>();

    //アイテムの出現確率定義
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

    //ホールドしたいアイテムを格納
    private GameObject holdItem;

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

        //リロールテキストの初期化
        rerollText.text = "リロール(-" + rerollMoney + ")";

        //スキルレベルのテキスト初期化
        skillLevelTextQ.text = "Lv.1";
        skillLevelTextE.text = "Lv.1";
        skillLevelTextR.text = "Lv.1";
        skillLevelTextF.text = "Lv.1";
    }

    private void Update()
    {
        //ショップステート時のみ実行
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            //ジョブタイプチェック
            PlayerJobTypeCheck();


            //ショップに陳列されるレアリティのチェック
            if (!isRateCheck)
            {
                //レアリティ設定
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
                //ユニークアイテム出現ウェーブ時
                if (GameManager.instance.waveNum == uniqueItemWave && !isUniqueShop)
                {
                    //ユニークアイテム出現
                    GenerateUniqueItems();

                    //ユニークアイテム分のお金を加算
                    GameManager.instance.player.GetComponent<Player>().money += uniqueItemMoney;

                    //お金のUI変更
                    UIManager.instance.ChangeMoney();

                    //フラグ更新
                    isUniqueShop = true;
                    isShowUniqueItem = true;

                }
                else 
                {
                    //通常のアイテム陳列処理
                    GenerateShopItems();
                }
                
                //フラグ更新
                isShowItem = true;

            }


            //ショップ内のアイテム操作

            //マウスの左クリック、アイテムを持っていない時、クリック可能時
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
                    //アイテムのタグ判定
                    if (result.gameObject.CompareTag("Item"))
                    {
                        //クリックしたアイテムを格納
                        catchItem = result.gameObject;

                        //クリックしたアイテムを最前面に表示
                        catchItem.transform.SetParent(shopCanvas.transform);
                        catchItem.transform.SetAsLastSibling();

                        //クリックしたアイテムの初期位置（購入しなかった場合元の位置に戻すため）
                        catchItemOriginPos = catchItem.transform.position;

                        //アイテムをつかんだフラグ更新
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
                //クリックしたアイテムの位置をマウスポインターの位置と連動させる
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
                //アイテムをつかんでいる判定を更新
                isItemCatch = false;

                //クリックを離した際にcatchItemにアイテムが格納されている場合
                if (catchItem != null)
                {

                    //アイテムのクリックを離したときに配置するのか判定
                    if (IsHaveItem() && catchItem.GetComponent<ItemIndividualData>().isSell)//アイテムを購入済みで売却スペースにある場合
                    {
                        //アイテム売却
                        ItemSell();

                    }
                    else if (IsHaveItem())//アイテムを購入済みの場合
                    {
                        //アイテムの移動処理
                        HaveItemMove();

                    }
                    else if (!IsHaveItem() && IsHaveMoney())//購入していないアイテムかつ選択したアイテムの値段以上のお金を所持している場合
                    {
                        //アイテム購入処理
                        ItemBuy();
                    }
                    else
                    {
                        //どの場合にも該当しない場合は元の位置に戻す
                        catchItem.transform.position = catchItemOriginPos;
                    }
                    
                    //つかんでいたアイテムを空にする
                    catchItem = null;
                }

            }


            //アイテムのホールド処理
            //マウスの右クリック、アイテムを持っていない時、クリック可能時
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




                // アイテムのホールド処理
                foreach (RaycastResult result in results)
                {
                    //アイテムのタグ判定
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

                                // ホールド用のエフェクト表示の処理
                                if (clickedSlot.isHold) clickedSlot.holdIcon.SetActive(true);
                                if (!clickedSlot.isHold) clickedSlot.holdIcon.SetActive(false);


                                //ホールドSE
                                soundManager.PlaySE(CommonSoundType.Hold);
                            }

                        }


                    }

                }

            }


        }


    }

    //プレイヤーのジョブタイプ取得関数
    private void PlayerJobTypeCheck()
    {

        if(playerJobType == JobType.None)
        {
            playerJobType = GameManager.instance.player.GetComponent<Player>().data.JobType;

            playerScript = GameManager.instance.player.GetComponent<Player>();
        }
     
    }


    //ショップアイテムの生成処理
    private void GenerateShopItems()
    {
        //陳列アイテムデータ初期化
        selectedItems.Clear();

        //リロール回数が既定の回数以上の場合リロールテキスト変更
        if (limitRerollCount <= rerollCount)
        {
            rerollText.text = "リロール(-" + increasedRerollMoney + ")";
        }

        //ショップアイテム陳列処理
        foreach (var slot in shopSlots)
        {
            // ショップアイテムスロットの状態がホールド中なら何もしない（維持）
            if (slot.isHold)
            {
                if (slot.currentObj != null) slot.currentObj.SetActive(true);
                continue;
            }

            // --- ここから新しいアイテムの抽選・生成 ---

            // 1. 古いアイテムを消す
            if (slot.currentObj != null) Destroy(slot.currentObj);

            // 2. 新しいアイテムを抽選
            ItemData nextItem = GetRandomItem();

            // 3. 生成してスロット情報を更新
            //3-1.データの変更
            slot.currentItem = nextItem;
            //3-2.画像の変更
            slot.currentObj = Instantiate(nextItem.WeaponImage, slot.pos);
            //3-3.名前の変更
            slot.currentObj.name = nextItem.name;

            // 4. 価格表示の更新
            slot.priceText.text = nextItem.Value.ToString();

        }


    }

    //陳列されるアイテムの抽選関数
    private ItemData GetRandomItem()
    {
        //ランダムな数字定義
        float rateCheck = Random.Range(0, 100);

        //抽選されたレアリティ
        ItemRarity nowRarity;

        //抽選されたレアリティのアイテム一覧
        List<ItemData> itemList = new();

        //レアリティ抽選
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

        //抽選されたアイテム格納
        itemList = itemDataBase.GetItems(nowRarity, playerJobType);

        //もうすでにショップに並んでいるアイテムを除外
        List<ItemData> availableItems = itemList
            .Where(item => !selectedItems.Contains(item))
            .ToList();

        //アイテム選定
        ItemData selected = availableItems[Random.Range(0, availableItems.Count)];

        //ショップに並ぶアイテムリストへ追加
        selectedItems.Add(selected);

        return selected;
    }


    //ユニークアイテム出現ショップ
    private void GenerateUniqueItems()
    {
        //陳列アイテムデータ初期化
        selectedItems.Clear();

        //一度空にする
        saveSlots.Clear();

        //ここでユニークアイテム陳列前の情報をセーブする（ユニークアイテム陳列前にホールドしていたデータを保持するため）
        foreach (var slot in shopSlots)
        {
            //saveSlotsへショップアイテムの情報を格納
            saveSlots.Add(new SaveShopSlotData { 
                saveItem = slot.currentItem,
                saveObj = slot.currentObj,
                isSaveHold = slot.isHold,
                });

            // 表示中のオブジェクトを一旦非アクティブにする
            if (slot.currentObj != null) slot.currentObj.SetActive(false);

            //一旦ホールドアイコンを非表示にする
            slot.holdIcon.SetActive(false);

            //セーブした後にホールドを解除
            slot.isHold = false;

        }

        foreach (var slot in shopSlots)
        {

            // --- ここから新しいアイテムの抽選・生成 ---

            // 新しいアイテムを抽選
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

    //陳列されるユニークアイテムの抽選関数
    private ItemData GetUniqueItem()
    {

        List<ItemData> itemList = new();

        ItemRarity nowRarity = ItemRarity.Unique;

        itemList = itemDataBase.GetItems(nowRarity, playerJobType);

        //もうすでにショップに並んでいるアイテムを除外
        List<ItemData> availableItems = itemList
            .Where(item => !selectedItems.Contains(item))
            .ToList();

        ItemData selected;

        //陳列するべきユニークアイテムの有無確認
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
        //一度ユニークアイテム表示前の状態へ戻す
        for (int i = 0; i < shopSlots.Count; i++)
        {
            if (i >= saveSlots.Count) break;

            //スロット状況を定義
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

        //ショップアイテム生成
        GenerateShopItems();
    }



    //リロール関数 ボタンに設定
    public void ShopReroll()
    {


        //ここで所持金を減らす
        //リロール回数によってリロールの値段が代わる
        if (rerollCount < limitRerollCount && rerollMoney <= playerScript.money)
        {
            //ショップのアイテム表示フラグを変更
            isShowItem = false;

            //お金計算
            playerScript.money -= rerollMoney;

            //お金のUI変更
            UIManager.instance.ChangeMoney();

            //リロール回数加算
            rerollCount++;

            //SE
            soundManager.PlaySE(CommonSoundType.Buy);
        }
        else if (increasedRerollMoney <= playerScript.money)//リロール値段上昇時
        {
            //ショップのアイテム表示フラグを変更
            isShowItem = false;

            //お金計算
            playerScript.money -= increasedRerollMoney;

            //お金のUI変更
            UIManager.instance.ChangeMoney();

            //リロール回数加算
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


    //ウェーブ数に応じたアイテムの出現確率取得関数
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
            //ステートをバトル状態へ更新
            GameManager.instance.state = GameManager.GameState.Battle;

            //アイテム出現確率取得フラグ更新
            isRateCheck = false;

            //アイテム表示フラグ更新
            isShowItem = false;

            //ショップ画面非表示
            UIManager.instance.shopCanvas.SetActive(false);

            //バトル時の画面表示
            UIManager.instance.playerCanvas.SetActive(true);

            //プレイヤーのスポーン
            GameManager.instance.PlayerWarpShopToBattleField();

            //カメラ変更
            GameManager.instance.ChangeBattleCmera();

            //リロール回数リセット
            rerollCount = 0;

            //プレイヤーのステータス保持(バトル中にステータスが変化する場合があるので元のステータスを保持する)
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


    //プレイヤーのアイテムリストに格納されているのかチェック(アイテム購入済みかの判定)
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
                //購入金額の半額が売価（端数切り捨て）
                int addValue = itemDataBase.allItems[i].Value / 2;

                //お金加算
                playerScript.money += addValue;

                //所持金のUI変更
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
        //クリックしたアイテムの初期位置と移動先の位置(アイテムを配置するフレームの位置)に変更が無ければ初期位置に戻す
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

            //スペアフレームにアイテムがおかれた場合、プレイヤーが所持（装備）しているアイテムのリストから外す
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
            else//指定の場所(アイテムの装備フレーム)に配置された場合リストに追加
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
        //選択したアイテムの位置が変更された場合（ちゃんとフレームにセットした場合）
        if (catchItemOriginPos != catchItem.GetComponent<ItemIndividualData>().itemSetPos)
        {
            // クリックしているアイテムについてRaycastで当たったGameObjectから、どのスロットか特定する
            var clickedSlot = shopSlots.Find(s => s.currentObj == catchItem);

            //価格表示を変更する
            clickedSlot.priceText.text = "--";

            //スロット内の画像データをnullにする
            clickedSlot.currentObj = null;


            //画像データをショップから所持画像データリストへ移行する
            playerScript.itemImageList.Add(catchItem);

            //購入したアイテムのデータをリストへ追加（ポーションだけ別枠処理）
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

            //所持金のUI変更
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

            //購入したアイテムのオブジェクトの位置を更新
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
            //アイテムが購入されなかった場合、初期位置に戻す
            catchItem.transform.position = catchItemOriginPos;
        }
    }


    //UIに詳細文表示
    private void ShowItemDetailText()
    {
        //一度詳細テキストをクリアする
        itemDetailText.text = string.Empty;

        //データへアクセスしてクリックしたアイテムの詳細テキストを表示
        for (int i = 0; i < itemDataBase.allItems.Count; i++)
        {
            if (itemDataBase.allItems[i].name == catchItem.name)
            {
                itemDetailText.text = itemDataBase.allItems[i].DetailText;
            }
        }

    }

    //売却価格表示関数
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


    //Unityのボタンから直接呼ぶためのラッパー関数
    public void OnClickSkillUpQ() => TrySkillLevelUp(SkillType.Q);
    public void OnClickSkillUpE() => TrySkillLevelUp(SkillType.E);
    public void OnClickSkillUpR() => TrySkillLevelUp(SkillType.R);
    public void OnClickSkillUpF() => TrySkillLevelUp(SkillType.F);

    
    //スキルレベルアップ共通処理
    private void TrySkillLevelUp(SkillType skillType)
    {
        //ショップ状態でない、またはお金が足りない場合はビープ音を鳴らして即リターン
        if (GameManager.instance.state != GameManager.GameState.Shop || playerScript.money < skillUpValue)
        {
            soundManager.PlaySE(CommonSoundType.Beep);
            return;
        }

        //スキルに応じたレベルアップ処理を実行
        ExecuteSkillLevelUp(skillType);

        //共通の支払処理とUI更新
        playerScript.money -= skillUpValue;
        UIManager.instance.ChangeMoney();
        soundManager.PlaySE(CommonSoundType.Buy);

        //対象スキルのUI表示を更新
        UpdateSkillUI(skillType);
    }


    // スキルタイプに応じてPlayer側の処理を呼び出す
    private void ExecuteSkillLevelUp(SkillType skillType)
    {
        //スキルの攻撃力倍率上昇
        switch (skillType)
        {
            case SkillType.Q: playerScript.SkillQLevelUp(); break;
            case SkillType.E: playerScript.SkillELevelUp(); break;
            case SkillType.R: playerScript.SkillRLevelUp(); break;
            case SkillType.F: playerScript.SkillFLevelUp(); break;
        }
    }

    // スキルタイプに応じてUIテキストとボタンの状態を更新する
    private void UpdateSkillUI(SkillType skillType)
    {
        //処理対象のUIと現在のレベルを特定する
        Button targetButton = null;
        TextMeshProUGUI targetText = null;
        int currentLevel = 0;

        //選択されたスキルタイプのボタン、スキルレベルテキスト、スキルレベル取得
        switch (skillType)
        {
            case SkillType.Q:
                targetButton = skillLevelUpButtonQ;
                targetText = skillLevelTextQ;
                currentLevel = playerScript.SkillLevelQ;
                break;
            case SkillType.E:
                targetButton = skillLevelUpButtonE;
                targetText = skillLevelTextE;
                currentLevel = playerScript.SkillLevelE;
                break;
            case SkillType.R:
                targetButton = skillLevelUpButtonR;
                targetText = skillLevelTextR;
                currentLevel = playerScript.SkillLevelR;
                break;
            case SkillType.F:
                targetButton = skillLevelUpButtonF;
                targetText = skillLevelTextF;
                currentLevel = playerScript.SkillLevelF;
                break;
        }

        //UIの共通書き換えロジック（スキルレベルMax時）
        if (currentLevel == playerScript.MaxSkillLevel)
        {
            targetText.text = "Lv.Max";
            targetButton.interactable = false;
            targetButton.GetComponentInChildren<TextMeshProUGUI>().text = "Mastered";
        }
        else
        {
            //レベル上昇時
            targetText.text = $"Lv.{currentLevel}";
        }
    }



}

