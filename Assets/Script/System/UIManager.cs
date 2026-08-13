using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static GameManager;
using UnityEngine.EventSystems;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    //シングルトン化
    public static UIManager instance { get; private set; }


    //タイトルキャンバス
    [SerializeField]
    private GameObject titleCanvas;

    //ショップキャンバス
    public GameObject shopCanvas;

    //プレイヤーUIキャンバス（バトル中UI）
    public GameObject playerCanvas;

    //タイトルテキスト
    [SerializeField]
    private GameObject titleLogo;

    //タイトルテキストの初期位置と移動位置
    private Vector3 titleLogoInitPos;

    private Vector3 titleLogoEndPos;

    //スタートボタン
    [SerializeField]
    private GameObject startButton;

    //ボタンの初期位置と移動位置
    private Vector3 startButtonInitPos;

    private Vector3 startButtonEndPos;

    private Vector3 moveTitleLogoAmount = new Vector3(0, 755, 0);

    //ボタンの移動量
    private Vector3 moveButtonAmount = new Vector3(0, 400, 0);

    //UI移動時間
    private float UIduration = 1f;

    //ショップフレーム
    [SerializeField]
    private GameObject shopFrame;

    //プレイヤー選択時のプレイヤーの位置(不必要かも)
    [SerializeField]
    private GameObject playerSelectPos;

    //ゲーム開始時ショップでのキャラクター位置
    [SerializeField]
    private GameObject playerShopPos;

    //選択可能なプレイヤーリスト
    [SerializeField]
    private GameObject[] playerArray;

    //ショップのフレーム位置
    [SerializeField]
    private GameObject shopFramePos;


    //プレイヤー選択ボタンの塊
    [SerializeField]
    private GameObject selectButtonMass;

    //バトル開始ボタン
    [SerializeField]
    private GameObject nextButtleButton;

    //バトル開始ボタン位置
    [SerializeField]
    private GameObject nextButtleButtonPos;

    //売却スペース
    [SerializeField]
    private GameObject sellSpace;

    //売却スペース位置
    [SerializeField]
    private GameObject sellSpacePos;

    //プレイヤーのスクリプト格納
    private Player selectPlayer;

    //読み取り専用の現在表示しているプレイヤーのスクリプト
    public Player SelectPlayer => selectPlayer;

    //装備品の装備位置
    //武器
    [SerializeField]
    private Transform weaponPos;

    //セカンダリー装備
    [SerializeField]
    private Transform secondaryPos;

    //頭装備
    [SerializeField]
    private Transform helmPos;

    //胴装備
    [SerializeField]
    private Transform armorPos;

    //手装備
    [SerializeField]
    private Transform glovePos;

    //足装備
    [SerializeField]
    private Transform shoesPos;

    //ポーション
    [SerializeField]
    private Transform[] potionPos;

    //アクセサリー
    [SerializeField]
    private Transform accessoryPos;

    //スペアスロット
    [SerializeField]
    private Transform[] sparePos;

    //初期武器表示フラグ
    private bool isShowInitItem;



    //キャラ選択画面で表示される初期装備リスト
    [SerializeField]
    private List<GameObject> initItemList;

    //今表示されているキャラクターの番号格納
    private int nowSlectJobNum;

    //所持金テキスト
    [SerializeField]
    private TextMeshProUGUI moneyText;


    //クリアUI
    [SerializeField]
    private GameObject clearUI;

    //ゲームクリアUI
    [SerializeField]
    private GameObject grandClearUI;

    //ゲームオーバーキャンバス
    [SerializeField]
    private GameObject gameOverUI;


    //waveText
    [SerializeField]
    private GameObject waveText;

    [SerializeField]
    private GameObject waveTextPos;

    //ステータス画面
    [SerializeField]
    private GameObject playerStatusUI;

    //ステータス画面処理用スクリプト
    [SerializeField]
    private PlayerStatusUI statusUI;


    //サウンドマネージャー
    [SerializeField]
    private CommonSoundManager soundManager;

    //メニューUI
    [SerializeField]
    private GameObject menuUI;

    //メニューUIのWave数テキスト
    [SerializeField]
    private TextMeshProUGUI nowWaveText;

    //タイトル画面へ遷移する時の警告キャンバス
    [SerializeField]
    private GameObject cautionCanvas;

    //アイテムリストキャンバス
    [SerializeField]
    private GameObject itemListCanvas;


    //アイテムリストメニューを開いた際の直前のステート
    private GameState beforeState;


    //スキルレベルアップ用スキルアイコンオブジェクト
    //剣士
    [SerializeField]
    private GameObject swordManSkillIcon;

    //魔術師
    [SerializeField]
    private GameObject mageSkillIcon;


    //操作説明用UI
    [SerializeField]
    private GameObject operateUI;

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

    void Start()
    {
        //表示UI初期化
        titleCanvas.SetActive(true);
        shopCanvas.SetActive(false);
        playerCanvas.SetActive(false);
        playerStatusUI.SetActive(false);
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);
        menuUI.SetActive(false);

        //タイトルロゴ初期位置
        titleLogoInitPos = titleLogo.transform.position;

        //スタートボタン初期位置
        startButtonInitPos = startButton.transform.position;

        //タイトルロゴ移動先位置
        titleLogoEndPos = titleLogoInitPos + moveTitleLogoAmount;

        //スタートボタン移動先位置
        startButtonEndPos = startButtonInitPos - moveButtonAmount;

        //プレイヤーのオブジェクト全て非表示
        for (int i = 0; i < playerArray.Length; i++)
        {
            playerArray[i].SetActive(false);
        }

    }

    private void Update()
    {
        if (GameManager.instance.state == GameManager.GameState.CharactorSelect)
        {
            if (!isShowInitItem)
            {
                //選択されているキャラクターの初期装備表示
                InitEquipment();
            }

        }

        //戦闘中Tabキーでメニュー表示
        if (GameManager.instance.state == GameManager.GameState.Battle && Input.GetKeyDown(KeyCode.Tab))
        {
            //メニューステートへ変更
            GameManager.instance.state = GameState.Menu;

            //カメラの速度停止
            GameManager.instance.CameraStop();

            //メニューUI表示
            menuUI.SetActive(true);

            //現在のWave数表示
            nowWaveText.text = "Round : " + GameManager.instance.waveNum.ToString() + " / " + GameManager.instance.maxWave.ToString();

            //SE
            soundManager.PlaySE(CommonSoundType.NormalButton);

            Time.timeScale = 0;
        }

    }




    //ゲームスタート時キャンバス表示変更(ボタン)
    public void CharactorSelect()
    {
        //タイトルロゴを移動させる
        titleLogo.transform.DOMove(titleLogoEndPos, UIduration);

        //スタートボタンを移動させた後UI変更処理実行（Title → CharactorSelect）
        startButton.transform.DOMove(startButtonEndPos, UIduration)
            .OnComplete(() => ChangeTitleToSelectCanvas());

        //SE
        soundManager.PlaySE(CommonSoundType.StartButton);

    }

    //キャンバス変更関数
    private void ChangeTitleToSelectCanvas()
    {
        //タイトルUI非表示
        titleCanvas.SetActive(false);
        //ショップUI表示
        shopCanvas.SetActive(true);

        //プレイヤー表示
        playerArray[0].SetActive(true);

        //タイトルロゴとスタートボタンの位置初期化
        titleLogo.transform.position = titleLogoInitPos;
        startButton.transform.position = startButtonInitPos;

        //ステート変更
        GameManager.instance.state = GameManager.GameState.CharactorSelect;

        //BGM変更
        BGMManager.instance.PlayBGM(BGM.shop);
    }

    //プレイヤー決定関数（ボタン）
    public void PlayerSelect()
    {
        for (int i = 0; i < playerArray.Length; i++)
        {
            //現在表示されているキャラクターをPlayerへ設定する
            if (playerArray[i].activeSelf)
            {
                GameManager.instance.player = playerArray[i];
            }
        }

        //ボタンSE
        soundManager.PlaySE(CommonSoundType.NextButton);

        //ショップフレーム表示の後ステート変更
        shopFrame.transform.DOMove(shopFramePos.transform.position, UIduration)
            .OnComplete(() => GameManager.instance.state = GameManager.GameState.Shop);

        //選択ボタン非表示
        selectButtonMass.SetActive(false);

        //バトル開始ボタン移動
        nextButtleButton.transform.DOMove(nextButtleButtonPos.transform.position, UIduration);

        //WaveText移動
        waveText.transform.DOMove(waveTextPos.transform.position, UIduration);

        //売却スペース移動
        sellSpace.transform.DOMove(sellSpacePos.transform.position, UIduration);

        //プレイヤー移動
        GameManager.instance.player.transform.DOMove(playerShopPos.transform.position, UIduration);

        //初期武器の画像データをプレイヤーが所持しているアイテムのリストに格納
        GameManager.instance.player.GetComponent<Player>().itemImageList.AddRange(initItemList);

    }


    //キャラクター選択時に初期武器を表示
    private void InitEquipment()
    {
        //選択されているキャラのスクリプト格納
        for (int i = 0; i < playerArray.Length; i++)
        {
            if (playerArray[i].activeSelf)
            {
                selectPlayer = playerArray[i].GetComponent<Player>();

                //今表示されているキャラクターの番号格納
                nowSlectJobNum = i;
            }
        }

        //そのスクリプトから初期武器を取得　→　武器種を読み取り指定の位置へ画像を表示
        for (int i = 0; i < selectPlayer.itemList.Count; i++)
        {
            GameObject instantiateObject;
            switch (selectPlayer.itemList[i].ItemType)
            {
                //武器
                case ItemType.Weapon:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, weaponPos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);

                    break;

                //防具
                case ItemType.Armor:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, armorPos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;

                //盾(セカンダリー)
                case ItemType.Secondary:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, secondaryPos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;

                //ヘルム
                case ItemType.Helm:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, helmPos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;

                //グローブ
                case ItemType.Glove:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, glovePos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;

                //ブーツ
                case ItemType.Boots:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, shoesPos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;

                //装飾品
                case ItemType.Accessory:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, accessoryPos);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;

                //ポーション
                case ItemType.Potion:
                    instantiateObject = Instantiate(selectPlayer.itemList[i].WeaponImage, potionPos[0]);
                    //(Clone)を消す
                    instantiateObject.name = instantiateObject.name.Replace("(Clone)", "");
                    initItemList.Add(instantiateObject);
                    break;
            }
        }

        //初期所持金表示
        moneyText.text = selectPlayer.data.FirstMoney.ToString();

        isShowInitItem = true;

        //ここでスキルレベルアップのスキルアイコンを変更する
        switch (selectPlayer.data.JobType)
        {
            case JobType.SwordMan:
                swordManSkillIcon.SetActive(true);
                mageSkillIcon.SetActive(false);
                break;

            case JobType.Mage:
                swordManSkillIcon.SetActive(false);
                mageSkillIcon.SetActive(true);
                break;
        }

    }


    //お金が変化した際に呼ぶ
    public void ChangeMoney()
    {
        moneyText.text = GameManager.instance.player.GetComponent<Player>().money.ToString();
    }





    //ジョブセレクト右ボタン(ボタンに設定)
    public void NextJobSelectButton()
    {
        //表示されている初期装備を削除
        for (int i = 0; i < initItemList.Count; i++)
        {
            Destroy(initItemList[i].gameObject);
        }

        //初期装備リストを初期化
        initItemList.Clear();

        //現在表示されているキャラクターを非表示
        playerArray[nowSlectJobNum].SetActive(false);

        //ジョブの表示番号加算
        nowSlectJobNum++;

        //キャラクターのリスト以上の数字になった場合初期化（右ボタンを押しているとループするようにする）
        if (playerArray.Length - 1 < nowSlectJobNum)
        {
            nowSlectJobNum = 0;
        }

        //プレイヤー表示
        playerArray[nowSlectJobNum].SetActive(true);

        //初期装備表示フラグ更新
        isShowInitItem = false;

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

    }


    //ジョブセレクト左ボタン(ボタンに設定)
    public void BackJobSelectButton()
    {
        for (int i = 0; i < initItemList.Count; i++)
        {
            Destroy(initItemList[i].gameObject);
        }

        initItemList.Clear();

        playerArray[nowSlectJobNum].SetActive(false);

        nowSlectJobNum--;

        if (nowSlectJobNum < 0)
        {
            nowSlectJobNum = playerArray.Length - 1;
        }

        playerArray[nowSlectJobNum].SetActive(true);

        isShowInitItem = false;

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

    }


    //バトル勝利時のUI表示関数
    public void ShowClearUI()
    {
        //最終Wave数の場合の処理
        if (GameManager.instance.maxWave <= GameManager.instance.waveNum)
        {
            //ゲームクリアUI
            grandClearUI.SetActive(true);

            //ゲームクリアBGM
            BGMManager.instance.PlayBGM(BGM.gameClear);

        }
        else
        {
            //通常のクリアUI
            clearUI.SetActive(true);
            //SE
            soundManager.PlaySE(CommonSoundType.WaveClear);

            //ウェーブクリアBGM
            BGMManager.instance.PlayBGM(BGM.waveClear);
        }


        //プレイヤーのアニメーションリセット（idol状態へ遷移）
        GameManager.instance.player.GetComponent<Player>().AnimationReset();



    }

    //ゲームオーバーUI表示関数
    public void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);

        //SE
        soundManager.PlaySE(CommonSoundType.GameOver);

        //BGM
        BGMManager.instance.PlayBGM(BGM.gameOver);
    }


    //バトル勝利時ショップへ遷移する関数(ボタン)
    public void GoShopButton()
    {
        //表示中のUI非表示
        playerCanvas.SetActive(false);
        clearUI.SetActive(false);

        //ショップのUI表示
        shopCanvas.SetActive(true);

        //ウェーブ数加算
        GameManager.instance.waveNum++;

        //ゲームステート変更
        GameManager.instance.state = GameState.Shop;

        //カメラの切り替え
        GameManager.instance.ChangeShopCamera();

        //プレイヤーの移動
        GameManager.instance.PlayerWarpBattleFieldToShop();

        //お金を取得 21～29
        GameManager.instance.player.GetComponent<Player>().money += (19 + GameManager.instance.waveNum);

        //所持金更新
        ChangeMoney();

        //クリア時の回復
        GameManager.instance.player.GetComponent<Player>().WaveClearHeal();

        //ポーションでのステータス上昇をリセット
        GameManager.instance.player.GetComponent<Player>().PotionEffectReset();

        //SE
        soundManager.PlaySE(CommonSoundType.NextButton);

        //BGM
        BGMManager.instance.PlayBGM(BGM.shop);

    }


    //ステータス画面表示関数（ボタン）
    public void ShowPlayerStatusUI()
    {
        //UI表示
        playerStatusUI.SetActive(true);

        //表示内容更新関数を実施
        statusUI.StatusUIUpdate();

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //ショップアイテムおよび装備クリック不可
        GameManager.instance.isClick = false;

    }

    //ステータス画面を非表示にする関数（ボタン）
    public void HiddenPlayerStatusUI()
    {
        //非表示
        playerStatusUI.SetActive(false);

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //クリック可能
        GameManager.instance.isClick = true;
    }

    //メニューUI非表示(ボタン)
    public void HiddenMenuUI()
    {
        //UI非表示
        menuUI.SetActive(false);

        //ステート変更
        GameManager.instance.state = GameState.Battle;

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //タイムスケール初期化
        Time.timeScale = 1.0f;

        //カメラ移動可能化
        GameManager.instance.CameraMove();

    }


    //警告キャンバス表示(ボタン)　ゲームの途中終了についての警告
    public void ShowCautionCanvas()
    {
        //UI表示
        cautionCanvas.SetActive(true);
        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //ショップアイテムおよび装備クリック不可
        GameManager.instance.isClick = false;
    }

    //警告キャンバス非表示
    public void HiddenCautionCanvas()
    {
        //非表示
        cautionCanvas.SetActive(false);

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //クリック可能化
        GameManager.instance.isClick = true;

    }

    //アイテムリスト表示(ボタン)
    public void ShowItemList()
    {
        //アイテムリスト表示前のステート格納
        beforeState = GameManager.instance.state;

        //UI表示
        itemListCanvas.SetActive(true);

        //ショップアイテムおよび装備クリック不可
        GameManager.instance.isClick = false;

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //ステート変更
        GameManager.instance.state = GameState.ItemListMenu;

    }

    //アイテムリスト非表示(ボタン)
    public void HiddenItemList()
    {
        //非表示
        itemListCanvas.SetActive(false);
        
        //クリック可能化
        GameManager.instance.isClick = true;

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

        //ステート変更
        GameManager.instance.state = beforeState;
    }


    //ゲーム終了(ボタン)
    public void GameEndButton()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif

    }


    //操作説明用UI表示関数
    public void ShowOperateUI()
    {
        operateUI.SetActive(true);

        soundManager.PlaySE(CommonSoundType.NormalButton);
    }

    //操作説明用UI非表示関数
    public void HiddenOperateUI()
    {
        operateUI.SetActive(false);

        soundManager.PlaySE(CommonSoundType.NormalButton);
    }


}