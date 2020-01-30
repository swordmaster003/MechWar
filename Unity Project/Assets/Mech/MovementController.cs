using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float moveSpeedMultiply = 1.0f;
    public float jumpPower = 12.0f;

    public Transform[] detectGroundedPoints;

    [Range(0.0f,float.MaxValue)]
    public float groundCheckDistance = 0.1f;

    [Range(1.0f, 4.0f)]
    public float gravityMultiplier = 2.0f;

    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private float m_OriginalGroundCheckDistance;
    private float m_SpeedInputValue;
    private bool m_JumpFlag;

    //private LayerMask m_LayerMask;

    [HideInInspector]
    public bool turnFinish = true;

    public float turnSpeed = 10.0f;

    private bool m_NowRightDirectionFlag = true;

    private bool m_LastWasRightDirectionFlag = true;

    private WeaponController m_WeaponController;

    private bool m_IsGrounded;

    void Awake()
    {
        m_Animator = this.GetComponent<Animator>();
        m_Rigidbody = this.GetComponent<Rigidbody>();
        m_WeaponController = this.GetComponent<WeaponController>();
        m_Animator.SetFloat("moveSpeedMultiply", moveSpeedMultiply);
        m_OriginalGroundCheckDistance = groundCheckDistance;
        //m_LayerMask = ~(1 << 8);
    }

    public void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        int detectedCount = 0;

        for (int i = 0; i < detectGroundedPoints.Length;i++)
        {
            Transform detectGroundedPoint = detectGroundedPoints[i];

#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(detectGroundedPoint.position, detectGroundedPoint.position + (detectGroundedPoint.TransformDirection(Vector3.down) * groundCheckDistance));
#endif
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            if (Physics.Raycast(detectGroundedPoint.position, detectGroundedPoint.TransformDirection(Vector3.down), out hitInfo, groundCheckDistance))
            {
                detectedCount++;
            }

        }

        if (detectedCount > 0)
        {
            m_IsGrounded = true;
        }
        else
        {
            m_IsGrounded = false;
        }
        
    }

    public void GiveJumpForce()
    {
        m_Animator.applyRootMotion = false;
        m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, jumpPower, m_Rigidbody.velocity.z);       
        groundCheckDistance = 0.01f;
    }

    public void ResetGroundCheckDistance()
    {
        groundCheckDistance = m_OriginalGroundCheckDistance;
    }


    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;

        m_Rigidbody.AddForce(extraGravityForce);

        //if (m_Rigidbody.velocity.y <= 0.0f)
        //{
        //    groundCheckDistance = m_OriginalGroundCheckDistance;
        //}
        //else
        //{
        //    groundCheckDistance = 0.01f;
        //}

    }

    void FixedUpdate()
    {
        if (m_IsGrounded == false)
        {
            HandleAirborneMovement();
        }
    }

    void Update()
    {
        CheckDirection();

        CheckGroundStatus();

        PCInput(m_IsGrounded);

        UpdateAnimator(m_SpeedInputValue, m_IsGrounded, m_JumpFlag);

      
    }

    private void CheckDirection()
    {
        if (turnFinish == false)
        {
            if (m_NowRightDirectionFlag == true)
            {
                if (Mathf.Abs(Mathf.Abs(this.transform.eulerAngles.y) - 0.0f) <= Time.deltaTime * turnSpeed)
                {
                    this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 0.0f, this.transform.eulerAngles.z);

                    turnFinish = true;
                }
                else
                {
                    turnFinish = false;

                    this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y - Time.deltaTime * turnSpeed, this.transform.eulerAngles.z);
                }

            }
            else
            {
                if (Mathf.Abs(Mathf.Abs(this.transform.eulerAngles.y) - 180.0f) <= Time.deltaTime * turnSpeed)
                {
                    this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 180.0f, this.transform.eulerAngles.z);

                    turnFinish = true;
                }
                else
                {
                    turnFinish = false;

                    this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + Time.deltaTime * turnSpeed, this.transform.eulerAngles.z);
                }
            }
        }
    }

    private void PCInput(bool isGrounded)
    {
        m_SpeedInputValue = Input.GetAxis("Horizontal");

        if (m_WeaponController.currentLeftWeapon.isFiring == false
         && m_WeaponController.currentRightWeapon.isFiring == false)
        {
            if (m_SpeedInputValue > 0.0f)
            {
                m_NowRightDirectionFlag = true;
            }
            else if (m_SpeedInputValue < 0.0f)
            {
                m_NowRightDirectionFlag = false;
            }

            if (m_NowRightDirectionFlag != m_LastWasRightDirectionFlag)
            {
                m_LastWasRightDirectionFlag = m_NowRightDirectionFlag;

                turnFinish = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            m_JumpFlag = true;
        }
        else
        {
            m_JumpFlag = false;
        }
    }

    private void UpdateAnimator(float speedInput,bool isGrounded,bool jump)
    {
        if (isGrounded == true)
        {
            m_Animator.applyRootMotion = true;
        }
        else
        {
            m_Animator.applyRootMotion = false;
        }


        if (turnFinish == true)
        {
            if (m_WeaponController.currentLeftWeapon.isFiring == true
                || m_WeaponController.currentRightWeapon.isFiring == true)
            {
                if (speedInput > 0.0f && m_NowRightDirectionFlag == true)
                {
                    m_Animator.SetFloat("AbsSpeedInput", Mathf.Abs(speedInput));
                }
                else if (speedInput < 0.0f && m_NowRightDirectionFlag == false)
                {
                    m_Animator.SetFloat("AbsSpeedInput", Mathf.Abs(speedInput));
                }
                else
                {
                    m_Animator.SetFloat("AbsSpeedInput", 0.0f);
                }
            }
            else if (m_WeaponController.currentLeftWeapon.isFiring == false
                && m_WeaponController.currentRightWeapon.isFiring == false)
            {
                m_Animator.SetFloat("AbsSpeedInput", Mathf.Abs(speedInput));
            }
            else
            {
                m_Animator.SetFloat("AbsSpeedInput", 0.0f);
            }          
        }
        else
        {
            m_Animator.SetFloat("AbsSpeedInput", 0.0f);
        }


        m_Animator.SetBool("OnGround", isGrounded);

        if (jump == true)
        {
            m_Animator.SetTrigger("Jump");
        }

        m_Animator.SetFloat("YSpeed", m_Rigidbody.velocity.y);     
    }


}
