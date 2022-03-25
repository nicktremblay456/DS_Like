using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan : BaseEnemy
{
    [SerializeField] private MeleeWeapon m_RightArm;
    [SerializeField] private MeleeWeapon m_LeftArm;
    [SerializeField] private int m_GroundAttackDamage;
    [SerializeField] private ParticleSystem m_DamageableAreaParticle;
    [Space]
    [SerializeField] private bool m_IsBoss = false;

    private float m_AttackTimer = 3f;
    private float m_ResetTimer;

    private BossHealthBar m_HealthBar;
    private bool m_IsHealthBarInit = false;

    private bool m_IsAttacking = false;
    private bool m_CanTakeDamage = false;

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
        m_DamageableAreaParticle.Stop();
    }

    private void Start()
    {
        if (m_IsBoss && m_HealthBar == null)
        {
            m_HealthBar = BossHealthBar.Instance;
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        m_Animator.SetInteger(m_HashSpeed, (int)m_Agent.velocity.magnitude);
        m_Animator.SetBool(m_HashDead, m_IsDead);
    }

    public override void TakeDamage(int damageAmount, bool ignoreRoll = false)
    {
        if (!m_CanTakeDamage) return;

        if (!m_IsDead)
        {
            m_Animator.SetTrigger(m_HashAttackThree);
            DeactivateDamageTrigger();
            m_AttackTimer = m_ResetTimer;
        }

        if (m_IsBoss)
        {
            if (!m_IsEngaged) m_IsEngaged = true;
            m_HealthBar.Health.TakeDamage(damageAmount);
            if (m_HealthBar.Health.CurrentHealth <= 0f && !m_IsDead) ChangeState(State.Death);
        }
        else base.TakeDamage(damageAmount, ignoreRoll);

        if (!m_DamageableAreaParticle.isStopped) m_DamageableAreaParticle.Stop();
        m_CanTakeDamage = false;
    }

    #region States
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
        m_Animator.SetTrigger(m_HashAttackOne);
        StopMovement();
    }

    protected override void OnAttackUpdate()
    {
        if (!IsTargetInRange(m_AttackThreshold + 1.5f) && !m_IsAttacking)
        {
            ChangeState(State.Chase);
        }
        if (m_Target.IsDead && !m_IsAttacking) ChangeState(State.Idle);

        m_AttackTimer -= Time.deltaTime;
        if (m_AttackTimer <= 0)
        {
            Attack();
        }
    }

    protected override void OnAttackExit()
    {
        m_AttackTimer = m_ResetTimer;   
    }

    protected override void OnDeathEnter()
    {
        m_IsDead = true;
        DeactivateDamageTrigger();
        if (m_OnDeathEvent != null) m_OnDeathEvent.Invoke();
    }

    protected override void OnDeathUpdate()
    {
        m_DespawnTimer -= Time.deltaTime;
        if (m_DespawnTimer <= 0f && !m_CanTakeDamage)
        {
            m_HealthBar.gameObject.SetActive(false);
            enabled = false;
        }
    }
    #endregion

    private void Attack()
    {
        transform.LookAt(m_Target.transform);
        if (m_Target.IsGrounded) m_Animator.SetTrigger(m_HashAttackOne);
        else m_Animator.SetTrigger(m_HashAttackTwo);
        m_AttackTimer = m_ResetTimer;
    }

    private void DeactivateDamageTrigger()
    {
        if (m_LeftArm.Collider.enabled) m_LeftArm.DeactivateWeaponCollider();
        if (m_RightArm.Collider.enabled) m_RightArm.DeactivateWeaponCollider();
    }

    #region Animation Event Methods
    public void OnLeftAttackStart()
    {
        if (m_DamageableAreaParticle.isStopped) m_DamageableAreaParticle.Play();
        m_CanTakeDamage = true;
        m_IsAttacking = true;
        m_LeftArm.ActivateWeaponCollider();
    }

    public void OnRightAttackStart()
    {
        if (m_DamageableAreaParticle.isStopped) m_DamageableAreaParticle.Play();
        m_CanTakeDamage = true;
        m_IsAttacking = true;
        m_RightArm.ActivateWeaponCollider();
    }

    public void OnLeftAttackEnd()
    {
        m_IsAttacking = false;
        m_LeftArm.DeactivateWeaponCollider();
    }

    public void OnRightAttackEnd()
    {
        m_IsAttacking = false;
        m_RightArm.DeactivateWeaponCollider();
    }

    public void OnGroundAttackHit()
    {
        m_IsAttacking = true;
        transform.LookAt(m_Target.transform);
        PoolMgr.Instance.Spawn("FX_Dark_Explosion", m_RightArm.transform.position, Quaternion.identity);
        Collider[] rangeChecks = Physics.OverlapSphere(m_RightArm.transform.position, 2.5f, LayerMask.GetMask("Player"));

        foreach(Collider hit in rangeChecks)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null) damageable.TakeDamage(m_GroundAttackDamage, false);
        }
    }
    #endregion
}