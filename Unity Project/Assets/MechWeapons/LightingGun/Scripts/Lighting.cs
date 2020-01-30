using UnityEngine;
using System.Collections;

public class Lighting : GameObjectPoolItem
{

    [HideInInspector]
    public LineRenderer lineRenderer;

    [HideInInspector]
    public float lightingUpdateTimeCount;//闪电频率时间计数器
    [HideInInspector]
    public float lightningUpdateFrequency;//闪电的频率，时间秒来计算，比如多少秒变换一次形状
    [HideInInspector]
    public float lightningWidth;//闪电的宽度
    [HideInInspector]
    public int lightingSegmentNum;//闪电上分段的数量
    [HideInInspector]
    public float lightningCurveScale;//闪电的弧度大小
    [HideInInspector]
    public float lightningShakeScale;//闪电抖动的大小

    [HideInInspector]
    public float lightingTextureOffsetSpeed;//闪电材质贴图偏移的速度
    [HideInInspector]
    public float lightingTextureOffsetX = 0.0f;
    [HideInInspector]

    public Color lightingColor;//闪电的颜色

    [HideInInspector]
    public float lightingAlphaFadeToFullTime;
    [HideInInspector]
    [Range(0.0f, 1.0f)]
    public float lightingAfterAlphaFullLifeTime;
    [HideInInspector]
    public float lightingDeltaLifeTime = 0.0f;

    [HideInInspector]
    public bool isVanish;
    [HideInInspector]
    public Vector3 attackPos;
    [HideInInspector]
    public GameObject attackTarget;

    public GameObjectPoolItem lightingImpact;
    public int lightingImpactPreNum;
    public string lightingImpactPoolName;
    private GameObjectPool m_LightingImpactPool;
    private GameObject m_CurrentLightingImpactClone;
    private bool m_DamageLock;
    public int damageValue;

    void Awake()
    {
        if (GameObjectPoolManager.HasGameObjectPool(lightingImpactPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(lightingImpactPoolName, lightingImpact.gameObject, lightingImpactPreNum);
        }

        m_LightingImpactPool = GameObjectPoolManager.GetGameObjectPool(lightingImpactPoolName);
    }

    void OnEnable()
    {
        lineRenderer = null;
        lightingUpdateTimeCount = 0.0f;//闪电频率时间计数器
        lightningUpdateFrequency = 0.0f;//闪电的频率，时间秒来计算，比如多少秒变换一次形状
        lightningWidth = 0.0f;//闪电的宽度
        lightingSegmentNum = 0;//闪电上分段的数量
        lightningCurveScale = 0.0f;//闪电的弧度大小
        lightningShakeScale = 0.0f;//闪电抖动的大小
        lightingTextureOffsetSpeed = 0.0f;//闪电材质贴图偏移的速度
        lightingTextureOffsetX = 0.0f;
        lightingColor = Color.white;//闪电的颜色
        lightingAlphaFadeToFullTime = 0.0f;
        lightingAfterAlphaFullLifeTime = 0.0f;
        lightingDeltaLifeTime = 0.0f;
        attackTarget = null;//要攻击的目标
        attackPos = Vector3.one * float.MaxValue;
        isVanish = false;
        m_CurrentLightingImpactClone = null;

        m_DamageLock = false;
        damageValue = 0;
    }

    void Update()
    {
        lightingUpdateTimeCount = lightingUpdateTimeCount + Time.deltaTime;

        if (lightingUpdateTimeCount >= (1 / lightningUpdateFrequency))
        {
            if (lineRenderer.enabled == false)
            {
                lineRenderer.enabled = true;
            }

            lightingUpdateTimeCount = 0.0f;

            //分段长度向量 = 攻击目标到枪的距离/分段的数量
            Vector3 sectionVector = (attackPos - this.transform.position) / lightingSegmentNum;

            //设置闪电链开始宽度和结束宽度
            lineRenderer.startWidth = lightningWidth;
            lineRenderer.endWidth = lightningWidth;

            //设置闪电链第一个点的位置
            lineRenderer.SetPosition(0, this.transform.position);
            for (int i = 1; i < (lightingSegmentNum - 1); i++)
            {
                Vector3 currentVertexPos = this.transform.position + (sectionVector * i);

                float totalAngle = Mathf.PI;
                float currentVertexAnglePrecent = (float)i / (float)lightingSegmentNum;
                float currentAngle = totalAngle * currentVertexAnglePrecent;
                float shift = lightningCurveScale * Mathf.Sin(currentAngle);

                currentVertexPos = currentVertexPos + Vector3.left * Random.Range(-lightningShakeScale, lightningShakeScale);
                currentVertexPos = currentVertexPos + Vector3.up * Random.Range(-lightningShakeScale, lightningShakeScale);
                currentVertexPos = currentVertexPos + Vector3.forward * Random.Range(-lightningShakeScale, lightningShakeScale);
                currentVertexPos = currentVertexPos + Vector3.up * shift;

                lineRenderer.SetPosition(i, currentVertexPos);
            }
            //设置闪电链最后一个点的位置
            lineRenderer.SetPosition(lightingSegmentNum - 1, attackPos);
        }

        //材质上闪电贴图Offset的X分量随时间的偏移
        lightingTextureOffsetX = lightingTextureOffsetX - Time.deltaTime * lightingTextureOffsetSpeed;
        lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(lightingTextureOffsetX, 0));


        if (attackTarget != null && m_CurrentLightingImpactClone == null)
        {
            m_CurrentLightingImpactClone = m_LightingImpactPool.SpawnGameObjectPoolItem(attackPos, Quaternion.identity);
        }


        if (lightingAfterAlphaFullLifeTime < 0.0f)
        {
            lightingDeltaLifeTime = lightingDeltaLifeTime - Time.deltaTime;

            if (lightingDeltaLifeTime < 0.0f)
            {
                isVanish = true;
                m_CurrentLightingImpactClone.GetComponent<GameObjectPoolItem>().Recycle();
                Recycle();
            }
            else
            {
                DamageTarget();
                float alphaValue = lightingDeltaLifeTime / lightingAlphaFadeToFullTime;
                lineRenderer.material.SetColor("_TintColor", new Color(lightingColor.r, lightingColor.g, lightingColor.b, alphaValue));
            }
        }
        else
        {
            if (lightingDeltaLifeTime < lightingAlphaFadeToFullTime)
            {
                lightingDeltaLifeTime = lightingDeltaLifeTime + Time.deltaTime;
                float alphaValue = lightingDeltaLifeTime / lightingAlphaFadeToFullTime;
                lineRenderer.material.SetColor("_TintColor", new Color(lightingColor.r, lightingColor.g, lightingColor.b, alphaValue));
            }
            else
            {
                lightingAfterAlphaFullLifeTime = lightingAfterAlphaFullLifeTime - Time.deltaTime;
            }
        }
    }

    private void DamageTarget()
    {
        if (m_DamageLock == false)
        {
            if (attackTarget != null)
            {
                LifeController lifeController = attackTarget.GetComponent<LifeController>();

                if (lifeController != null)
                {
                    lifeController.TakeDamage(damageValue);
                }
            }

            m_DamageLock = true;
        }
    }

}
