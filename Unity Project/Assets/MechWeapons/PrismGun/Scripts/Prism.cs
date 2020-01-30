using UnityEngine;
using System.Collections;

public class Prism : GameObjectPoolItem
{
    [HideInInspector]
    public LineRenderer lineRenderer;
    [HideInInspector]
    public float prismWidth;

    [HideInInspector]
    public Color prismColor;
    [HideInInspector]
    public float prismAlphaFadeToFullTime;
    [HideInInspector]
    [Range(0.0f, 1.0f)]
    public float prismAfterAlphaFullLifeTime;
    [HideInInspector]
    public float prismDeltaLifeTime = 0.0f;

    [HideInInspector]
    public bool isVanish;
    [HideInInspector]
    public Vector3 attackPos;
    [HideInInspector]
    public GameObject attackTarget;

    public GameObjectPoolItem prismImpact;
    public int prismImpactPreNum;
    public string prismImpactPoolName;
    private GameObjectPool m_PrismImpactPool;
    private GameObject m_CurrentPrismImpactClone;
    private bool m_DamageLock;
    [HideInInspector]
    public int damageValue;

    void Awake()
    {
        if (GameObjectPoolManager.HasGameObjectPool(prismImpactPoolName) == false)
        {
            GameObjectPoolManager.CreateGameObjectPoolForGameObject(prismImpactPoolName, prismImpact.gameObject, prismImpactPreNum);
        }

        m_PrismImpactPool = GameObjectPoolManager.GetGameObjectPool(prismImpactPoolName);
    }

    void OnEnable()
    {
        lineRenderer = null;
        prismWidth = 0.0f;//闪电的宽度
        prismColor = Color.white;//闪电的颜色
        prismAlphaFadeToFullTime = 0.0f;
        prismAfterAlphaFullLifeTime = 0.0f;
        prismDeltaLifeTime = 0.0f;
        attackTarget = null;//要攻击的目标
        attackPos = Vector3.one * float.MaxValue;
        isVanish = false;
        m_CurrentPrismImpactClone = null;

        m_DamageLock = false;
        damageValue = 0;
    }

    void Update()
    {
        lineRenderer.enabled = true;
        lineRenderer.startWidth = prismWidth;
        lineRenderer.endWidth = prismWidth;


        lineRenderer.SetPosition(0, this.transform.position);
        lineRenderer.SetPosition(1, attackPos);

        if (m_CurrentPrismImpactClone != null)
        {
            if (attackTarget != null)
            {
                m_CurrentPrismImpactClone.SetActive(true);
                m_CurrentPrismImpactClone.transform.position = attackPos;
            }
            else
            {
                m_CurrentPrismImpactClone.SetActive(false);
            }
        }
        else
        {
            if (attackTarget != null)
            {
                m_CurrentPrismImpactClone = m_PrismImpactPool.SpawnGameObjectPoolItem(attackPos, Quaternion.identity);
            }
        }


        if (prismAfterAlphaFullLifeTime < 0.0f)
        {
            prismDeltaLifeTime = prismDeltaLifeTime - Time.deltaTime;

            if (prismDeltaLifeTime < 0.0f)
            {
                isVanish = true;

                if (m_CurrentPrismImpactClone != null)
                {
                    m_CurrentPrismImpactClone.GetComponent<GameObjectPoolItem>().Recycle();
                }
              
                Recycle();
            }
            else
            {
                DamageTarget();
                float alphaValue = prismDeltaLifeTime / prismAlphaFadeToFullTime;
                lineRenderer.material.SetColor("_TintColor", new Color(prismColor.r, prismColor.g, prismColor.b, alphaValue));
            }
        }
        else
        {
            if (prismDeltaLifeTime < prismAlphaFadeToFullTime)
            {
                prismDeltaLifeTime = prismDeltaLifeTime + 3.0f * Time.deltaTime;
                float alphaValue = prismDeltaLifeTime / prismAlphaFadeToFullTime;
                lineRenderer.material.SetColor("_TintColor", new Color(prismColor.r, prismColor.g, prismColor.b, alphaValue));
            }
            else
            {
                prismAfterAlphaFullLifeTime = prismAfterAlphaFullLifeTime - Time.deltaTime;
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
