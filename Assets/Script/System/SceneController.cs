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
        soundManager.PlaySE(CommonSoundType.TitleBackButton);
        fadeImage.enabled = true;
        StartCoroutine(FadeAndLoadScene("GameMain"));
    }

    // フェードアウトしてからシーンを読み込む
    private IEnumerator FadeAndLoadScene(string sceneName)
    {

        yield return StartCoroutine(Fade(1)); // フェードアウト
        Time.timeScale = 1.0f;
        yield return new WaitForSeconds(durationWaitTime); //フェードアウトしてから一定時間の後シーン遷移
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }


}
