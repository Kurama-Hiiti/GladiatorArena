using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Coffee.UIEffects;
using TMPro;


public class ItemDetailChack : MonoBehaviour
{
    //アイテム一覧表示時のアイテムの詳細表示処理



    //グラフィックレイキャスター（アイテム操作用）定義
    private GraphicRaycaster raycaster;

    //マウスポインターのデータ格納
    private PointerEventData pointerEventData;

    //イベントシステム
    private EventSystem eventSystem;

    //クリックしたアイテム
    private GameObject catchItem;

    //クリックしたアイテムのUIEffect
    private UIEffect nowEffect;

    //アイテム詳細テキスト
    [SerializeField]
    private TextMeshProUGUI itemDetailText;

    //アイテムデータベース
    [SerializeField]
    private ItemDataBase itemDataBase;

    void Start()
    {
        // Canvas から Raycaster を取得
        raycaster = this.GetComponent<GraphicRaycaster>();

        // EventSystem をシーン内から取得
        eventSystem = EventSystem.current;

    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
                    //アイテム強調表示用のエフェクトを非表示
                    if (nowEffect != null)
                    {
                        nowEffect.enabled = false;
                    }

                    //クリックしたアイテムを格納
                    catchItem = result.gameObject;

                    //クリックしたアイテムをハイライトする
                    UIEffect effect = catchItem.GetComponent<UIEffect>();

                    nowEffect = effect;

                    //アイテム強調表示
                    effect.enabled = true;

                    effect.SetVerticesDirty(); // 頂点情報の更新
                    effect.SetMaterialDirty(); // マテリアル情報の更新

                    //クリックしたアイテムの詳細表示
                    ShowItemDetailText();

                    break;

                }
                else
                {
                    //条件外の場合はエフェクトを消し詳細文も空白にする
                    if (nowEffect != null)
                    {
                        nowEffect.enabled = false;

                        nowEffect = null;
                    }

                    itemDetailText.text = string.Empty;


                }

            }
        }


    }


    //UIに詳細文表示
    private void ShowItemDetailText()
    {
        //一度詳細テキストをクリアする
        itemDetailText.text = string.Empty;

        //データベースから現在選択しているアイテムの詳細文を読み取る
        for (int i = 0; i < itemDataBase.allItems.Count; i++)
        {
            if (itemDataBase.allItems[i].name == catchItem.name)
            {
                itemDetailText.text = itemDataBase.allItems[i].DetailText;
            }
        }

    }


}