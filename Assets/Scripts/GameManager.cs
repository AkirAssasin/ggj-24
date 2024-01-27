using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //static instance
    public static GameManager Instance { get; private set; }

    //player reference
    [field:SerializeField] public PlayerController Player { get; private set; }
    [field:SerializeField] public CameraController Camera { get; private set; }

    //enemies
    [SerializeField] GameObject m_enemyPrefab;

    //checklist
    [SerializeField] RectTransform m_checklistParent;
    [SerializeField] GameObject m_checklistItemPrefab;

    //routines
    [SerializeField] List<RoutineController> m_routines;

    void Awake()
    {
        if (Instance != null) throw new System.Exception();
        Instance = this;
    }

    void Start()
    {
        foreach (RoutineController routine in m_routines)
        {
            GameObject checklistItem = Instantiate(m_checklistItemPrefab, m_checklistParent);
            routine.Initialize(checklistItem);

            routine.Enable();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SpawnEnemy(Vector2 position)
    {
        EnemyController enemy = EnemyController.GetFromPool(m_enemyPrefab);
        enemy.Initialize(position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SpawnEnemy(Random.insideUnitCircle * 5f);
    }
}
