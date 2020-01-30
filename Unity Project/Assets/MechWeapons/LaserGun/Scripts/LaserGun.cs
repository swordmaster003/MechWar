using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;


[RequireComponent(typeof(AudioSource))]
public class LaserGun : BaseWeapon
{
    public float shootSpeed = 1.0f;

    public GameObjectPoolItem bulletPoolItem;
    public string bulletPoolName;
    public int bulletPoolPreNum;
    private GameObjectPool m_BulletPool;
    public float bulletLifeTime = 3.0f;
    public Transform shootPoint;
    public float shootForce = 8000;

    public AudioClip[] openFireSounds;

    private AudioSource m_AudioSource;

    public float fireInterval = 0.5f;

    private float m_Timer;

    private float m_FireInterval;

    private void Awake()
    {

        if (GameObjectPoolManager.HasGameObjectPool(bulletPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(bulletPoolName, bulletPoolItem.gameObject, bulletPoolPreNum);
        }

        m_BulletPool = GameObjectPoolManager.GetGameObjectPool(bulletPoolName);

        m_AudioSource = this.GetComponent<AudioSource>();

        if (m_AudioSource == null)
        {
            this.gameObject.AddComponent<AudioSource>();
            m_AudioSource = this.GetComponent<AudioSource>();
        }
    }

    public override void OpenFire()
    {
        if (Time.time - m_Timer >= m_FireInterval)
        {
            Shoot();

            m_Timer = Time.time;

            m_FireInterval = fireInterval;
        }    
    }

    public override void StopFire()
    {

    }


    public void Shoot()
    {
        isFiring = true;

        Vector3 bulletStartPosition = shootPoint.position;
        Quaternion bulletStartRotation= shootPoint.rotation;

        if (bulletPoolItem != null)
        {
            GameObject bulletClone = m_BulletPool.SpawnGameObjectPoolItem(bulletStartPosition, bulletStartRotation);
            bulletClone.GetComponent<Laser>().recycleTime = bulletLifeTime;

            Rigidbody bulletRigidBody = bulletClone.GetComponent<Rigidbody>();
            bulletRigidBody.AddForce(this.transform.forward * shootForce);
        }


        if (openFireSounds.Length > 0)
        {
            if (m_AudioSource != null)
            {
                int randomSoundIndex = Random.Range(0, openFireSounds.Length);
                AudioClip openFireSound = openFireSounds[randomSoundIndex];
                m_AudioSource.PlayOneShot(openFireSound);
            }
        }

        isFiring = false;

    }




}

