using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Animator)), RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour, IDamageable
{
    #region Variables/Props
    [Header("Player Settings")]
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
    [SerializeField] private PotionSlot m_Potion;
    [SerializeField] private HealthBars m_HealthBars;
    [SerializeField] private MeleeWeapon m_Weapon;
    private float m_NextAttackTime = 0f;
    private int m_NbrOfClicks = 0;
    private float m_LastClickedTime = 0;
    private float m_MaxComboDelay = 1;

    private float m_ResetSprintDelay;
    private float m_SprintStamDelay = 0.5f;
    private const int SPRINT_STAM_COST = 2;
    private const int ROLL_STAM_COST = 15;
    private const int JUMP_ATTACK_COST = 25;

    private PlayerInput m_Input;
    private Rigidbody m_RigidBody;
    private Animator m_Animator;
    private CapsuleCollider m_Collider;

    private PhysicMaterial m_FrictionPhysics, m_MaxFrictionPhysics, m_SlippyPhysics;

    private float m_CurrentSpeed = 0f;
    private float m_RotationAngle;
    private float m_GroundAngle;

    private Quaternion m_TargetRotation = new Quaternion();
    private Vector3 m_Forward = new Vector3();
    private Vector3 m_MoveDirection = new Vector3();
    private Transform m_Camera;

    private RaycastHit m_HitInfo;

    private bool m_IsAttacking = false;
    private bool m_IsSprinting = false;
    private bool m_IsRunning = false;
    private bool m_IsDrinking = false;
    //private bool m_IsWalkable = true;
    private bool m_IsRolling = false;
    private bool m_IsDead = false;

    public bool IsDrinking { get => m_IsDrinking; }
    private bool isGrounded
    {
        get => Physics.SphereCast(transform.position + m_Collider.center, m_Collider.radius, Vector3.down, out m_HitInfo, m_Collider.height * 0.5f, m_WalkGround, QueryTriggerInteraction.Ignore);
    }
    public bool IsDead { get => m_IsDead; }
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
    private readonly int m_HashDrink = Animator.StringToHash("Drink");
    private readonly int m_HashJumpAttack = Animator.StringToHash("JumpAttack");
    private readonly int m_HashDeath = Animator.StringToHash("Death");
    private readonly int m_HashRoll = Animator.StringToHash("Roll");
    #endregion

    #region Mono Methods
    private void OnEnable() 
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake ()
    {
        Init();
    }

    private void Start ()
    {
        m_Camera = Camera.main.transform;
        //m_HealthBars = HealthBars.Instance;
    }

    private void Update ()
    {
        CalculateDirection();
        CalculateForward();
        CalculateGroundAngle();
        ControlMaterialPhysics();
        HandleAttackAnim();
        if (m_Input.AttackInput && !m_IsRunning && !m_IsSprinting && !m_IsRolling && !m_IsDrinking)
        {
            if (Time.time > m_NextAttackTime)
                Attack();
        }
        if (m_Input.AttackInput && !isGrounded)
        {
            if (m_RigidBody.velocity.y > 0)
            {
                if (Time.time > m_NextAttackTime)
                    Attack();
            }
        }

        if (m_Input.JumpInput)
        {
            Jump();
        }

        if (m_Input.RollInput)
        {
            Roll();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DrinkPotion();
        }

        if (m_Input.SprintInput && m_HealthBars.Health.CurrentStamina >= SPRINT_STAM_COST)
        {
            if (!m_IsSprinting) m_IsSprinting = true;
            m_SprintStamDelay -= Time.deltaTime;
            if (m_SprintStamDelay <= 0f)
            {
                m_HealthBars.UseStamina(SPRINT_STAM_COST);
                m_SprintStamDelay = m_ResetSprintDelay;
            }
        }
        else
        {
            if (m_IsSprinting) m_IsSprinting = false;
            if (m_CurrentSpeed > m_RunSpeed) m_CurrentSpeed = m_RunSpeed;
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
        if (m_Input.MoveInput != Vector2.zero && !m_IsAttacking && !m_IsDrinking)
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
    private void Init()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_Collider = GetComponent<CapsuleCollider>();
        m_Input = GetComponent<PlayerInput>();

        // Slides the character through walls and edges
        m_FrictionPhysics = new PhysicMaterial();
        m_FrictionPhysics.name = "frictionPhysics";
        m_FrictionPhysics.staticFriction = 0.25f;
        m_FrictionPhysics.dynamicFriction = 0.25f;
        m_FrictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

        // Prevent the collider from slipping on ramps
        m_MaxFrictionPhysics = new PhysicMaterial();
        m_MaxFrictionPhysics.name = "maxFrictionPhysics";
        m_MaxFrictionPhysics.staticFriction = 1f;
        m_MaxFrictionPhysics.dynamicFriction = 1f;
        m_MaxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

        // Air physics
        m_SlippyPhysics = new PhysicMaterial();
        m_SlippyPhysics.name = "slippyPhysics";
        m_SlippyPhysics.staticFriction = 0f;
        m_SlippyPhysics.dynamicFriction = 0f;
        m_SlippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

        m_ResetSprintDelay = m_SprintStamDelay;
    }

    private void ControlMaterialPhysics()
    {
        m_Collider.material = (isGrounded && m_GroundAngle < m_MaxGroundAngle) ? m_FrictionPhysics : m_SlippyPhysics;

        if (isGrounded && m_Input.MoveInput == Vector2.zero) m_Collider.material = m_MaxFrictionPhysics;
        else if (isGrounded && m_Input.MoveInput != Vector2.zero) m_Collider.material = m_FrictionPhysics;
        else m_Collider.material = m_SlippyPhysics;
    }

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
            if (m_IsSprinting && m_CurrentSpeed < m_SprintSpeed)
            {
                m_CurrentSpeed += m_SprintSpeed * (m_RunAcceleration * Time.fixedDeltaTime);
            }
            else if (m_CurrentSpeed < m_RunSpeed)
            {
                m_CurrentSpeed += m_RunSpeed * (m_RunAcceleration * Time.fixedDeltaTime);
                if (!m_IsRunning) m_IsRunning = true;
            }
        }
        else ResetSpeed();
    }

    private void Jump ()
    {
        if (isGrounded && !m_IsRolling && !m_IsDrinking)
        {
            m_RigidBody.AddForce(transform.up * m_JumpForce);
        }
    }

    private void Roll()
    {
        if (!m_IsRolling && isGrounded && !m_IsAttacking && !m_IsDrinking)
        {
            if (m_HealthBars.Health.CurrentStamina >= ROLL_STAM_COST)
            {
                m_IsRolling = true;
                m_Animator.SetTrigger(m_HashRoll);
                m_HealthBars.UseStamina(ROLL_STAM_COST);
            }
        }
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

    private void DrinkPotion()
    {
        if (!m_IsDrinking)
        {
            m_IsDrinking = true;
            m_Animator.SetBool(m_HashDrink, true);
            m_Potion.DrinkPotion();
        }
    }

    public void OnDrinkEnd()
    {
        m_IsDrinking = false;
        m_Animator.SetBool(m_HashDrink, false);
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
        if (isGrounded)
        {
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
        else
        {
            if (m_HealthBars.Health.CurrentStamina < JUMP_ATTACK_COST) return;
            m_HealthBars.UseStamina(JUMP_ATTACK_COST);
            m_Weapon.SetJumpAttackDamage();
            if (m_NbrOfClicks == 1) m_Animator.SetTrigger(m_HashJumpAttack);
        }
    }

    public void OnAttackStart()
    {
        m_IsAttacking = true;
        m_Weapon.ActivateWeaponCollider();
    }

    public void OnAttackEnd()
    {
        m_IsAttacking = false;
        m_Weapon.DeactivateWeaponCollider();
    }

    public void OnJumpAttackEnd()
    {
        m_Weapon.ResetDamage();
    }
    #endregion

    // IDamageable method
    public void TakeDamage(int damageAmount, bool ignoreRoll = false)
    {
        if (!ignoreRoll || m_IsDead)
        {
            if (m_IsRolling || m_IsDead) return;
        }

        m_HealthBars.TakeDamage(damageAmount);
        if (m_HealthBars.Health.CurrentHealth <= 0f)
        {
            if (m_IsRolling) m_IsRolling = false;
            m_IsDead = true;
            m_Animator.SetTrigger(m_HashDeath);
            m_Input.ReleaseControl();
        }
    }
}