using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan : BaseEnemy
{
    [SerializeField] private MeleeWeapon m_Weapon;
    [Space]
    [SerializeField] private bool m_IsBoss = false;

    private float m_AttackTimer = 3f;
    private float m_ResetTimer;

    private BossHealthBar m_HealthBar;
    private bool m_IsHealthBarInit = false;

    private bool m_IsAttacking = false;

    private readonly int m_HashSpeed = Animator.StringToHash("Speed");
    private readonly int m_HashDead = Animator.StringToHash("IsDead");
    private readonly int m_HashTakeDmg = Animator.StringToHash("TakeDamage");
    private readonly int m_HashAttackOne = Animator.StringToHash("Attack_01");
    private readonly int m_HashAttackTwo = Animator.StringToHash("Attack_02");
    private readonly int m_HashAttackThree = Animator.StringToHash("Attack_03");

    protected override void Awake()
    {
        base.Awake();
        m_ResetTimer = m_AttackTimer;
    }

    private void Start()
    {
        if (m_IsBoss && m_HealthBar == null)
        {

        }
    }

    protected override void Update()
    {
        base.Update();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        m_Animator.SetInteger(m_HashSpeed, (int)m_Agent.velocity.z);
        m_Animator.SetBool(m_HashDead, m_IsDead);
    }

    public override void TakeDamage(int damageAmount, bool ignoreRoll = false)
    {
        if (!m_IsDead) m_Animator.SetTrigger(m_HashTakeDmg);
        if (m_Weapon.Collider.enabled) m_Weapon.DeactivateWeaponCollider();

        base.TakeDamage(damageAmount, ignoreRoll);
    }

    protected override void OnIdleEnter()
    {
        StopMovement();
    }

    protected override void OnIdleUpdate()
    {
        if ((m_Fov.CanSeePlayer && !m_Target.IsDead) || m_IsEngaged) ChangeState(State.Chase);
    }

    protected override void OnChaseEnter()
    {
        if (m_IsBoss && !m_IsHealthBarInit)
        {
            m_HealthBar.SetBossHealth("Titan", m_MaxHealth);
            m_IsHealthBarInit = true;
        }
        GainMovement();
    }

    protected override void OnChaseUpdate()
    {
        m_Agent.SetDestination(m_Target.transform.position);
        if (IsTargetInRange(m_AttackThreshold))
            ChangeState(State.Attack);
    }

    protected override void OnAttackEnter()
    {
        StopMovement();
    }

    protected override void OnAttackExit()
    {
        
    }

    protected override void OnAttackUpdate()
    {
        
    }

    protected override void OnDeathEnter()
    {
        m_IsDead = true;
        if (m_OnDeathEvent != null) m_OnDeathEvent.Invoke();
    }

    protected override void OnDeathUpdate()
    {
        if (!m_IsDead)
        {
            m_DespawnTimer -= Time.deltaTime;
            if (m_DespawnTimer <= 0f)
            {
                m_HealthBar.gameObject.SetActive(false);
                //DeactivateDamageTrigger();
                enabled = false;
            }
        }
    }
}