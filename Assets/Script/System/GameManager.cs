using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{    
    
    //シングルトン化
    public static GameManager instance { get; private set; }

    //メインカメラ
    [SerializeField]
    private Camera mainCamera;

    //プレイヤーに追従するカメラ
    [SerializeField]
    private CinemachineVirtualCamera playerFollowCamera;

    //ショップカメラ
    [SerializeField]
    private CinemachineVirtualCamera shopCamera;

    //カメラの表示優先度
    private int inActiveCameraPriority = 10;
    private int activeCameraPriority = 11;

    //選択されたプレイヤー
    public GameObject player;
    


    //ゲームの状態を格納
    public enum GameState
    {
        Title,
        Battle,
        Menu,
        CharactorSelect,
        Shop,
        GameClear,
        GameOver,
        ItemListMenu,

    }

    //状態設定関数
    public GameState state;

    //現在のウェーブ数
    public int waveNum;

    //最大ウェーブ数
    public int maxWave = 10;

    //プレイヤーのショップ時の位置、角度、スケール
    [SerializeField]
    private Transform playerShopPos;

    private Quaternion playerShopRotate = Quaternion.Euler(0, 160, 0);

    //プレイヤーのバトル時の初期位置、角度、スケール
    [SerializeField]
    private Transform playerSpawnPos; 

    private Quaternion playerSpawnRotate = Quaternion.Euler(0, 90, 0);



    //ショップでのアイテムのクリック可能判定
    public bool isClick;


    //プレイヤー追従カメラの照準定義
    private CinemachinePOV playerFollowCameraPov;


    // ImpulseSourceをインスペクターでセット
    [SerializeField]
    private CinemachineImpulseSource impulseSource;


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

        //タイトルステート設定
        state = GameState.Title;

        isClick = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ウェーブ初期値定義
        waveNum = 1;

        //カメラの優先順位決定
        playerFollowCamera.Priority = inActiveCameraPriority;
        shopCamera.Priority = activeCameraPriority;

        //プレイヤー追従カメラの照準初期化
        playerFollowCameraPov = playerFollowCamera.GetCinemachineComponent<CinemachinePOV>();

        //タイトルBGM再生
        BGMManager.instance.PlayBGM(BGM.title);

        // Cinemachineの入力取得処理をこの関数の戻り値で上書きする
        //特定条件下でマウス入力による画面の向きを抑制する
        CinemachineCore.GetInputAxis = (axisName) => {
            if (Time.timeScale == 0f || state == GameState.Menu || state == GameState.GameOver || state == GameState.GameClear) return 0f; // メニュー中は入力を無視
            return Input.GetAxis(axisName); // 通常時はマウス入力を返す
        };

    }

    // Update is called once per frame
    void Update()
    {
        //バトル時はマウスカーソル非表示
        if (state == GameState.Battle)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    //バトルカメラへの遷移
    public void ChangeBattleCmera()
    {
        //プレイヤーの位置取得
        if (playerFollowCamera.Follow == null)
        {
            playerFollowCamera.Follow = player.transform;

        }

        //カメラの優先度変更
        playerFollowCamera.Priority = activeCameraPriority;
        shopCamera.Priority = inActiveCameraPriority;

        // カメラ内部の状態（Dampingによる遅延）を強制リセットしてターゲットへ飛ばす
        playerFollowCamera.OnTargetObjectWarped(player.transform, player.transform.position - playerFollowCamera.transform.position);
    }


    //ショップカメラへの遷移
    public void ChangeShopCamera()
    {
        //カメラの優先度変更
        playerFollowCamera.Priority = inActiveCameraPriority;
        shopCamera.Priority = activeCameraPriority;

        //プレイヤー追従カメラの照準の値初期化
        playerFollowCameraPov.m_HorizontalAxis.Value = 90;
        playerFollowCameraPov.m_VerticalAxis.Value = 0;

    }


    //プレイヤーの移動ショップ→バトル
    public void PlayerWarpShopToBattleField()
    {
        //プレイヤーの位置更新
        player.transform.rotation = playerSpawnRotate;

        player.transform.position = playerSpawnPos.position;

    }

    //プレイヤーの移動バトル→ショップ
    public void PlayerWarpBattleFieldToShop()
    {
        //プレイヤーの位置更新
        player.transform.position = playerShopPos.position;

        player.transform.rotation = playerShopRotate;

    }


    //カメラの移動停止
    public void CameraStop()
    {
        mainCamera.GetComponent<CinemachineBrain>().enabled = false;

    }

    //カメラの移動開始
    public void CameraMove()
    {
        mainCamera.GetComponent<CinemachineBrain>().enabled = true;

    }


    //カメラの振動
    public void OnHit()
    {
        // 衝撃を発生させる！
        impulseSource.GenerateImpulse();
    }


}
