using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIndividualData : MonoBehaviour
{
    //アイテムプレファブに個別にアタッチするスクリプト

    //セットする位置
    public Vector3 itemSetPos;

    //初期位置
    private Vector3 originPos;

    //アイテムデータベース
    [SerializeField]
    private ItemDataBase itemDataBase;

    //このアイテムのタイプ
    private ItemType type;

    //スペアフレーム内にアイテムがあるか
    public bool isSpare;

    //売却フラグ
    public bool isSell;


    private void Start()
    {

        //アイテムのタイプを取得
        foreach (ItemData list in itemDataBase.allItems)
        {
            if (this.name == list.name)
            {
                type = list.ItemType;
            }
        }

        //初期位置設定
        itemSetPos = transform.position;
        originPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {

            //スペアフレーム
            if (collision.gameObject.CompareTag("SpareFrame"))
            {
                //フレームにセットされているアイテムが無い時
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    //位置更新
                    itemSetPos = collision.transform.position;

                    //スペアアイテムフラグ更新
                    isSpare = true;

                    //スロットのセットフラグ更新
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }

            }

            //武器フレーム
            else if (collision.gameObject.CompareTag("WeaponFrame") && type == ItemType.Weapon)
            {
                
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //セカンダリーフレーム
            else if (collision.gameObject.CompareTag("SecondaryFrame") && type == ItemType.Secondary)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //ヘルムフレーム
            else if (collision.gameObject.CompareTag("HelmFrame") && type == ItemType.Helm)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //アーマーフレーム
            else if (collision.gameObject.CompareTag("ArmorFrame") && type == ItemType.Armor)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //グローブフレーム
            else if (collision.gameObject.CompareTag("GloveFrame") && type == ItemType.Glove)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //シューズフレーム
            else if (collision.gameObject.CompareTag("ShoesFrame") && type == ItemType.Boots)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //アクセサリーフレーム
            else if (collision.gameObject.CompareTag("AccessoryFrame") && type == ItemType.Accessory)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            //ポーションフレーム
            else if (collision.gameObject.CompareTag("PotionFrame") && type == ItemType.Potion)
            {
                if (!collision.gameObject.GetComponent<FrameItemCheck>().isSet)
                {
                    itemSetPos = collision.transform.position;
                    isSpare = false;
                    collision.gameObject.GetComponent<FrameItemCheck>().isSet = true;
                }
            }

            else if (collision.gameObject.CompareTag("SellSpace"))
            {
                isSell = true;
            }
        }

    }

    //フレームから外れた時
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (GameManager.instance.state == GameManager.GameState.Shop)
        {
            //スペアフレーム
            //購入していないアイテムをフレームに入れてから購入するのをやめてフレームから外したときに元の商品陳列位置に戻す
            if (collision.gameObject.CompareTag("SpareFrame") && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;

            }

            //武器フレーム
            else if (collision.gameObject.CompareTag("WeaponFrame") && type == ItemType.Weapon && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //セカンダリーフレーム
            else if (collision.gameObject.CompareTag("SecondaryFrame") && type == ItemType.Secondary && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //ヘルムフレーム
            else if (collision.gameObject.CompareTag("HelmFrame") && type == ItemType.Helm && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //アーマーフレーム
            else if (collision.gameObject.CompareTag("ArmorFrame") && type == ItemType.Armor && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //グローブフレーム
            else if (collision.gameObject.CompareTag("GloveFrame") && type == ItemType.Glove && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //シューズフレーム
            else if (collision.gameObject.CompareTag("ShoesFrame") && type == ItemType.Boots && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //アクセサリーフレーム
            else if (collision.gameObject.CompareTag("AccessoryFrame") && type == ItemType.Accessory && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }

            //ポーションフレーム
            else if (collision.gameObject.CompareTag("PotionFrame") && type == ItemType.Potion && !IsHaveItem())
            {
                itemSetPos = originPos;
                isSpare = false;
            }


            if (collision.gameObject.CompareTag("SellSpace"))
            {
                isSell = false;
            }


        }



    }


    //プレイヤーのアイテムリストに格納されているのかチェック
    private bool IsHaveItem()
    {
        //ここでプレイヤーが所持しているのか判定
        foreach (GameObject list in GameManager.instance.player.GetComponent<Player>().itemImageList)
        {
            if (list == this.gameObject)
            {
                return true;
            }
        }

        return false;

    }
}
