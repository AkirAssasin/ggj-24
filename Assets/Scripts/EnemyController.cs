using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : PoolableObject<EnemyController>
{
    [SerializeField] float m_walkSpeed;

    //coroutine
    Akir.Coroutine m_spawnCoroutine;

    //components
    Transform m_transform;
    Rigidbody2D m_rigidbody;
    Collider2D m_collider;
    SpriteRenderer m_renderer;

    void Awake()
    {
        m_spawnCoroutine = new Akir.Coroutine(this);

        m_transform = GetComponent<Transform>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
        m_renderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2 position)
    {
        m_transform.position = position;

        m_rigidbody.simulated = true;
        m_collider.enabled = true;
        m_renderer.enabled = true;

        IEnumerator SpawnWaitHack() { yield return new Akir.WaitForSeconds(2); }
        m_spawnCoroutine.Start(SpawnWaitHack());
    }

    public new void Pool()
    {
        if (base.Pool())
        {
            m_spawnCoroutine.Stop();

            m_rigidbody.simulated = false;
            m_collider.enabled = false;
            m_renderer.enabled = false;
        }
    }

    void Update()
    {
        if (m_spawnCoroutine.Running || InPool) return;

        //move towards player
        PlayerController player = GameManager.Instance.Player;
        Vector2 input = player.Position - m_rigidbody.position;
        input.Normalize();

        //set as velocity
        m_rigidbody.velocity = input * m_walkSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //can latch onto player?
        if (m_spawnCoroutine.Running || GameManager.Instance.IsEnteringNextDay || GameManager.Instance.Player.IsLatched) return;

        //try get player
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        //latch onto player, then pool
        player.LatchOn();
        Pool();
    }
}
