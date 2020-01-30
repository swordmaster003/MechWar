using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Missile : GameObjectPoolItem
{
    [System.Serializable]
    public class EffectInfo
    {
        public string effectPoolName;

        public GameObjectPoolItem effectItem;

        public int effectPoolNum;
        public GameObjectPool effectPool { set; get; }
    }

    public string[] collisionDetect_IgnoreLayers;
    public string[] collisionDetect_IgnoreTags;
    public Transform detectPoint;
    public float detectExtendDistance = 2.0f;

    public float sightDistance = 10.0f;
    public float sightViewAngle = 60.0f;
    public float moveDamping = 10.0f;
    public float trackDelaytime = 1.0f;
    public float autoDestroyTime = 10.0f;
    public float hitTargetDamage = 10.0f;

    public Transform trailMountPoint;

    public EffectInfo trailEffectInfo;

    public EffectInfo spaceExplosionEffectInfo;

    public EffectInfo[] hitExplosionDataArray;

    private GameObject m_AttackTarget;

    private bool m_TargetBeLocked;

    private float m_Timer;

    private Rigidbody m_Rigidbody;

    private AudioSource m_AudioSource;

    private MissileTrail m_TrailHandler;

    private Vector3 m_LastDetectPointPos;

    private Dictionary<int, GameObjectPool> m_MissileExplosionPoolDictionary = new Dictionary<int, GameObjectPool>();

    public int damageTargetValue = 20;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        m_AudioSource = GetComponent<AudioSource>();

        if (GameObjectPoolManager.HasGameObjectPool( trailEffectInfo.effectPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(trailEffectInfo.effectPoolName, trailEffectInfo.effectItem.gameObject, trailEffectInfo.effectPoolNum);
        }

        trailEffectInfo.effectPool = GameObjectPoolManager.GetGameObjectPool(trailEffectInfo.effectPoolName);
    

        for (int i = 0; i < hitExplosionDataArray.Length; i++)
        {
            EffectInfo missileExplosionData = hitExplosionDataArray[i];

            string missileExplosionPoolName = missileExplosionData.effectPoolName;
            GameObjectPoolItem missileExplosionPoolItem = missileExplosionData.effectItem;
            int missileExplosionPreNum = missileExplosionData.effectPoolNum;

            if (GameObjectPoolManager.HasGameObjectPool(missileExplosionPoolName) == false)
            {
                GameObjectPoolManager.CreateGameObjectPoolForGameObject(missileExplosionPoolName, missileExplosionPoolItem.gameObject, missileExplosionPreNum);
            }

            GameObjectPool missileExplosionPool = GameObjectPoolManager.GetGameObjectPool(missileExplosionPoolName);
            hitExplosionDataArray[i].effectPool = missileExplosionPool;
            m_MissileExplosionPoolDictionary.Add(missileExplosionPoolItem.gameObject.layer, missileExplosionPool);

        }

        if (GameObjectPoolManager.HasGameObjectPool(spaceExplosionEffectInfo.effectPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(spaceExplosionEffectInfo.effectPoolName, spaceExplosionEffectInfo.effectItem.gameObject, spaceExplosionEffectInfo.effectPoolNum);
        }

        spaceExplosionEffectInfo.effectPool = GameObjectPoolManager.GetGameObjectPool(spaceExplosionEffectInfo.effectPoolName);


    }

    void OnEnable()
    {
        m_Rigidbody.useGravity = false;

        m_Rigidbody.isKinematic = true;

        m_Rigidbody.velocity = Vector3.zero;

        m_AttackTarget = null;

        m_TargetBeLocked = false;

        m_AudioSource.enabled = false;

        m_LastDetectPointPos = detectPoint.position - detectPoint.forward * detectExtendDistance;

        m_TrailHandler = null;

        m_Timer = 0.0f;

    }

    void FixedUpdate()
    {
        if (this.transform.parent != null)
        {
            return;
        }

        if ((m_AttackTarget != null) && (m_TargetBeLocked == true) && (Time.time > m_Timer + trackDelaytime))
        {
            if (Vector3.Angle(m_AttackTarget.transform.position - transform.position, transform.forward) <= sightViewAngle)
            {
                m_Rigidbody.rotation = Quaternion.Slerp(m_Rigidbody.rotation, Quaternion.LookRotation(m_AttackTarget.transform.position - transform.position), Time.fixedDeltaTime * moveDamping);

                m_Rigidbody.velocity = m_Rigidbody.transform.forward * m_Rigidbody.velocity.magnitude;
            }
            else
            {
                m_AttackTarget = null;
            }
        }
        else
        {
            //m_Rigidbody.rotation = Quaternion.Slerp(m_Rigidbody.rotation, Quaternion.LookRotation(m_Rigidbody.velocity), Time.fixedDeltaTime * moveDamping);
        }

    }

    public void Update()
    {
        if(this.transform.parent != null)
        {
            return;
        }

        if (m_TargetBeLocked == false)
        {
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");

            for (int i = 0; i < enemys.Length; i++)
            {
                GameObject enemy = enemys[i];

                if (Vector3.Distance(enemy.transform.position, this.gameObject.transform.position) <= sightDistance
                   && Vector3.Angle(enemy.transform.position - transform.position, transform.forward) <= sightViewAngle)
                {
                    m_AttackTarget = enemy;

                    m_TargetBeLocked = true;

                    break;
                }
            }
        }

        if (Time.time > m_Timer + autoDestroyTime)
        {
            spaceExplosionEffectInfo.effectPool.SpawnGameObjectPoolItem(this.transform.position, Quaternion.identity);

            m_TrailHandler.Detach();

            m_TrailHandler = null;

            Recycle();
        }
        else
        {
            PhysicsLineDetect();

            m_LastDetectPointPos = detectPoint.position - detectPoint.forward * detectExtendDistance;
        }
    }


    public void Launch(Vector3 basicVelocity, Vector3 addForce)
    {
        this.transform.SetParent(null);

        m_Rigidbody.useGravity = false;

        m_Rigidbody.isKinematic = false;

        m_Rigidbody.velocity = basicVelocity;

        m_Rigidbody.AddForce(addForce);

        //生成导弹拖尾
        GameObject trailGameobject = trailEffectInfo.effectPool.SpawnGameObjectPoolItem(trailMountPoint.position, trailMountPoint.rotation);

        m_TrailHandler = trailGameobject.GetComponent<MissileTrail>();

        m_TrailHandler.Attach(trailMountPoint);

        m_AudioSource.enabled = true;

        m_Timer = Time.time;

        m_LastDetectPointPos = detectPoint.position - detectPoint.forward * detectExtendDistance;
    }

    void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collisionDetect_IgnoreLayers.Length; i++)
        {
            string layerName = collisionDetect_IgnoreLayers[i];

            int layer = LayerMask.NameToLayer(layerName);

            if (collision.transform.gameObject.layer == layer)
            {
                return;
            }
        }

        for (int i = 0; i < collisionDetect_IgnoreTags.Length; i++)
        {
            string tagName = collisionDetect_IgnoreTags[i];

            if (collision.transform.gameObject.CompareTag(tagName))
            {
                return;
            }
        }


        ContactPoint contectPoint = collision.contacts[0];

        for (int i = 0; i < hitExplosionDataArray.Length; i++)
        {
            int detectLayer = hitExplosionDataArray[i].effectItem.gameObject.layer;

            if (collision.transform.root.gameObject.layer == detectLayer)
            {
                m_MissileExplosionPoolDictionary[detectLayer].SpawnGameObjectPoolItem(contectPoint.point + contectPoint.normal * 0.05f, Quaternion.LookRotation(contectPoint.normal));

                break;
            }
        }

        GameObject hitRootGameObject = collision.transform.root.gameObject;

        LifeController lifeController = hitRootGameObject.GetComponent<LifeController>();

        if (lifeController != null)
        {
            lifeController.TakeDamage(damageTargetValue);
        }

        m_TrailHandler.Detach();

        m_TrailHandler = null;

        Recycle();
    }

    private void PhysicsLineDetect()
    {
        RaycastHit hitInfo;

        Vector3 currentDetectPoint = detectPoint.position;

#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(m_LastDetectPointPos, currentDetectPoint, Color.red);
#endif

        if (Physics.Linecast(m_LastDetectPointPos, currentDetectPoint, out hitInfo, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) == true)
        {
            for (int i = 0; i < collisionDetect_IgnoreLayers.Length; i++)
            {
                string layerName = collisionDetect_IgnoreLayers[i];

                int layer = LayerMask.NameToLayer(layerName);

                if (hitInfo.transform.gameObject.layer == layer)
                {
                    return;
                }
            }

            for (int i = 0; i < collisionDetect_IgnoreTags.Length; i++)
            {
                string tagName = collisionDetect_IgnoreTags[i];

                if (hitInfo.transform.gameObject.CompareTag(tagName))
                {
                    return;
                }
            }


            bool hasLayer = false;

            for (int i = 0; i < hitExplosionDataArray.Length; i++)
            {
                int detectLayer = hitExplosionDataArray[i].effectItem.gameObject.layer;

                if (hitInfo.transform.gameObject.layer == detectLayer)
                {
                    m_MissileExplosionPoolDictionary[detectLayer].SpawnGameObjectPoolItem(hitInfo.point + hitInfo.normal * 0.05f, Quaternion.LookRotation(hitInfo.normal));

                    hasLayer = true;

                    break;
                }
            }

            if(hasLayer == false)
            {
                spaceExplosionEffectInfo.effectPool.SpawnGameObjectPoolItem(hitInfo.point + hitInfo.normal * 0.05f, Quaternion.LookRotation(hitInfo.normal));
            }

            GameObject hitRootGameObject = hitInfo.transform.root.gameObject;

            LifeController lifeController = hitRootGameObject.GetComponent<LifeController>();

            if (lifeController != null)
            {
                lifeController.TakeDamage(damageTargetValue);
            }

            m_TrailHandler.Detach();

            m_TrailHandler = null;

            Recycle();
        }

    }

}
