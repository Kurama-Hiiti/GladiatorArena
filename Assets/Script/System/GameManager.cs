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


    [SerializeField]
    private CinemachineVirtualCamera shopCamera;

    private int inActiveCameraPriority = 10;

    private int activeCameraPriority = 11;

    private bool changePriority;

    //選択されたプレイヤー
    public GameObject player;

    private Vector3 playerEndPos;


    


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


    //プレイヤー追従カメラの角度(Value)を初期の90度へ戻す
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
        waveNum = 1;

        //カメラの優先順位決定
        playerFollowCamera.Priority = inActiveCameraPriority;
        shopCamera.Priority = activeCameraPriority;

        playerFollowCameraPov = playerFollowCamera.GetCinemachineComponent<CinemachinePOV>();

        //タイトルBGM再生
        BGMManager.instance.PlayBGM(BGM.title);

        // Cinemachineの入力取得処理をこの関数の戻り値で上書きする
        CinemachineCore.GetInputAxis = (axisName) => {
            if (Time.timeScale == 0f || state == GameState.Menu || state == GameState.GameOver || state == GameState.GameClear) return 0f; // メニュー中は入力を無視
            return Input.GetAxis(axisName); // 通常時はマウス入力を返す
        };

    }

    // Update is called once per frame
    void Update()
    {
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
        if (playerFollowCamera.Follow == null)
        {
            playerFollowCamera.Follow = player.transform;

        }

        playerFollowCamera.Priority = activeCameraPriority;
        shopCamera.Priority = inActiveCameraPriority;

        // カメラ内部の状態（Dampingによる遅延）を強制リセットしてターゲットへ飛ばす
        playerFollowCamera.OnTargetObjectWarped(player.transform, player.transform.position - playerFollowCamera.transform.position);
    }


    //ショップカメラへの遷移
    public void ChangeShopCamera()
    {
        playerFollowCamera.Priority = inActiveCameraPriority;
        shopCamera.Priority = activeCameraPriority;

        playerFollowCameraPov.m_HorizontalAxis.Value = 90;
        playerFollowCameraPov.m_VerticalAxis.Value = 0;

    }


    //プレイヤーの移動ショップ→バトル
    public void PlayerWarpShopToBattleField()
    {

        player.transform.rotation = playerSpawnRotate;

        player.transform.position = playerSpawnPos.position;

    }

    //プレイヤーの移動バトル→ショップ
    public void PlayerWarpBattleFieldToShop()
    {
        player.transform.position = playerShopPos.position;

        player.transform.rotation = playerShopRotate;

    }


    //カメラの移動停止
    public void CameraStop()
    {
        mainCamera.GetComponent<CinemachineBrain>().enabled = false;

    }

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


//スペアフレーム用のアイテム追加