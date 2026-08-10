using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    //シングルトン化
    public static SceneController instance { get; private set; }

    //サウンドマネージャー
    [SerializeField]
    private CommonSoundManager soundManager;

    [Header("フェード設定")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float durationWaitTime = 0.5f;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    //タイトルシーン遷移
    public void LoadTitle()
    {
        //SE
        soundManager.PlaySE(CommonSoundType.TitleBackButton);

        //フェードアウト用画像表示
        fadeImage.enabled = true;
        
        //シーンのロード
        StartCoroutine(FadeAndLoadScene("GameMain"));
    }

    // フェードアウトしてからシーンを読み込む
    private IEnumerator FadeAndLoadScene(string sceneName)
    {

        yield return StartCoroutine(Fade(1)); // フェードアウト

        //timeScaleを初期値へ
        Time.timeScale = 1.0f;

        //フェードアウトしてから一定時間の後シーン遷移
        yield return new WaitForSeconds(durationWaitTime); 
        SceneManager.LoadScene(sceneName);
    }

    //フェードアウト関数
    private IEnumerator Fade(float targetAlpha)
    {
        //フェードアウト画像のα値定義
        float startAlpha = fadeImage.color.a;
        //経過時間定義
        float time = 0f;

        //既定（fadeDuration）の時間までループする
        while (time < fadeDuration)
        {
            //経過時間増加(timeScaleに依存しない)
            time += Time.unscaledDeltaTime;

            //経過時間とfadeDurationの割合でα値を変化させる
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);

            //計算したα値を適応
            fadeImage.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        //最終的なα値を適応
        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }


}
