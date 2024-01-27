using System;
using System.Collections.Generic;
using UnityEngine;

// augmentable unmanaged type (e.g. int, float)
[Serializable]
public class Augmentable<T> where T : unmanaged
{
    // augments
    public delegate T AugmentDelegate(T value);
    public class Augment : IDisposable
    {
        Augmentable<T> m_augmentable;

        // you can modify the delegate but try not to
        AugmentDelegate m_delegate;
        public AugmentDelegate Delegate
        {
            get => m_delegate;
            set
            {
                m_delegate = value;
                m_augmentable.UpdateAugmentValues(this);
            }
        }

        public readonly object m_source;
        public readonly int m_order;
        public T m_value;

        // do not call this please
        public Augment(Augmentable<T> augmentable, AugmentDelegate @delegate, object source, int order)
        {
            m_augmentable = augmentable;
            m_delegate = @delegate;
            m_source = source;
            m_order = order;
        }

        // call this if your delegate output changes for some weird reason (please try not to)
        public void UpdateAugmentValues() => m_augmentable.UpdateAugmentValues(this);

        // dispose
        public void Dispose()
        {
            // remove self from augment
            m_augmentable.RemoveAugment(this);
        }

    }
    readonly List<Augment> m_augments = new();

    // base value
    [SerializeField] T m_baseValue = default;
    public T BaseValue
    {
        get => m_baseValue;
        set
        {
            if (m_baseValue.Equals(value)) return;
            m_baseValue = value;
            UpdateAugmentValues(0);
        }
    }

    // final value
    T? m_finalValue = null;
    public T FinalValue => m_finalValue ?? m_baseValue;

    // converter
    public static implicit operator T(Augmentable<T> augmentable) => augmentable.FinalValue;

    // add augment
    public Augment AddAugment(AugmentDelegate @delegate, object source, int order = 0)
    {
        // find index to insert
        int index = m_augments.Count;
        while (index > 0)
            if (m_augments[index - 1].m_order > order) --index;
        
        // insert augment and update value
        Augment augment = new(this, @delegate, source, order);
        m_augments.Insert(index, augment);
        UpdateAugmentValues(index);

        // return augment
        return augment;
    }

    // update augment - call this when augment is modified
    public void UpdateAugmentValues(Augment augment) => UpdateAugmentValues(m_augments.IndexOf(augment));

    // remove augment
    public void RemoveAugment(Augment augment)
    {
        int index = m_augments.IndexOf(augment);
        if (index == -1) return;

        m_augments.RemoveAt(index);
        UpdateAugmentValues(index);
    }

    // remove augments
    public void RemoveAugmentsFrom(object source)
    {
        m_augments.RemoveAll(a => a.m_source == source);
        UpdateAugmentValues(0);
    }

    // update augment values
    private void UpdateAugmentValues(int startIndex)
    {
        T previousValue = (startIndex == 0 ? m_baseValue : m_augments[startIndex - 1].m_value);
        for (int i = startIndex; i < m_augments.Count; ++i)
        {
            Augment augment = m_augments[i];
            previousValue = augment.m_value = augment.Delegate(previousValue);
        }
        m_finalValue = previousValue;
    }

    // reset
    public void Reset(T value)
    {
        m_augments.Clear();
        BaseValue = value;
    }
}
