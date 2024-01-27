using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// poolable interface
public interface IPoolable
{
    bool InPool { get; set; }
}

// pool container
public class GameObjectPool<T> where T : MonoBehaviour, IPoolable
{
    readonly List<T> m_actives = new();
    readonly List<T> m_pool = new();

    public int ActiveCount => m_actives.Count;

    public T GetFromPool(GameObject prefab)
    {
        T result;

        if (m_pool.Count > 0)
        {
            result = m_pool[^1];
            m_pool.RemoveAt(m_pool.Count - 1);
        }
        else
        {
            result = Object.Instantiate(prefab).GetComponent<T>();
        }

        // I don't know if this works
        result.InPool = false;
        m_actives.Add(result);
        return result;
    }
    
    public void Remove(T toRemove)
    {
        if (toRemove.InPool)
        {
            m_pool.Remove(toRemove);
        }
        else m_actives.Remove(toRemove);
    }

    public bool Pool(T toPool)
    {
        if (toPool.InPool)
            return false;

        toPool.InPool = true;
        m_actives.Remove(toPool);
        m_pool.Add(toPool);
        return true;
    }
}

// C# doesn't allow for inheriting from type parameters
// so if you need to do inheritance, put these into the derived class
public class PoolableObject<T> : MonoBehaviour, IPoolable where T : PoolableObject<T>
{
    static readonly GameObjectPool<T> s_pool = new();
    
    public static int ActiveCount => s_pool.ActiveCount;

    bool m_inPool = true;
    public bool InPool { get => m_inPool; }
    bool IPoolable.InPool { get => m_inPool; set => m_inPool = value; }
    public static T GetFromPool(GameObject prefab) => s_pool.GetFromPool(prefab);
    protected virtual void OnDestroy() => s_pool.Remove(this as T);
    protected bool Pool () => s_pool.Pool(this as T);
}