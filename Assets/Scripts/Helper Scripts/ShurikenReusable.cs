using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ShurikenReusable : MonoBehaviour
{
    public class Handle
    {
        ShurikenReusable m_reusable;
    
        public Handle(ShurikenReusable reusable)
        {
            m_reusable = reusable;
        }

        public bool Valid => m_reusable != null;
        public void Invalidate() => m_reusable = null;

        public void SetParent(Transform transform)
        {
            m_reusable.m_transform.SetParent(transform, true);   
            m_reusable.m_transform.localScale = Vector3.one;
        }

        public void Stop()
        {
            SetParent(null);
            m_reusable.m_system.Stop();
        }
    }

    // dictionary
    static readonly Dictionary<GameObject, List<ShurikenReusable>> s_pools = new();

    // spawn from pool
    public static Handle SpawnFromPool(GameObject prefab, Vector3 position, Vector3 scale, Quaternion rotation, Color color)
    {
        // if no existing pool, make one
        if (!s_pools.TryGetValue(prefab, out var pool))
        {
            pool = new List<ShurikenReusable>();
            s_pools.Add(prefab, pool);
        }

        // get from pool
        ShurikenReusable result;
        if (pool.Count > 0)
        {
            result = pool[^1];
            pool.RemoveAt(pool.Count - 1);
            result.m_transform.position = position;
            result.m_transform.rotation = rotation;
            result.m_transform.localScale = Vector3.one;
        }
        else
        {
            result = Instantiate(prefab, position, rotation).GetComponent<ShurikenReusable>();
            result.m_original = prefab;
        }
        result.m_transform.localScale = scale;

        // initialize
        result.m_handle = new Handle(result);
        var main = result.m_system.main;
        main.startColor = color;
        result.m_system.Play();
        result.m_poolOnStop = true;
        return result.m_handle;
    }

    // original
    bool m_poolOnStop;
    GameObject m_original;
    Transform m_transform;
    ParticleSystem m_system;

    // current handle
    Handle m_handle;

    // awake
    void Awake()
    {
        m_system = GetComponent<ParticleSystem>();
        m_transform = GetComponent<Transform>();
    }

    // remove from pool when destroyed
    void OnDestroy()
    {
        m_handle.Invalidate();

        if (s_pools.TryGetValue(m_original, out var pool))
            pool.Remove(this);
    }

    // pool on system stop
    private void Update()
    {
        if (m_poolOnStop && m_system.isStopped)
        {
            m_handle.Invalidate();
            m_transform.SetParent(null, false);

            m_poolOnStop = false;
            if (s_pools.TryGetValue(m_original, out var pool))
                pool.Add(this);
        }
    }
}
