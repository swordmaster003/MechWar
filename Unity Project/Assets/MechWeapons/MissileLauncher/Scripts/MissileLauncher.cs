using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class MissileLauncher :BaseWeapon
{
    public Transform[] missileMountPoint;

    public GameObjectPoolItem misslePrefab;//Missile预制物体

    public string missilePoolName = "MultiMissilesGameObjectPool";

    public int misslePoolMultiTime = 3;//对象池大小为弹舱实际需要导弹的倍数

    private GameObjectPool missileGameObjectPool;//主导弹舱对象池

    public float launchMissileForce = 1500.0f;//发射导弹时的推力

    public float launchEachMissileInterval = 0.2f;

    private float m_LaunchEachMissileTimer;

    private bool m_FireEachMissileLockFlag;

    public float missileReloadTime = 1.5f;

    private float m_MissileReloadTimer;

    public bool rigidbodyProjectile;

    private Rigidbody m_PlayerRigidBody;//刚体

    private GameObject[] m_MissileRuntimeDatas;

    private bool m_MissileReloadIsDone;

    private int m_CurrentMissileIndex;

    void Awake()
    {
        int missileNum = missileMountPoint.Length;

        if (GameObjectPoolManager.HasGameObjectPool(missilePoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(missilePoolName, misslePrefab.gameObject, missileNum * misslePoolMultiTime);
        }

        missileGameObjectPool = GameObjectPoolManager.GetGameObjectPool(missilePoolName);

        m_MissileRuntimeDatas = new GameObject[missileNum];

        ReloadMissile();

    }


    void FixedUpdate()
    {
        if (m_MissileReloadIsDone == false && isFiring == false)
        {
            if (Time.time - m_MissileReloadTimer >= missileReloadTime)
            {
                ReloadMissile();
            }
        }

        if (isFiring == true)
        {
            if (m_FireEachMissileLockFlag == false)
            {
                LaunchMissile(m_CurrentMissileIndex);

                m_FireEachMissileLockFlag = true;

                m_LaunchEachMissileTimer = Time.time;

                m_CurrentMissileIndex++;

                if (m_CurrentMissileIndex >= m_MissileRuntimeDatas.Length)
                {
                    m_CurrentMissileIndex = 0;

                    isFiring = false;

                    m_MissileReloadTimer = Time.time;
                }
            }
            else
            {
                if (Time.time - m_LaunchEachMissileTimer >= launchEachMissileInterval)
                {
                    m_FireEachMissileLockFlag = false;
                }
            }
            
        }
    }

    private void LaunchMissile(int missileIndex)
    {
        GameObject missileClone = m_MissileRuntimeDatas[missileIndex];

        Missile missile = missileClone.GetComponent<Missile>();

        Vector3 basicVelocity = Vector3.zero;

        if (rigidbodyProjectile == true)
        {
            m_PlayerRigidBody = this.transform.root.gameObject.GetComponent<Rigidbody>();

            if (m_PlayerRigidBody != null)
            {
                basicVelocity = m_PlayerRigidBody.velocity;
            }
        }

        Vector3 addForce = missileClone.transform.forward * launchMissileForce;

        missile.Launch(basicVelocity, addForce);

        m_MissileRuntimeDatas[missileIndex] = null;
    }

    public override void OpenFire()
    {
        if (m_MissileReloadIsDone == true)
        {
            m_MissileReloadIsDone = false;

            isFiring = true;
        }
    }
    public override void StopFire()
    {

    }

    public void ReloadMissile()
    {
        for (int i = 0; i < m_MissileRuntimeDatas.Length; i++)
        {
            Transform missileMountPointTransform = missileMountPoint[i];
          
            GameObject missileClone = missileGameObjectPool.SpawnGameObjectPoolItem(missileMountPointTransform.position, missileMountPointTransform.rotation);

            missileClone.transform.SetParent(missileMountPointTransform);

            m_MissileRuntimeDatas[i] = missileClone;
        }

        m_MissileReloadIsDone = true;
    }


}