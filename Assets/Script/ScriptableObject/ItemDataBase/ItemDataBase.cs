using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDataBase", menuName = "ScriptableObject/ItamDataBase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> allItems;


    //ショップへの陳列用
    public List<ItemData> GetItems(ItemRarity rarity, JobType job)
    {
        return allItems
            .Where(i => i.Rarity == rarity)
            .Where(i => i.JobType == JobType.Normal || i.JobType == job)
            .ToList();
    }

    //共通アイテム抜きジョブソート用
    public List<ItemData> SortItemsJobType(JobType job)
    {
        return allItems
            .Where(i => i.JobType == job)
            .ToList();
    }


    //レアリティソート用
    public List<ItemData> SortItemsRarity(ItemRarity rarity)
    {
        return allItems
            .Where(i => i.Rarity == rarity)
            .ToList();
    }


    //共通アイテム入りジョブタイプソート用
    public List<ItemData> SortItemsJobTypeAndNormal(JobType job)
    {
        return allItems
            .Where(i => i.JobType == JobType.Normal || i.JobType == job)
            .ToList();
    }


    //アイテムのレアリティとジョブタイプソート用（共通アイテム抜き）
    public List<ItemData> SortItemsRarityAndJobType(ItemRarity rarity, JobType job)
    {
        return allItems
            .Where(i => i.Rarity == rarity)
            .Where(i => i.JobType == job)
            .ToList();
    }


}
