using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LightingGun : BaseWeapon
{
    public GameObjectPoolItem lightningPoolItem;
    public string lightningPoolName;
    public int lightningPoolPreNum;
    private GameObjectPool m_LightningPool;

    public float lightingTextureOffsetSpeed = 2.0f;

    public float AfterAlphaFullLifeTime = 0.7f;
    public float AlphaFadeToFullTime = 0.1f;

    public float lightningWidth;
    public float segmentNumMulti = 1.0f;
    public float lightningCurveScale = 1.0f;
    public float lightningShakeScale = 1.0f;

    public float lightningUpdateFrequency = 60.0f;
    public Color lightingColor = Color.white;

    public Transform rayCastPoint;
    private LineRenderer rayCastLine;
    public Transform firePoint;
    public float maxFireDistance = 7.0f;

    public float fireInterval = 0.5f;
    private float m_Timer;
    private float m_FireInterval;

    public class LightingRuntimeData
    {
        public GameObject lightingGameObject;
        public Lighting lighting;
        public GameObject currentAimTarget;
        public Vector3 currentAimPos;
    }

    private LightingRuntimeData lightingRuntimeData = new LightingRuntimeData();

    public AudioClip[] openFireSounds;
    private AudioSource m_AudioSource;

    public int giveTargetDamage = 10;

    private Gradient m_RaycastGradient = new Gradient();
    private GradientColorKey[] m_RaycastGradientColorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) };
    private GradientAlphaKey[] m_RaycastGradientAlphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) };
    public void Awake()
    {
        if (GameObjectPoolManager.HasGameObjectPool(lightningPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(lightningPoolName, lightningPoolItem.gameObject, lightningPoolPreNum);
        }

        m_LightningPool = GameObjectPoolManager.GetGameObjectPool(lightningPoolName);

        m_AudioSource = this.GetComponent<AudioSource>();

        if (m_AudioSource == null)
        {
            this.gameObject.AddComponent<AudioSource>();
            m_AudioSource = this.GetComponent<AudioSource>();
        }

        rayCastLine = rayCastPoint.GetComponent<LineRenderer>();
    }

    public void Update()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(rayCastPoint.position, rayCastPoint.forward, out hitInfo, maxFireDistance) == true)
        {
            Vector3 rayCastStartPoint = rayCastPoint.position;
            Vector3 rayCastEndPoint = hitInfo.point;

            lightingRuntimeData.currentAimPos = rayCastEndPoint;
            lightingRuntimeData.currentAimTarget = hitInfo.transform.root.gameObject;


            rayCastLine.SetPosition(0, rayCastStartPoint);
            rayCastLine.SetPosition(1, rayCastEndPoint);

            float distance = Vector3.Distance(rayCastStartPoint,rayCastEndPoint);
            m_RaycastGradientAlphaKeys[1].alpha = 1.0f - distance / maxFireDistance;
            m_RaycastGradient.SetKeys(m_RaycastGradientColorKeys, m_RaycastGradientAlphaKeys);
            rayCastLine.colorGradient = m_RaycastGradient;

        }
        else
        {
            lightingRuntimeData.currentAimPos = Vector3.one * float.MaxValue;
            lightingRuntimeData.currentAimTarget = null;

            rayCastLine.SetPosition(0, rayCastPoint.position);
            rayCastLine.SetPosition(1, rayCastPoint.position + rayCastPoint.forward * maxFireDistance);

            m_RaycastGradientAlphaKeys[1].alpha = 0.0f;
            m_RaycastGradient.SetKeys(m_RaycastGradientColorKeys, m_RaycastGradientAlphaKeys);
            rayCastLine.colorGradient = m_RaycastGradient;
        }



        if (lightingRuntimeData.lighting != null)
        {
            if (isFiring == true && lightingRuntimeData.lighting.isVanish == false)
            {
                lightingRuntimeData.lightingGameObject.transform.position = firePoint.position;
                lightingRuntimeData.lightingGameObject.transform.rotation = firePoint.rotation;
            }
            else
            {
                lightingRuntimeData.currentAimTarget = null;
                lightingRuntimeData.currentAimPos = Vector3.one * float.MaxValue;
                lightingRuntimeData.lightingGameObject = null;
                lightingRuntimeData.lighting = null;
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
        float distance = Vector3.Distance(lightingRuntimeData.currentAimPos, firePoint.position);

        int lightingSegmentNum = (int)Mathf.Round(segmentNumMulti * distance);

        if (lightingSegmentNum < 4)
        {
            lightingSegmentNum = 4;
        }

        Vector2 texScale = new Vector2(distance * 0.1f * Mathf.Pow(lightingSegmentNum, 0.5f), 1.0f);

        GameObject lightningClone = m_LightningPool.SpawnGameObjectPoolItem(firePoint.position, firePoint.rotation);

        LineRenderer lineRenderer = lightningClone.GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.material.SetColor("_TintColor", lightingColor);
        lineRenderer.material.SetTextureScale("_MainTex", texScale);
        lineRenderer.positionCount = lightingSegmentNum;
        lineRenderer.startWidth = lightningWidth;
        lineRenderer.endWidth = lightningWidth;

        Lighting lighting = lightningClone.GetComponent<Lighting>();
        lighting.lightingSegmentNum = lightingSegmentNum;
        lighting.lineRenderer = lineRenderer;
        lighting.lightingAfterAlphaFullLifeTime = AfterAlphaFullLifeTime;
        lighting.lightningUpdateFrequency = lightningUpdateFrequency;
        lighting.lightingUpdateTimeCount = lightningUpdateFrequency;
        lighting.lightningWidth = lightningWidth;
        lighting.lightningCurveScale = lightningCurveScale;
        lighting.lightningShakeScale = lightningShakeScale;
        lighting.lightingTextureOffsetSpeed = lightingTextureOffsetSpeed;
        lighting.lightingColor = lightingColor;
        lighting.lightingAlphaFadeToFullTime = AlphaFadeToFullTime;

        lighting.attackTarget = lightingRuntimeData.currentAimTarget;
        lighting.attackPos = lightingRuntimeData.currentAimPos;
        lighting.damageValue = giveTargetDamage;

        lightingRuntimeData.lightingGameObject = lightningClone;
        lightingRuntimeData.lighting = lighting;

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
        if (lightingRuntimeData.currentAimTarget != null && isFiring == false && Time.time - m_Timer >= m_FireInterval)
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