using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDataBase", menuName = "ScriptableObject/ItamDataBase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> allItems;


    //ショップへの陳列用のアイテム取得処理
    public List<ItemData> GetItems(ItemRarity rarity, JobType job)
    {
        return allItems
            .Where(i => i.Rarity == rarity)
            .Where(i => i.JobType == JobType.Normal || i.JobType == job)
            .ToList();
    }


}
