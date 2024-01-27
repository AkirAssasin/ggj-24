using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Akir
{
    /* Akir's coroutine class
     * 
     * advantages over Unity coroutines:
     * - can be paused and unpaused
     * - can check if running
     * 
     * disadvantages over Unity coroutines:
     * - not fire-and-forget
     * - probably slower
     * - can't use Unity yield instructions
    */
    public class Coroutine
    {
        public bool Paused { get; set; }
    
        // end action
        Action m_onEndAction;
    
        // stack to execute
        IEnumerator m_wrapperCoroutine;
        readonly Stack<IEnumerator> m_stack = new();
        public bool Running => m_stack.Count > 0;
    
        // caller
        readonly MonoBehaviour m_caller;
    
        // constructor
        public Coroutine(MonoBehaviour caller)
        {
            m_caller = caller;
        }
    
        // start coroutine
        public void Start(IEnumerator coroutine, Action onEndAction = null)
        {
            // to start, you need to stop
            Stop();
    
            // add to stack
            m_stack.Push(coroutine);
    
            // start running
            Paused = false;
            m_onEndAction = onEndAction;
            m_wrapperCoroutine = WrapperCoroutine();
            m_caller.StartCoroutine(m_wrapperCoroutine);
        }
    
        IEnumerator WrapperCoroutine()
        {
            // keep running
            while (Running)
            {
                if (Paused)
                {
                    // if paused, do nothing
                    yield return null;
                    continue;
                }
                
                // if not paused, movenext for top of stack
                IEnumerator top = m_stack.Peek();
                if (top.MoveNext())
                {
                    // if top yielded something else, add to top of stack
                    if (top.Current is IEnumerator current)
                    {
                        m_stack.Push(current);
                    }
    
                    yield return null;
                    continue;
                }
    
                // if coroutine was stopped by coroutine, catch it here
                if (!Running)
                    break;
                
                // top has finished running
                m_stack.Pop();
            }
    
            // ended, do end action
            m_onEndAction?.Invoke();
            m_wrapperCoroutine = null;
        }
    
        public void Stop()
        {
            if (m_wrapperCoroutine == null)
                return;
    
            m_stack.Clear();
            m_caller.StopCoroutine(m_wrapperCoroutine);
            m_onEndAction = null;
            m_wrapperCoroutine = null;
        }
    }

    // wait for seconds
    // you need to use this instead of Unity's built in one
    // if you are using the custom coroutine
    public class WaitForSeconds : CustomYieldInstruction
    {
        float m_duration;
        float m_t = 0;
        readonly Func<float> m_timeScaleFunc;

        // increment time
        public override bool keepWaiting
        {
            get
            {
                if (m_t >= m_duration)
                    return false;

                m_t += Time.unscaledDeltaTime * m_timeScaleFunc();
                return true;
            }
        }

        // constructor
        public WaitForSeconds(float duration, Func<float> timeScaleFunc = null)
        {
            m_duration = duration;
            m_timeScaleFunc = timeScaleFunc ?? (() => Time.timeScale);
        }

        // for reuse
        public WaitForSeconds Reuse(float duration)
        {
            m_duration = duration;
            m_t = 0;
            return this;
        }
    }
}