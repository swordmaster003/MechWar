using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpact : GameObjectPoolItem
{
    private BulletHoleLogic[] m_BulletHoleLogicArray;

    public void Awake()
    {
        m_BulletHoleLogicArray = this.GetComponentsInChildren<BulletHoleLogic>();
    }

    public void OnEnable()
    {
        
    }

    public void Update()
    {
        bool fadeFinish = true;

        for(int i = 0;i<m_BulletHoleLogicArray.Length;i++)
        {
            BulletHoleLogic bulletHoleLogic = m_BulletHoleLogicArray[i];

            if (bulletHoleLogic.currentTintAlpha > 0.0f)
            {
                fadeFinish = false;

                break;
            }
        }

        if(fadeFinish == true)
        {
            Recycle();
        }
    }

}
