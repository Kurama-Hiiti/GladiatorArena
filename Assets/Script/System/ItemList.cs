using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ItemList : MonoBehaviour
{
    //アイテム一覧表示時のソート機能スクリプト

    //データベース定義
    [SerializeField]
    private ItemDataBase ItemDataBase;

    //一覧表示するオブジェクトのリスト
    private List<GameObject> itemList = new List<GameObject>();

    //アイテム表示空間（Content）のサイズ
    private RectTransform rectTransform;

    //Contentのサイズの下端部の余白サイズ
    private float bottomMargin = 160f;


    //サウンドマネージャー
    [SerializeField]
    private CommonSoundManager soundManager;



    //アイテムソート用のトグル
    [SerializeField]
    private List<Toggle> toggles;


    private async void Start()
    {
        //ContentのRectTransformの取得
        rectTransform = GetComponent<RectTransform>();

        //アイテム一覧表示時に全てのアイテムを生成・表示する
        foreach (var item in ItemDataBase.allItems)
        {
            GameObject addItemImage = Instantiate(item.WeaponImage,this.gameObject.transform);

            //アイテム名のCloneを消す
            addItemImage.name = addItemImage.name.Replace("(Clone)", "");

            //リストに追加
            itemList.Add(addItemImage);


        }

        //アイテム生成によるレイアウト計算が終わるまで待つ
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        //Contentサイズ変更
        if (itemList.Count > 0)
        {
            //最後の要素のY軸位置を取得する
            Transform lastItemPos = itemList[itemList.Count - 1].transform;
            ReSizeContent(lastItemPos);
        }


        //toggleを定義しソートする
        foreach (var toggle in toggles)
        {
            Debug.Log("通過");

            // スクリプトからイベントを登録（名前を識別子として渡す）
            string filterName = toggle.gameObject.name;

            toggle.onValueChanged.AddListener((isOn) => {
                // オン/オフどちらに動いても、最新の状態に一括更新する
                OnToggleChanged(isOn);
            });
        }


    }

    // トグルがクリックされたときの共通イベント
    private void OnToggleChanged(bool isOn)
    {
        // オンになったときだけSEを鳴らす
        if (isOn)
        {
            soundManager.PlaySE(CommonSoundType.NormalButton);
        }

        // フィルターとソートをまとめて実行
        ApplyFilterAndSort();
    }


    //ソートリセット関数
    private async void ResetList()
    {
        //一度全ての画像を表示にする
        foreach (var itemImage in itemList)
        {
            itemImage.SetActive(true);
        }

        //レイアウト計算が終わるまで待つ
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        //Contentサイズ変更
        if (itemList.Count > 0)
        {
            //最後の要素のY軸位置を取得する
            Transform lastItemPos = itemList[itemList.Count - 1].transform;
            ReSizeContent(lastItemPos);
        }
        

    }


    //生成されたアイテムの最後の要素を考慮したContentサイズに変更する関数
    private void ReSizeContent(Transform lastItemPos)
    {

        //最後の要素の位置からContentのサイズを変更する
        float contentHeight = Mathf.Abs(lastItemPos.localPosition.y) + bottomMargin;

        // 高さだけ変更する
        Vector2 size = rectTransform.sizeDelta;
        size.y = contentHeight;
        rectTransform.sizeDelta = size;

    }



    //ソートリセットボタン
    public void SortReset()
    {
        ResetList();

        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }

        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

    }



    //アイテム一覧が表示された時の処理
    private void OnEnable()
    {
        //全てのアイテム表示
        ResetList();

        //トグルの状況リセット
        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }

    }



    private async void ApplyFilterAndSort()
    {
        // 現在「オン」になっているトグルの名前（条件）をすべて取得
        // 例: ["SwordMan", "Rare"] のようなリストが取れる
        List<string> activeFilters = toggles
            .Where(t => t.isOn)
            .Select(t => t.gameObject.name)
            .ToList();

        //ソートするジョブタイプリスト
        List<JobType> sortJobTypes = new List<JobType>();

        //ソートするレアリティリスト
        List<ItemRarity> sortItemRarities = new List<ItemRarity>();

        //取得したactiveFiltersはトグルの名前なので名前と一致するジョブタイプやレアリティを選定する
        foreach (var filter in activeFilters)
        {
            switch (filter)
            {
                case "Normal":
                    sortJobTypes.Add(JobType.Normal);
                    break;
                case "SwordMan":
                    sortJobTypes.Add(JobType.SwordMan);
                    break;
                case "Mage":
                    sortJobTypes.Add(JobType.Mage);
                    break;
                case "Common":
                    sortItemRarities.Add(ItemRarity.Common);
                    break;
                case "Rare":
                    sortItemRarities.Add(ItemRarity.Rare);
                    break;
                case "Epic":
                    sortItemRarities.Add(ItemRarity.Epic);
                    break;
                case "Legendary":
                    sortItemRarities.Add(ItemRarity.Legendary);
                    break;
                case "Unique":
                    sortItemRarities.Add(ItemRarity.Unique);
                    break;
            }
        }

        //ソートするアイテムのリスト
        List<ItemData> sortItemList = ItemDataBase.allItems;

        // もしトグルが1つも挙がっていなければ、全アイテムを表示
        if (activeFilters.Count == 0)
        {
            foreach (var itemImage in itemList)
            {
                itemImage.SetActive(true);
            }
        }else
        {
            //ジョブソート
            if (sortJobTypes.Count > 0)
            {
                // オンになっているトグルの条件に「合致する」アイテムだけを抽出（フィルター）
                sortItemList = sortItemList.Where(item =>
                sortJobTypes.Contains(item.JobType)
                ).ToList();
            }

            //レアリティソート
            if (sortItemRarities.Count > 0)
            {
                // オンになっているトグルの条件に「合致する」アイテムだけを抽出（フィルター）
                sortItemList = sortItemList.Where(item =>
                sortItemRarities.Contains(item.Rarity)
                ).ToList();

            }

        }

        //ソートして表示されるアイテムを表示
        foreach (var itemImage in itemList)
        {
            foreach (var item in sortItemList)
            {
                if (item.name == itemImage.name)
                {
                    itemImage.SetActive(true);
                    break;
                }
                else
                {
                    itemImage.SetActive(false);
                }
            }
        }


        //アイテム生成によるレイアウト計算が終わるまで待つ
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);


        //表示されているオブジェクトの最後尾を取得する
        GameObject lastActiveItem = itemList.LastOrDefault(obj => obj != null && obj.activeInHierarchy);

        //最後の要素のY軸位置を取得する
        Transform lastItemPos = lastActiveItem.transform;

        //最後の要素の位置からContentのサイズを変更する
        ReSizeContent(lastItemPos);



    }





}
