using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //static instance
    public static GameManager Instance { get; private set; }

    //player reference
    [field:SerializeField] public PlayerController Player { get; private set; }

    //prefab
    [SerializeField] GameObject m_enemyPrefab;

    void Awake()
    {
        if (Instance != null) throw new System.Exception();
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnemyController enemy = EnemyController.GetFromPool(m_enemyPrefab);
            enemy.Initialize(Random.insideUnitCircle * 5);
        }
    }
}
