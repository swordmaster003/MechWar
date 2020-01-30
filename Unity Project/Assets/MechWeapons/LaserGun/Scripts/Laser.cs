using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : GameObjectPoolItem
{

    public float recycleTime;
    private float m_Timer;

    public string[] collisionDetect_IgnoreLayers;
    public string[] collisionDetect_IgnoreTags;
    public Transform detectPoint;
    public float detectExtendDistance = 2.0f;

    public GameObjectPoolItem laserImpactPoolItem;
    public string laserImpactPoolName;
    public int laserImpactPreNum;
    private GameObjectPool m_LaserImpactPool;

    public int damageTargetValue = 3;

    private Vector3 m_LastDetectPointPos;


    public void Awake()
    {
        if (GameObjectPoolManager.HasGameObjectPool(laserImpactPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(laserImpactPoolName, laserImpactPoolItem.gameObject, laserImpactPreNum);
        }

        m_LaserImpactPool = GameObjectPoolManager.GetGameObjectPool(laserImpactPoolName);
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
        Debug.DrawLine(m_LastDetectPointPos, currentDetectPoint, Color.red);
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

            m_LaserImpactPool.SpawnGameObjectPoolItem(hitInfo.point, Quaternion.LookRotation(hitInfo.normal));

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

        m_LaserImpactPool.SpawnGameObjectPoolItem(contectPoint.point, Quaternion.LookRotation(contectPoint.normal));

        GameObject hitRootGameObject = collision.transform.root.gameObject;

        LifeController lifeController = hitRootGameObject.GetComponent<LifeController>();

        if (lifeController != null)
        {
            lifeController.TakeDamage(damageTargetValue);
        }


        Recycle();
    }
}
