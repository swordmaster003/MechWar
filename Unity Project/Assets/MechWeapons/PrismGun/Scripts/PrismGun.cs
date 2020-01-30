using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PrismGun : BaseWeapon
{
    public GameObjectPoolItem prismPoolItem;
    public string prismPoolName;
    public int prismPoolPreNum;
    private GameObjectPool m_PrismPool;

    public float AfterAlphaFullLifeTime = 0.7f;
    public float AlphaFadeToFullTime = 0.1f;

    public float prismWidth;
    public Color prismColor = Color.white;

    public Transform firePoint;
    public float maxFireDistance = 7.0f;

    public float fireInterval = 0.5f;
    private float m_Timer;
    private float m_FireInterval;

    public class PrismRuntimeData
    {
        public GameObject prismGameObject;
        public Prism prism;
        public GameObject currentAimTarget;
        public Vector3 currentAimPos;
    }

    private PrismRuntimeData prismRuntimeData = new PrismRuntimeData();

    public AudioClip[] openFireSounds;
    private AudioSource m_AudioSource;

    public int giveTargetDamage = 10;

    public void Awake()
    {
        if (GameObjectPoolManager.HasGameObjectPool(prismPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(prismPoolName, prismPoolItem.gameObject, prismPoolPreNum);
        }

        m_PrismPool = GameObjectPoolManager.GetGameObjectPool(prismPoolName);

        m_AudioSource = this.GetComponent<AudioSource>();

        if (m_AudioSource == null)
        {
            this.gameObject.AddComponent<AudioSource>();
            m_AudioSource = this.GetComponent<AudioSource>();
        }

    }

    public void Update()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(firePoint.position, firePoint.forward, out hitInfo, maxFireDistance) == true)
        {
            prismRuntimeData.currentAimPos = hitInfo.point;
            prismRuntimeData.currentAimTarget = hitInfo.transform.root.gameObject;
        }
        else
        {
            prismRuntimeData.currentAimPos = firePoint.position + firePoint.forward * maxFireDistance;
            prismRuntimeData.currentAimTarget = null;
        }

        if (prismRuntimeData.prism != null)
        {
            if (isFiring == true && prismRuntimeData.prism.isVanish == false)
            {
                prismRuntimeData.prismGameObject.transform.position = firePoint.position;
                prismRuntimeData.prismGameObject.transform.rotation = firePoint.rotation;
                prismRuntimeData.prism.attackPos = prismRuntimeData.currentAimPos;
                prismRuntimeData.prism.attackTarget = prismRuntimeData.currentAimTarget;
            }
            else
            {
                prismRuntimeData.currentAimTarget = null;
                prismRuntimeData.currentAimPos = Vector3.one * float.MaxValue;
                prismRuntimeData.prismGameObject = null;
                prismRuntimeData.prism = null;
                m_Timer = Time.time;
                isFiring = false;
            }
        }
        else
        {
            isFiring = false;
        }
    }

    private void Shoot()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(firePoint.position, firePoint.forward, out hitInfo, maxFireDistance) == true)
        {
            prismRuntimeData.currentAimPos = hitInfo.point;
            prismRuntimeData.currentAimTarget = hitInfo.transform.root.gameObject;
        }
        else
        {
            prismRuntimeData.currentAimPos = firePoint.position + firePoint.forward * maxFireDistance;
            prismRuntimeData.currentAimTarget = null;
        }


        GameObject prismClone = m_PrismPool.SpawnGameObjectPoolItem(firePoint.position, firePoint.rotation);

        LineRenderer lineRenderer = prismClone.GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.material.SetColor("_TintColor", prismColor);
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = prismWidth;
        lineRenderer.endWidth = prismWidth;

        Prism prism = prismClone.GetComponent<Prism>();
        prism.lineRenderer = lineRenderer;
        prism.prismAfterAlphaFullLifeTime = AfterAlphaFullLifeTime;
        prism.prismWidth = prismWidth;
        prism.prismColor = prismColor;
        prism.prismAlphaFadeToFullTime = AlphaFadeToFullTime;

        prism.attackTarget = prismRuntimeData.currentAimTarget;
        prism.attackPos = prismRuntimeData.currentAimPos;
        prism.damageValue = giveTargetDamage;

        prismRuntimeData.prismGameObject = prismClone;
        prismRuntimeData.prism = prism;

        if (openFireSounds.Length > 0)
        {
            if (m_AudioSource != null)
            {
                int randomSoundIndex = Random.Range(0, openFireSounds.Length);
                AudioClip openFireSound = openFireSounds[randomSoundIndex];
                m_AudioSource.PlayOneShot(openFireSound);
            }
        }

    }


    public override void OpenFire()
    {
        if (isFiring == false && Time.time - m_Timer >= m_FireInterval)
        {
            isFiring = true;

            Shoot();

            m_FireInterval = fireInterval;
        }
    }

    public override void StopFire()
    {

    }
}