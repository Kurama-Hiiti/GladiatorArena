using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/AnimationSpeedData")]
public class AnimationSpeedData : ScriptableObject
{
    //データを記入する構造体を作成
    [System.Serializable]
    public struct AnimationSpeedEntry
    {
        public string animationName; // アニメーション名（ステート名 or クリップ名）
        public float baseSpeed;      // 基礎速度
    }

    //モーションデータの構造体をリスト化
    [SerializeField]
    private List<AnimationSpeedEntry> animationSpeeds = new List<AnimationSpeedEntry>();

    //Dictionaryのキャッシュ
    private Dictionary<string, float> speedDictionary;

    // 名前から速度を取得（見つからなければ1.0f）
    public float GetBaseSpeed(string animationName)
    {
        // 初回アクセス時に辞書化（一度作成して処理を軽くする）
        if (speedDictionary == null)
        {
            speedDictionary = new Dictionary<string, float>();

            //アニメーションのデータをDictionaryへ格納
            foreach (AnimationSpeedEntry entry in animationSpeeds)
            {
                //アニメーションの名称がすでに登録されていなければ登録する
                if (!speedDictionary.ContainsKey(entry.animationName))
                    speedDictionary.Add(entry.animationName, entry.baseSpeed);
            }
        }

        //アニメーションの名称と速度を格納した辞書から速度を取得する
        return speedDictionary.TryGetValue(animationName, out float speed) ? speed : 1.0f;
    }

}
