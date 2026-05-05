using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    //スポーンする敵のプレファブリスト
    [SerializeField]
    private List<GameObject> enemyList;

    //敵の種類
    public class EnemySpqwnNum
    {
        public int zombie;
        public int highZombie;
        public int mutant;
        public int minotaur;
        public int beetle;
        public int golem;
    }




    //敵のスポーン数
    private List<EnemySpqwnNum> enemySpawnNum = new()
    {
        new EnemySpqwnNum { zombie = 10, highZombie = 0, mutant = 0, minotaur = 0, beetle = 0, golem = 0 },
        new EnemySpqwnNum { zombie =  8, highZombie = 3, mutant = 0, minotaur = 0, beetle = 0, golem = 0 },
        new EnemySpqwnNum { zombie =  0, highZombie = 8, mutant = 0, minotaur = 0, beetle = 0, golem = 0 },
        new EnemySpqwnNum { zombie =  5, highZombie = 0, mutant = 0, minotaur = 0, beetle = 5, golem = 0 },
        new EnemySpqwnNum { zombie =  5, highZombie = 4, mutant = 0, minotaur = 0, beetle = 3, golem = 0 },
        new EnemySpqwnNum { zombie = 10, highZombie = 0, mutant = 0, minotaur = 3, beetle = 5, golem = 0 },
        new EnemySpqwnNum { zombie =  5, highZombie = 0, mutant = 1, minotaur = 5, beetle = 0, golem = 0 },
        new EnemySpqwnNum { zombie =  0, highZombie = 0, mutant = 5, minotaur = 0, beetle = 0, golem = 0 },
        new EnemySpqwnNum { zombie =  5, highZombie = 4, mutant = 1, minotaur = 2, beetle = 3, golem = 0 },
        new EnemySpqwnNum { zombie = 10, highZombie = 0, mutant = 2, minotaur = 0, beetle = 0, golem = 1 }
    };

    //敵のスポーン数
    private EnemySpqwnNum spawnNum;

    //スポーンした敵を格納するリスト(敵の有無を確認するため)
    private List<GameObject> spawnList = new List<GameObject>();

    //敵スポーンチェック
    private bool isEnemySpawn = false;


    //テスト用スポーン
    private void Start()
    {

    }

    private void Update()
    {
        //バトル状態且つ敵がスポーンしていなかったら敵スポーン
        if (GameManager.instance.state == GameManager.GameState.Battle && !isEnemySpawn)
        {
            isEnemySpawn = true;
            spawnNum = GetSpawnNum(GameManager.instance.waveNum);
            EnemySpawn();
        }

        if (GameManager.instance.state == GameManager.GameState.Battle)
        {
            EnemyCheck();
        }
        
    }

    //敵スポーン関数
    private void EnemySpawn()
    {
        //ゾンビスポーン
        for (int i = 0; i < spawnNum.zombie; i++)
        {
            GameObject enemy = Instantiate(enemyList[0], new Vector3(SpawnEnemyPositonX(), 0.1f, SpawnEnemyPositonZ()), Quaternion.identity);

            enemy.transform.parent = transform;

            spawnList.Add(enemy);

        }

        //ハイゾンビスポーン
        for (int i = 0; i < spawnNum.highZombie; i++)
        {
            GameObject enemy = Instantiate(enemyList[1], new Vector3(SpawnEnemyPositonX(), 0.1f, SpawnEnemyPositonZ()), Quaternion.identity);

            enemy.transform.parent = transform;

            spawnList.Add(enemy);

        }

        //ミュータントスポーン
        for (int i = 0; i < spawnNum.mutant; i++)
        {
            GameObject enemy = Instantiate(enemyList[2], new Vector3(SpawnEnemyPositonX(), 0.1f, SpawnEnemyPositonZ()), Quaternion.identity);

            enemy.transform.parent = transform;

            spawnList.Add(enemy);

        }

        //ミノタウロススポーン
        for (int i = 0; i < spawnNum.minotaur; i++)
        {
            GameObject enemy = Instantiate(enemyList[3], new Vector3(SpawnEnemyPositonX(), 0.1f, SpawnEnemyPositonZ()), Quaternion.identity);

            enemy.transform.parent = transform;

            spawnList.Add(enemy);

        }

        //ビートルスポーン
        for (int i = 0; i < spawnNum.beetle; i++)
        {
            GameObject enemy = Instantiate(enemyList[4], new Vector3(SpawnEnemyPositonX(), 0.1f, SpawnEnemyPositonZ()), Quaternion.identity);

            enemy.transform.parent = transform;

            spawnList.Add(enemy);

        }

        //ゴーレムスポーン
        for (int i = 0; i < spawnNum.golem; i++)
        {
            GameObject enemy = Instantiate(enemyList[5], new Vector3(SpawnEnemyPositonX(), 0.1f, SpawnEnemyPositonZ()), Quaternion.identity);

            enemy.transform.parent = transform;

            spawnList.Add(enemy);

        }

    }


    //敵のスポーン位置計算関数
    private float SpawnEnemyPositonX()
    {
        float x = Random.Range(-25f,25f);

        return x;

    }

    private float SpawnEnemyPositonZ()
    {
        float z = Random.Range(-25f, 25f);

        return z;

    }


    //敵の数のチェック
    private void EnemyCheck()
    {
        if (spawnList.Count != 0)
        {
            for (int i = 0; i < spawnList.Count; i++)
            {
                GameObject obj = spawnList[i];
                if (obj == null)
                {
                    spawnList.RemoveAt(i);
                }

            }

            if (spawnList.Count == 0)
            {
                GameManager.instance.state = GameManager.GameState.GameClear;

                UIManager.instance.ShowClearUI();

                //もう一度敵が現れるようにする
                isEnemySpawn = false;
            }
        }


    }

    private EnemySpqwnNum GetSpawnNum(int waveNum)
    {
        if (enemySpawnNum.Count <= waveNum - 1)
        {
            return enemySpawnNum[enemySpawnNum.Count - 1];
        }


        return enemySpawnNum[waveNum - 1];
    }


}
