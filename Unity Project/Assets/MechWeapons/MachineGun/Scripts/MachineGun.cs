using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;


[RequireComponent(typeof(AudioSource))]
public class MachineGun : BaseWeapon
{
    public float shootSpeed = 1.0f;

    public GameObjectPoolItem bulletPoolItem;
    public string bulletPoolName;
    public int bulletPoolPreNum;
    private GameObjectPool m_BulletPool;
    public float bulletLifeTime = 2.0f;

    public Transform shootPoint;
    public float shootForce = 8000;
    public float spread = 1;

    public GameObjectPoolItem shellPoolItem;
    public string shellPoolName;
    public int shellPoolPreNum;
    private GameObjectPool m_ShellPool;

    public float shellLifeTime = 4;
    public Transform shellStartPoint;
    public int shellOutForce = 300;


    public GameObjectPoolItem muzzlePoolItem;
    public string muzzlePoolName;
    public int muzzlePoolPreNum;
    private GameObjectPool m_MuzzlePool;

    public float muzzleLifeTime = 2;

    public AudioClip[] openFireSounds;

    public bool RigidbodyProjectile;

    private Animator m_Animator;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        if (GameObjectPoolManager.HasGameObjectPool(bulletPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(bulletPoolName, bulletPoolItem.gameObject, bulletPoolPreNum);
        }

        m_BulletPool = GameObjectPoolManager.GetGameObjectPool(bulletPoolName);

        if (GameObjectPoolManager.HasGameObjectPool(shellPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(shellPoolName, shellPoolItem.gameObject, shellPoolPreNum);
        }

        m_ShellPool = GameObjectPoolManager.GetGameObjectPool(shellPoolName);

        if (GameObjectPoolManager.HasGameObjectPool(muzzlePoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(muzzlePoolName, muzzlePoolItem.gameObject, muzzlePoolPreNum);
        }

        m_MuzzlePool = GameObjectPoolManager.GetGameObjectPool(muzzlePoolName);

        m_AudioSource = this.GetComponent<AudioSource>();
        if (m_AudioSource == null)
        {
            this.gameObject.AddComponent<AudioSource>();
            m_AudioSource = this.GetComponent<AudioSource>();
        }

        m_Animator = this.GetComponent<Animator>();

        m_Animator.speed = shootSpeed;
    }

    public override void OpenFire()
    {
        m_Animator.SetBool("OpenFire", true);
    }

    public override void StopFire()
    {
        m_Animator.SetBool("OpenFire", false);
    }


    public void Shoot()
    {
        isFiring = true;

        Vector3 bulletStartPosition;
        Quaternion bulletStartRotation;

        bulletStartPosition = shootPoint.position;
        bulletStartRotation = shootPoint.rotation;

        float spreadRange = spread / 10.0f;
        float randomSpreadX = Random.Range(-spreadRange, spreadRange);
        float randomSpreadY = Random.Range(-spreadRange, spreadRange);

        Vector3 direction = this.shootPoint.forward + this.shootPoint.up * randomSpreadY + this.shootPoint.right * randomSpreadX;
        bulletStartRotation = Quaternion.LookRotation(direction);

        GameObject bulletClone = m_BulletPool.SpawnGameObjectPoolItem(bulletStartPosition, bulletStartRotation);
        bulletClone.GetComponent<Bullet>().recycleTime = bulletLifeTime;
        Rigidbody bulletRigidBody = bulletClone.GetComponent<Rigidbody>();

        if (RigidbodyProjectile == true)
        {
            Rigidbody ownerRigidBody = this.transform.root.gameObject.GetComponent<Rigidbody>();

            if (ownerRigidBody != null)
            {
                bulletRigidBody.velocity = ownerRigidBody.velocity;
            }
        }

        bulletRigidBody.AddForce(direction * shootForce);


        GameObject muzzleClone = m_MuzzlePool.SpawnGameObjectPoolItem(bulletStartPosition, bulletStartRotation);
        muzzleClone.GetComponent<Muzzle>().recycleTime = muzzleLifeTime;


        Transform shellStartTransform;

        if (shellStartPoint != null)
        {
            shellStartTransform = shellStartPoint;
        }
        else
        {
            shellStartTransform = this.transform;
        }

        GameObject shellClone = m_ShellPool.SpawnGameObjectPoolItem(shellStartTransform.position, shellStartTransform.rotation);
        shellClone.GetComponent<BulletShell>().recycleTime = shellLifeTime;

        Rigidbody shellRigidbody = shellClone.GetComponent<Rigidbody>();

        if (shellRigidbody != null)
        {
            shellRigidbody.AddForce(shellStartTransform.right * shellOutForce);
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

