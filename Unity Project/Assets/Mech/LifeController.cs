using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeController : MonoBehaviour
{
    public int lifeValue = 100;
    private int m_LifeCurrentValue;

    public void OnEnable()
    {
        m_LifeCurrentValue = lifeValue;
    }

    public void TakeDamage(int deltaValue)
    {
        if (m_LifeCurrentValue > 0)
        {
            m_LifeCurrentValue = m_LifeCurrentValue - deltaValue;
        }

        if(m_LifeCurrentValue < 0)
        {
            m_LifeCurrentValue = 0;
        }
    }

    public void Update()
    {
        if (m_LifeCurrentValue == 0)
        {
            Destroy(this.gameObject);
        }
    }
}
