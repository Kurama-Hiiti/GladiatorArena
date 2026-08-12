using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowShopRate : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    //ショップの文字にマウスカーソルが重なった時に現在のショップのレアリティの出現レートを表示する

    public void OnPointerEnter(PointerEventData eventData)
    {
        // マネージャーに対して「自分を表示して」と命令する
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            ShowShopRateManager.instance.ShowRatePopUp();
        }
        
    }

    //マウスカーソルが離れたとき非表示
    public void OnPointerExit(PointerEventData eventData)
    {
        ShowShopRateManager.instance.HiddenRatePopUp();
    }

}
