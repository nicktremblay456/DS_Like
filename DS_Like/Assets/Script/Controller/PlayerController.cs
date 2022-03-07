using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    #region Variables/Props
    [SerializeField] private float m_AttackTreshold = 5.0f;
    [SerializeField] private float m_SpherecastRadius = 5.0f;

    [Space, Header("Player Settings")]
    [SerializeField] private float m_RunSpeed = 10f;
    [SerializeField] private float m_SprintSpeed = 15f;
    [SerializeField] private float m_RunAcceleration = 5f;
    [SerializeField] private float m_TurnSpeed = 10f;
    [SerializeField] private float m_JumpForce = 650f;
    [SerializeField] private float m_RollingForce = 200f;
    [SerializeField] private float m_Gravity = 20f;
    
    [SerializeField] private float m_MaxGroundAngle = 45f;
    [SerializeField] private LayerMask m_WalkGround;

    [Space, Header("Combat Settings")]
    [SerializeField] private float m_AtkCooldownTime = 2f;
    private float m_NextAttackTime = 0f;
    private int m_NbrOfClicks = 0;
    private float m_LastClickedTime = 0;
    private float m_MaxComboDelay = 1;

    private PlayerInput m_Input;
    private Rigidbody m_RigidBody;
    private Animator m_Animator;
    private CapsuleCollider m_Collider;

    private float m_CurrentSpeed = 0f;
    private float m_RotationAngle;
    private float m_GroundAngle;

    private Quaternion m_TargetRotation = new Quaternion();
    private Transform m_Camera;
    private Transform m_Follow;
    private Transform m_LookAt;

    private Vector3 m_Forward = new Vector3();
    private RaycastHit m_HitInfo;
    private Vector3 m_MoveDirection = new Vector3();

    private bool m_IsSprinting = false;
    private bool m_IsRunning = false;
    //private bool m_IsWalkable = true;
    private bool m_IsRolling = false;
    private bool isGrounded
    {
        get
        {
            return Physics.SphereCast(transform.position + m_Collider.center, m_Collider.radius, Vector3.down, out m_HitInfo, m_Collider.height * 0.5f, m_WalkGround, QueryTriggerInteraction.Ignore);
        }
    }
    public float ColliderHeight
    {
        get => m_Collider.height;
    }
    public Vector3 ColliderCenter
    {
        get => m_Collider.center;
    }
    public Rigidbody GetRigidbody
    {
        get => m_RigidBody;
    }

    private readonly int m_HashGrounded = Animator.StringToHash("Grounded");
    private readonly int m_HashSpeed = Animator.StringToHash("Speed");
    private readonly int m_HashVelocityY = Animator.StringToHash("VelocityY");
    private readonly int m_HashAttackOne = Animator.StringToHash("Attack_1");
    private readonly int m_HashAttackTwo = Animator.StringToHash("Attack_2");
    private readonly int m_HashAttackThree = Animator.StringToHash("Attack_3");
    private readonly int m_HashDeath = Animator.StringToHash("Death");
    private readonly int m_HashRoll = Animator.StringToHash("Roll");
    #endregion

    #region Mono Methods
    private void Reset() 
    {
        //Transform footStepSource = transform.Find("FootStepSource");

        if (m_Follow == null)
        {
            m_Follow = transform;
        }
        if (m_LookAt == null)
        {
            m_LookAt = m_Follow.Find("HeadTarget");
        }
    }

    private void OnEnable() 
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake ()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_Collider = GetComponent<CapsuleCollider>();
        m_Input = GetComponent<PlayerInput>();
    }

    private void Start ()
    {
        m_Camera = Camera.main.transform;
    }

    private void Update ()
    {
        CalculateDirection();
        CalculateForward();
        CalculateGroundAngle();
        HandleAttackAnim();
        if (m_Input.AttackInput && !m_IsRunning && ! m_IsSprinting && !m_IsRolling)
        {
            if (Time.time > m_NextAttackTime)
            {
                Attack();
            }
        }

        if (m_Input.JumpInput)
        {
            Jump();
        }

        if (m_Input.RollInput)
        {
            if (!m_IsRolling && isGrounded)
            {
                m_IsRolling = true;
                m_Animator.SetTrigger(m_HashRoll);
            }
        }

        HandleMovementAnimation();
        //if (m_ActiveDebug)
        //{
        //    DrawDebugLines();
        //}
    }

    private void FixedUpdate ()
    {
        // Physics
        if (m_Input.MoveInput != Vector2.zero)
        {
            Rotate();
            SetSpeed();
        }
        else ResetSpeed();

        if (m_IsRolling)
        {
            m_RigidBody.AddForce(transform.forward * m_RollingForce, ForceMode.VelocityChange);
            m_CurrentSpeed = 0;
        }

        m_MoveDirection = m_Forward * m_CurrentSpeed;
        m_MoveDirection.y = m_RigidBody.velocity.y;

        if (m_RigidBody.velocity.y != 0)
        {
            m_MoveDirection.y -= m_Gravity * Time.fixedDeltaTime;
        }
        m_RigidBody.velocity = m_MoveDirection;
    }
    #endregion

    #region Controller Movement
    private void HandleMovementAnimation()
    {
        m_Animator.SetFloat(m_HashVelocityY, m_RigidBody.velocity.y);
        m_Animator.SetInteger(m_HashSpeed, (int)m_CurrentSpeed);
        m_Animator.SetBool(m_HashGrounded, isGrounded);
    }

    private void CalculateDirection ()
    {
        // Calcul l'angle selon l'input
        m_RotationAngle = Mathf.Atan2(m_Input.MoveInput.x, m_Input.MoveInput.y);
        // Transfère l'angle en degré.
        m_RotationAngle *= Mathf.Rad2Deg;
        // Rotation relative a la camera.
        m_RotationAngle += m_Camera.eulerAngles.y;
    }

    private void CalculateForward ()
    {
        // Calculer le forward selon l'angle du sol.
        if (isGrounded)
        {
            // Retourne le vector "Forward" en considérant la "pente".
            m_Forward = Vector3.Cross(m_HitInfo.normal, -transform.right);
        }
        else m_Forward = transform.forward;
    }

    private void CalculateGroundAngle ()
    {
        // Calcule l'angle du sol, en utilisant sa normal.
        //Debug.Log("Ground Angle: " + m_GroundAngle + " | " + "m_MaxGroundAngle: " + (m_GroundAngle + 90f));
        if (isGrounded)
        {
            // On détermine l'angle du sol en utilisant la normal et notre forward.            
            m_GroundAngle = Vector3.Angle(m_HitInfo.normal, transform.forward);
        }
        else  m_GroundAngle = 90f;
    }

    private void Rotate ()
    {
        // Convertis notre eulerAngle en quaternion.
        m_TargetRotation = Quaternion.Euler(0f , m_RotationAngle, 0f);
        // TO DO -> Add a check if joystick.x != 0, use a bigger turn speed. This will prevent any unwanted drifting effect. By snapping the rotation.
        transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.fixedDeltaTime);
    }

    private void SetSpeed ()
    {
        if (m_GroundAngle < m_MaxGroundAngle + 90f)
        {
            if (m_Input.SprintInput)
            {
                if (m_CurrentSpeed < m_SprintSpeed)
                {
                    m_CurrentSpeed += m_SprintSpeed * (m_RunAcceleration * Time.fixedDeltaTime);
                    if (!m_IsSprinting)
                        m_IsSprinting = true;
                }
            }
            else
            {
                if (m_CurrentSpeed < m_RunSpeed)
                {
                    m_CurrentSpeed += m_RunSpeed * (m_RunAcceleration * Time.fixedDeltaTime);
                    if (!m_IsRunning)
                        m_IsRunning = true;
                }
            }
        }
        else ResetSpeed();
    }

    private void Jump ()
    {
        if (isGrounded && !m_IsRolling)
            m_RigidBody.AddForce(transform.up * m_JumpForce);
    }

    private void ResetSpeed ()
    {
        m_CurrentSpeed = 0f;
        if (m_IsRunning) m_IsRunning = false;
        if (m_IsSprinting) m_IsSprinting = false;
    }

    public void Death ()
    {
        m_Input.ReleaseControl();
        m_Animator.SetTrigger(m_HashDeath);
    }

    public void OnRollingEnd()
    {
        m_IsRolling = false;
    }
    #endregion

    #region Combat Methods
    private void HandleAttackAnim()
    {
        if (IsAttackAnim("Attack_1")) m_Animator.SetBool(m_HashAttackOne, false);
        if (IsAttackAnim("Attack_2")) m_Animator.SetBool(m_HashAttackTwo, false);
        if (IsAttackAnim("Attack_3"))
        {
            m_Animator.SetBool(m_HashAttackThree, false);
            m_NbrOfClicks = 0;
        }

        if (Time.time - m_LastClickedTime > m_MaxComboDelay) m_NbrOfClicks = 0;
    }

    private bool IsAttackAnim(string a_Name)
    {
        return (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f && 
                m_Animator.GetCurrentAnimatorStateInfo(0).IsName(a_Name));
    }

    private void Attack()
    {
        m_LastClickedTime = Time.time;
        m_NbrOfClicks++;
        if (m_NbrOfClicks == 1) m_Animator.SetBool(m_HashAttackOne, true); ;
        m_NbrOfClicks = Mathf.Clamp(m_NbrOfClicks, 0, 3);

        if (m_NbrOfClicks >= 2 && IsAttackAnim("Attack_1"))
        {
            m_Animator.SetBool(m_HashAttackOne, false);
            m_Animator.SetBool(m_HashAttackTwo, true);
        }
        if (m_NbrOfClicks >= 3 && IsAttackAnim("Attack_2"))
        {
            m_Animator.SetBool(m_HashAttackTwo, false);
            m_Animator.SetBool(m_HashAttackThree, true);
        }
    }
    #endregion
}