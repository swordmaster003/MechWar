using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleLogic : MonoBehaviour
{
    public float fadeSpeed = 1.0f;
    public float beginTintAlpha = 0.5f;
    public Color myColor;
    public float currentTintAlpha;
    private Renderer m_Renderer;

    void Awake()
    {
        m_Renderer = this.GetComponent<Renderer>();
    }

    // Use this for initialization
    void OnEnable()
    {
        float randomRotZ = Random.Range(0.0f, 360.0f);
        this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, randomRotZ);
        currentTintAlpha = beginTintAlpha;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTintAlpha > 0.0f)
        {
            currentTintAlpha = currentTintAlpha - Time.deltaTime * fadeSpeed;
        }

        if (currentTintAlpha < 0.0f)
        {
            currentTintAlpha = 0.0f;
        }

        m_Renderer.material.SetColor("_TintColor", new Color(myColor.r, myColor.g, myColor.b, currentTintAlpha));
    }
}
