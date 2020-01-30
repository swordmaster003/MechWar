using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletShell : GameObjectPoolItem
{
    private float m_Timer;

    public float recycleTime;


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
