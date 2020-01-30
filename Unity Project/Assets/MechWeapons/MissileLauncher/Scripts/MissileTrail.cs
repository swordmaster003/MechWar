using UnityEngine;
using System.Collections;

public class MissileTrail : GameObjectPoolItem 
{
    private ParticleSystem[] m_PS;

    private float m_Timer;

    public float recycleTime;

	void Awake()
	{
        m_PS = GetComponentsInChildren<ParticleSystem>();

        float maxValue = 0.0f;

        for(int i = 0 ;i<m_PS.Length;i++)
        {
            if(m_PS[i].main.startLifetime.constantMax > maxValue)
            {
                maxValue = m_PS[i].main.startLifetime.constantMax;
            }
        }

        recycleTime = maxValue;
	}

    public void Update()
    {
        if (this.transform.parent != null)
        {
            return;
        }

        if (Time.time >= m_Timer + recycleTime)
        {
            Recycle();
        }

    }

    public void Attach(Transform parent)
    {
        this.transform.SetParent(parent);
    }

    public void Detach()
    {
        for (int i = 0; i < m_PS.Length; i++)
        {
            m_PS[i].Stop(true);
        }

        this.transform.SetParent(null);

        m_Timer = Time.time;
    }

}
