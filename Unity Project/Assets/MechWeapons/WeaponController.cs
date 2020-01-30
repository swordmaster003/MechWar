using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponPrefabData
    {
        public GameObject leftWeaponPrefab;
        public GameObject rightWeaponPrefab;
    }

    public WeaponPrefabData[] weaponDataArray;
    //public GameObject[] weaponPrefabs;

    //public Transform body;
    public Transform leftWeaponConnect;
    public Transform rightWeaponConnect;

    private int m_ActiveIndex;

    private MovementController m_MovementController;

    public class WeaponRunTimeData
    {
        public GameObject leftWeapon;
        public GameObject rightWeapon;

        public BaseWeapon leftWeaponScript;
        public BaseWeapon rightWeaponScript;

        public WeaponRunTimeData(GameObject leftWeapon, GameObject rightWeapon, BaseWeapon leftWeaponScript, BaseWeapon rightWeaponScript)
        {
            this.leftWeapon = leftWeapon;
            this.rightWeapon = rightWeapon;
            this.leftWeaponScript = leftWeaponScript;
            this.rightWeaponScript = rightWeaponScript;
        }
    }

    private List<WeaponRunTimeData> m_WeaponList = new List<WeaponRunTimeData>();

    [HideInInspector]
    public BaseWeapon currentLeftWeapon;
    [HideInInspector]
    public BaseWeapon currentRightWeapon;

    void Awake()
    {
        m_MovementController = this.GetComponent<MovementController>();

        for (int i = 0; i < weaponDataArray.Length;i++ )
        {
            WeaponPrefabData weaponData = weaponDataArray[i];

            GameObject leftWeaponClone = Instantiate(weaponData.leftWeaponPrefab, leftWeaponConnect.position, leftWeaponConnect.rotation);
            leftWeaponClone.transform.SetParent(leftWeaponConnect,true);
            BaseWeapon leftWeaponScript = leftWeaponClone.GetComponentInChildren<BaseWeapon>();

            GameObject rightWeaponClone = Instantiate(weaponData.rightWeaponPrefab, rightWeaponConnect.position, rightWeaponConnect.rotation);
            rightWeaponClone.transform.SetParent(rightWeaponConnect,true);
            BaseWeapon rightWeaponScript = rightWeaponClone.GetComponentInChildren<BaseWeapon>();

            WeaponRunTimeData weaponRunTimeData = new WeaponRunTimeData(leftWeaponClone, rightWeaponClone, leftWeaponScript, rightWeaponScript);

            m_WeaponList.Add(weaponRunTimeData);

            if (i != m_ActiveIndex)
            {
                leftWeaponClone.SetActive(false);
                rightWeaponClone.SetActive(false);
            }
            else
            {
                currentLeftWeapon = leftWeaponScript;
                currentRightWeapon = rightWeaponScript;
            }

        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            if(m_WeaponList[m_ActiveIndex].leftWeaponScript.isFiring == true)
            {
                return;
            }

            if (m_WeaponList[m_ActiveIndex].rightWeaponScript.isFiring == true)
            {
                return;
            }

            m_ActiveIndex++;
            m_ActiveIndex = m_ActiveIndex % m_WeaponList.Count;


            for (int i = 0; i < m_WeaponList.Count;i++ )
            {
                if (i == m_ActiveIndex)
                {
                    m_WeaponList[i].leftWeapon.SetActive(true);
                    m_WeaponList[i].rightWeapon.SetActive(true);

                    currentLeftWeapon = m_WeaponList[i].leftWeaponScript;
                    currentRightWeapon = m_WeaponList[i].rightWeaponScript;
                }
                else
                {
                    m_WeaponList[i].leftWeapon.SetActive(false);
                    m_WeaponList[i].rightWeapon.SetActive(false);
                }
            }
        }

        if (Input.GetKey(KeyCode.B) && m_MovementController.turnFinish == true)
        {
            m_WeaponList[m_ActiveIndex].leftWeaponScript.OpenFire();
            m_WeaponList[m_ActiveIndex].rightWeaponScript.OpenFire();
        }
        else
        {
            m_WeaponList[m_ActiveIndex].leftWeaponScript.StopFire();
            m_WeaponList[m_ActiveIndex].rightWeaponScript.StopFire();
        }
    }
}
