using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentCycler : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] m_renderers;
    Color[] m_colors;
    [SerializeField] float m_durationPerCycle, m_offAlpha, m_onAlpha;
    Akir.Coroutine m_coroutine;

    // Start is called before the first frame update
    void Start()
    {
        m_colors = new Color[m_renderers.Length];
        for (int X = 0; X < m_colors.Length; ++X) m_colors[X] = m_renderers[X].color;

        IEnumerator CyclerCoroutine()
        {
            Akir.WaitForSeconds wait = new Akir.WaitForSeconds(m_durationPerCycle);
            while (true)
            {
                for (int X = 0; X < m_renderers.Length; ++X)
                {
                    for (int Y = 0; Y < m_renderers.Length; ++Y)
                    {
                        m_colors[Y].a = (X == Y) ? m_onAlpha : m_offAlpha;
                        m_renderers[Y].color = m_colors[Y];
                    }
                    yield return wait.Reuse(m_durationPerCycle);
                }
            }
        }
        m_coroutine = new Akir.Coroutine(this);
        m_coroutine.Start(CyclerCoroutine());
    }
}
