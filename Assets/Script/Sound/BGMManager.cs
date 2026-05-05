using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BGMの名前設定
public enum BGM
{
    title,shop,battle,boss,gameClear,gameOver,waveClear,
}

public class BGMManager : MastarSoundManager<BGM>
{
    //シングルトン化
    public static BGMManager instance { get; private set; }

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
}
