using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    [SerializeField]
    private ItemDataBase ItemDataBase;

    private List<GameObject> itemList = new List<GameObject>();

    private RectTransform rectTransform;

    //Contentのサイズの下端部の余白サイズ
    private float bottomMargin = 160f;


    //サウンドマネージャー
    [SerializeField]
    private CommonSoundManager soundManager;

    //該当アイテムが存在しない場合表示されるテキスト
    [SerializeField]
    private TextMeshProUGUI itemNoneText;

    //表示されているアイテムの個数をカウントする変数
    private int visibleItemCount;


    //ジョブソートフラグ
    private bool isSwordManSort;

    private bool isMageSort;

    private bool isNormalSort;

    //レアリティソートフラグ
    private bool isRaritySort;

    private ItemRarity nowSortItemRarity;


    //アイテムソート用のトグル
    [SerializeField]
    private List<Toggle> toggles;


    private async void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        itemNoneText.enabled = false;



        foreach (var item in ItemDataBase.allItems)
        {
            GameObject addItemImage = Instantiate(item.WeaponImage,this.gameObject.transform);

            addItemImage.name = addItemImage.name.Replace("(Clone)", "");

            itemList.Add(addItemImage);


        }


        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (itemList.Count > 0)
        {

            //最後の要素のY軸位置を取得する
            Transform lastItemPos = itemList[itemList.Count - 1].transform;

            //最後の要素の位置からContentのサイズを変更する

            float contentHeight = Mathf.Abs(lastItemPos.localPosition.y) + bottomMargin;

            // 高さだけ変更する
            Vector2 size = rectTransform.sizeDelta;
            size.y = contentHeight;
            rectTransform.sizeDelta = size;
        }


        foreach (var toggle in toggles)
        {
            // スクリプトからイベントを登録（名前を識別子として渡す）
            string filterName = toggle.gameObject.name;
            toggle.onValueChanged.AddListener((isOn) => {
                UpdateFilter(filterName, isOn);
            });
        }


    }

    //ソートリセット関数
    private async void ResetList()
    {
        itemNoneText.enabled = false;

        //一度全ての画像を表示にする
        foreach (var itemImage in itemList)
        {
            itemImage.SetActive(true);
        }

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (itemList.Count > 0)
        {

            //最後の要素のY軸位置を取得する
            Transform lastItemPos = itemList[itemList.Count - 1].transform;

            //最後の要素の位置からContentのサイズを変更する

            float contentHeight = Mathf.Abs(lastItemPos.localPosition.y) + bottomMargin;

            // 高さだけ変更する
            Vector2 size = rectTransform.sizeDelta;
            size.y = contentHeight;
            rectTransform.sizeDelta = size;
        }

    }


    //ジョブソート
    private async void JobTypeSortItem(JobType job)
    {
        visibleItemCount = 0;

        //一時的にソートするアイテムのリスト
        List<ItemData> sortItemList = new();

        if (isNormalSort && !isSwordManSort && !isMageSort)
        {
            foreach (var ItemImage in itemList)
            {
                ItemImage.SetActive(true);
            }

            sortItemList = ItemDataBase.SortItemsJobTypeAndNormal(job);

            if (isRaritySort)
            {
                sortItemList = ItemDataBase.GetItems(nowSortItemRarity,job);
            }

        }
        else
        {
            sortItemList = ItemDataBase.SortItemsJobType(job);

            if (isRaritySort)
            {
                sortItemList = ItemDataBase.SortItemsRarityAndJobType(nowSortItemRarity,job);
            }
        }




        foreach (var itemImage in itemList)
        {
            if (itemImage.activeInHierarchy)
            {
                foreach (var item in sortItemList)
                {
                    if (item.name == itemImage.name)
                    {
                        itemImage.SetActive(true);
                        visibleItemCount++;
                        break;
                    }
                    else
                    {
                        itemImage.SetActive(false);
                    }
                }
            }

        }

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (itemList.Count > 0 && visibleItemCount > 0)
        {

            //表示されているオブジェクトの最後尾を取得する
            GameObject lastActiveItem = itemList.LastOrDefault(obj => obj != null && obj.activeInHierarchy);

            //最後の要素のY軸位置を取得する
            Transform lastItemPos = lastActiveItem.transform;

            //最後の要素の位置からContentのサイズを変更する

            float contentHeight = Mathf.Abs(lastItemPos.localPosition.y) + bottomMargin;

            // 高さだけ変更する
            Vector2 size = rectTransform.sizeDelta;
            size.y = contentHeight;
            rectTransform.sizeDelta = size;
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(0,0);

            itemNoneText.enabled = true;

            foreach (var ItemImage in itemList)
            {
                ItemImage.SetActive(false);
            }
        }

    }


    //共通アイテムソート
    private async void NormalSortItem()
    {
        visibleItemCount = 0;

        if (!isRaritySort)
        {
            foreach (var ItemImage in itemList)
            {
                ItemImage.SetActive(true);
            }
        }
        else
        {
            foreach (var ItemImage in itemList)
            {
                foreach (var itemData in ItemDataBase.allItems)
                {
                    if (itemData.Rarity == nowSortItemRarity && ItemImage.name == itemData.name)
                    {
                        ItemImage.SetActive(true);
                    }

                }
            }
        }


        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        //一時的にソートするアイテムのリスト
        List<ItemData> sortItemList = new();

        if (isSwordManSort)
        {
            sortItemList = ItemDataBase.SortItemsJobTypeAndNormal(JobType.SwordMan);
        }
        else if (isMageSort)
        {
            sortItemList = ItemDataBase.SortItemsJobTypeAndNormal(JobType.Mage);
        }
        else
        {
            sortItemList = ItemDataBase.SortItemsJobType(JobType.Normal);
        }


        

        foreach (var itemImage in itemList)
        {
            if (itemImage.activeInHierarchy)
            {
                foreach (var item in sortItemList)
                {
                    if (item.name == itemImage.name)
                    {
                        itemImage.SetActive(true);
                        visibleItemCount++;
                        break;
                    }
                    else
                    {
                        itemImage.SetActive(false);
                    }
                }
            }

        }

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (itemList.Count > 0 && visibleItemCount > 0)
        {

            //表示されているオブジェクトの最後尾を取得する
            GameObject lastActiveItem = itemList.LastOrDefault(obj => obj != null && obj.activeInHierarchy);

            //最後の要素のY軸位置を取得する
            Transform lastItemPos = lastActiveItem.transform;

            //最後の要素の位置からContentのサイズを変更する

            float contentHeight = Mathf.Abs(lastItemPos.localPosition.y) + bottomMargin;

            // 高さだけ変更する
            Vector2 size = rectTransform.sizeDelta;
            size.y = contentHeight;
            rectTransform.sizeDelta = size;
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(0, 0);

            itemNoneText.enabled = true;

            foreach (var ItemImage in itemList)
            {
                ItemImage.SetActive(false);
            }
        }

    }





    //剣士アイテムソートボタン
    public void SwordManItemSort()
    {
        JobTypeSortItem(JobType.SwordMan);

        isSwordManSort = true;

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);


    }

    //魔導士アイテムソートボタン
    public void MageItemSort()
    {
        JobTypeSortItem(JobType.Mage);

        isMageSort = true;

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }

    //共通アイテムソート
    public void NormalItemSort()
    {
        NormalSortItem();

        isNormalSort = true;

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }


    //ソートリセットボタン
    public void SortReset()
    {
        ResetList();

        isSwordManSort = false;

        isMageSort = false;

        isNormalSort = false;

        isRaritySort = false;

        nowSortItemRarity = ItemRarity.None;

        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }


        //SE
        soundManager.PlaySE(CommonSoundType.NormalButton);

    }






    //レアリティソート
    private async void RaritySortItem(ItemRarity rarity)
    {
        visibleItemCount = 0;

        //一時的にソートするアイテムのリスト
        List<ItemData> sortItemList = new();

        //共通アイテムをソート状態か判定
        if (isNormalSort)
        {
            //剣士ソート時
            if (isSwordManSort)
            {
                //レアリティと共通アイテムと剣士アイテムソート
                sortItemList = ItemDataBase.GetItems(rarity, JobType.SwordMan);
            }
            //魔導士ソート時
            else if (isMageSort)
            {
                //レアリティと共通アイテムと魔導士アイテムソート
                sortItemList = ItemDataBase.GetItems(rarity, JobType.Mage);
            }
            //共通アイテムソートのみ
            else
            {
                //レアリティと共通アイテムソート
                sortItemList = ItemDataBase.GetItems(rarity, JobType.Normal);
            }
        }
        //共通アイテムソートをしていない時
        else
        {
            //剣士ソート時
            if (isSwordManSort)
            {
                //剣士アイテムとレアリティソート
                sortItemList = ItemDataBase.SortItemsRarityAndJobType(rarity, JobType.SwordMan);
            }
            //魔導士ソート時
            else if (isMageSort)
            {
                //魔導士アイテムとレアリティソート
                sortItemList = ItemDataBase.SortItemsRarityAndJobType(rarity, JobType.Mage);
            }
            //レアリティソートのみ
            else
            {
                //レアリティソート
                sortItemList = ItemDataBase.SortItemsRarity(rarity);
            }
        }


        foreach (var itemImage in itemList)
        {
            if (itemImage.activeInHierarchy)
            {
                foreach (var item in sortItemList)
                {
                    if (item.name == itemImage.name)
                    {
                        itemImage.SetActive(true);
                        visibleItemCount++;
                        break;
                    }
                    else
                    {
                        itemImage.SetActive(false);
                    }
                }
            }
        }

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (itemList.Count > 0 && visibleItemCount > 0)
        {

            //表示されているオブジェクトの最後尾を取得する
            GameObject lastActiveItem = itemList.LastOrDefault(obj => obj != null && obj.activeInHierarchy);

            //最後の要素のY軸位置を取得する
            Transform lastItemPos = lastActiveItem.transform;

            //最後の要素の位置からContentのサイズを変更する

            float contentHeight = Mathf.Abs(lastItemPos.localPosition.y) + bottomMargin;

            // 高さだけ変更する
            Vector2 size = rectTransform.sizeDelta;
            size.y = contentHeight;
            rectTransform.sizeDelta = size;
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(0, 0);

            itemNoneText.enabled = true;

            foreach (var ItemImage in itemList)
            {
                ItemImage.SetActive(false);
            }
        }

        isRaritySort = true;

        nowSortItemRarity = rarity;

    }


    //コモンアイテムソート
    public void CommonItemSort()
    {
        RaritySortItem(ItemRarity.Common);

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }

    //レアアイテムソート
    public void RareItemSort()
    {
        RaritySortItem(ItemRarity.Rare);

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }

    //エピックアイテムソート
    public void EpicItemSort()
    {
        RaritySortItem(ItemRarity.Epic);

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }

    //レジェンダリーアイテムソート
    public void LegendaryItemSort()
    {
        RaritySortItem(ItemRarity.Legendary);

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }

    //ユニークアイテムソート
    public void UniqueItemSort()
    {
        RaritySortItem(ItemRarity.Unique);

        //SE
        //soundManager.PlaySE(CommonSoundType.NormalButton);
    }


    private void OnEnable()
    {
        ResetList();

        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }

    }


    private void OnDisable()
    {

        isSwordManSort = false;

        isMageSort = false;

        isNormalSort = false;

        isRaritySort = false;

        nowSortItemRarity = ItemRarity.None;
    }



    //トグルソート
    public void UpdateFilter(string filterName, bool isOn)
    {
        if (isOn)
        {
            //SE
            soundManager.PlaySE(CommonSoundType.NormalButton);

            switch (filterName)
            {
                case "Normal":
                    NormalItemSort();
                    break;
                case "SwordMan":
                    SwordManItemSort();
                    break;
                case "Mage":
                    MageItemSort();
                    break;
                case "Common":
                    CommonItemSort();
                    break;
                case "Rare":
                    RareItemSort();
                    break;
                case "Epic":
                    EpicItemSort();
                    break;
                case "Legendary":
                    LegendaryItemSort();
                    break;
                case "Unique":
                    UniqueItemSort();
                    break;
            }
        }
        else
        {
            switch (filterName)
            {
                case "Normal":
                    isNormalSort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "SwordMan":
                    isSwordManSort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "Mage":
                    isMageSort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "Common":
                    isRaritySort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "Rare":
                    isRaritySort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "Epic":
                    isRaritySort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "Legendary":
                    isRaritySort = false;
                    ResetList();
                    RetrySort();
                    break;
                case "Unique":
                    isRaritySort = false;
                    ResetList();
                    RetrySort();
                    break;
            }
            
        }

        
    }



    //再検索関数
    private void RetrySort()
    {
        foreach (var t in toggles)
        {
            if (t.isOn)
            {
                switch (t.gameObject.name)
                {
                    case "Normal":
                        isNormalSort = false;
                        NormalItemSort();
                        break;
                    case "SwordMan":
                        isSwordManSort = false;
                        SwordManItemSort();
                        break;
                    case "Mage":
                        isMageSort = false;
                        MageItemSort();
                        break;
                    case "Common":
                        CommonItemSort();
                        break;
                    case "Rare":
                        RareItemSort();
                        break;
                    case "Epic":
                        EpicItemSort();
                        break;
                    case "Legendary":
                        LegendaryItemSort();
                        break;
                    case "Unique":
                        UniqueItemSort();
                        break;
                }
            }
        }
    }

}
