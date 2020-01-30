using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : GameObjectPoolItem
{
    
    public float recycleTime;
    private float m_Timer;

    public string[] collisionDetect_IgnoreLayers;
    public string[] collisionDetect_IgnoreTags;
    public Transform detectPoint;
    public float detectExtendDistance = 2.0f;

    [System.Serializable]
    public class BulletImpactData
    {
        public GameObjectPoolItem bulletImpactPoolItem;
        public string bulletImpactPoolName;
        public int bulletImpactPreNum;
    }

    public BulletImpactData[] bulletImpactDataArray;
    public int damageTargetValue = 3;

    private Vector3 m_LastDetectPointPos;
    private Dictionary<int,GameObjectPool> m_ImpactPoolDictionary = new Dictionary<int,GameObjectPool>();


    public void Awake()
    {

        for (int i = 0; i < bulletImpactDataArray.Length;i++ )
        {
            BulletImpactData bulletImpactData = bulletImpactDataArray[i];

            string bulletImpactPoolName = bulletImpactData.bulletImpactPoolName;
            GameObjectPoolItem bulletImpactPoolItem = bulletImpactData.bulletImpactPoolItem;           
            int bulletImpactPreNum = bulletImpactData.bulletImpactPreNum;

            if (GameObjectPoolManager.HasGameObjectPool(bulletImpactPoolName) == false)
            {
                GameObjectPoolManager.CreateGameObjectPoolForGameObject(bulletImpactPoolName, bulletImpactPoolItem.gameObject, bulletImpactPreNum);
            }

            GameObjectPool bulletImpactPool = GameObjectPoolManager.GetGameObjectPool(bulletImpactPoolName);
            m_ImpactPoolDictionary.Add(bulletImpactPoolItem.gameObject.layer,bulletImpactPool);

        }


    }

    public void OnEnable()
    {
        m_Timer = Time.time;

        m_LastDetectPointPos = detectPoint.position - transform.forward * detectExtendDistance;

        this.GetComponent<Rigidbody>().velocity = Vector3.zero;

    }

    public void Update()
    {
        if (Time.time >= m_Timer + recycleTime)
        {
            Recycle();
        }
        else
        {
            PhysicsLineDetect();

            m_LastDetectPointPos = detectPoint.position - transform.forward * detectExtendDistance;
        }
    }

    private void PhysicsLineDetect()
    {
        RaycastHit hitInfo;

        Vector3 currentDetectPoint = detectPoint.position;


#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(m_LastDetectPointPos, currentDetectPoint,Color.red);
#endif

        if (Physics.Linecast(m_LastDetectPointPos, currentDetectPoint, out hitInfo) == true)
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

            for (int i = 0; i < bulletImpactDataArray.Length;i++ )
            {
                int detectLayer = bulletImpactDataArray[i].bulletImpactPoolItem.gameObject.layer;

                if (hitInfo.transform.gameObject.layer == detectLayer)
                {
                    m_ImpactPoolDictionary[detectLayer].SpawnGameObjectPoolItem(hitInfo.point + hitInfo.normal * 0.05f, Quaternion.LookRotation(hitInfo.normal));

                    break;
                }
            }

            GameObject hitRootGameObject = hitInfo.transform.root.gameObject;

            LifeController lifeController = hitRootGameObject.GetComponent<LifeController>();

            if (lifeController != null)
            {
                lifeController.TakeDamage(damageTargetValue);
            }

            Recycle();
        }

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

        for (int i = 0; i < bulletImpactDataArray.Length; i++)
        {
            int detectLayer = bulletImpactDataArray[i].bulletImpactPoolItem.gameObject.layer;

            if (collision.transform.root.gameObject.layer == detectLayer)
            {
                m_ImpactPoolDictionary[detectLayer].SpawnGameObjectPoolItem(contectPoint.point + contectPoint.normal * 0.05f, Quaternion.LookRotation(contectPoint.normal));

                break;
            }
        }

        GameObject hitRootGameObject = collision.transform.root.gameObject;

        LifeController lifeController = hitRootGameObject.GetComponent<LifeController>();

        if (lifeController != null)
        {
            lifeController.TakeDamage(damageTargetValue);
        }


        Recycle();
    }
}
