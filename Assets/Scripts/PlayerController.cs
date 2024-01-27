using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float m_walkSpeed, m_latchedSpeedMultiplier;

    //latching mechanic
    public bool IsLatched { get; private set; } = false;

    //components
    Rigidbody2D m_rigidbody;
    public Vector2 Position => m_rigidbody.position;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        
    }

    void Update()
    {
        //update latching
        if (IsLatched) UpdateLatch();

        //get normalized input
        Vector2 input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        //set as velocity
        float speed = m_walkSpeed;
        if (IsLatched) speed *= m_latchedSpeedMultiplier;
        m_rigidbody.velocity = input * speed;
    }

    void UpdateLatch()
    {
        //update le quicktime event
        if (Input.GetKeyDown(KeyCode.Z))
        {
            EnemyController.GetFromPool(null).Initialize(Position);
            IsLatched = false;
        }
    }

    public void LatchOn()
    {
        //begin le quicktime event
        IsLatched = true;
    }
}
