using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : GameObjectPoolItem
{
    private ParticleSystem[] m_PS;

    private float m_Timer;

    public float recycleTime;

    void Awake()
    {
        m_PS = GetComponentsInChildren<ParticleSystem>();

        float maxValue = 0.0f;

        for (int i = 0; i < m_PS.Length; i++)
        {
            if (m_PS[i].main.startLifetime.constantMax > maxValue)
            {
                maxValue = m_PS[i].main.startLifetime.constantMax;
            }
        }

        recycleTime = maxValue;
    }

    public void OnEnable()
    {
        m_Timer = Time.time;
    }

    public void Update()
    {
        if (Time.time >= m_Timer + recycleTime)
        {
            Recycle();
        }
    }

}
