using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    //エネミー取得
    [SerializeField]
    private EnemyManager enemy;

    //HP取得
    private IntReactiveProperty enemyHp = new IntReactiveProperty(0);

    //HPバーの画像
    [SerializeField]
    private Image hpBar;

    //HPバーの遷移時間
    [SerializeField]
    private float barDuration;



    private void Start()
    {
        enemyHp
            .ObserveEveryValueChanged(hp => enemyHp.Value = enemy.currentHealth)
            .Subscribe(hp => EnemyHpUIManagement(hp));
    }


    private void EnemyHpUIManagement(int hp)
    {

        float targetValue = (float)hp / (float)enemy.maxHealth;

        hpBar.DOFillAmount(targetValue, barDuration);

    }

}
