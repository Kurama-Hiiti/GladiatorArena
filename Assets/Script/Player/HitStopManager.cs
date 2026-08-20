using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class HitStopManager : MonoBehaviour
{
    //シングルトン化
    public static HitStopManager instance { get; private set; }

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

    public void HitStop(float duration)
    {
        StartCoroutine(DoStop(duration));
    }

    //ヒットストップ処理
    IEnumerator DoStop(float duration)
    {
        Time.timeScale = 0.05f; // 完全に0にすると不自然な場合があるため、わずかに動かす
        yield return new WaitForSecondsRealtime(duration); // リアルタイムで待機
        Time.timeScale = 1.0f;
    }


}
