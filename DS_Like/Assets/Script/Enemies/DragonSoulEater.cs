using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSoulEater : BaseEnemy, IDamageable
{
    private float m_AttackTimer = 5f;
    private float m_ResetAttackTimer;

    private BossHealthBar m_HealthBar;
    private bool m_IsHealthBarInit = false;

    private readonly int m_HashRun = Animator.StringToHash("IsRunning");
    private readonly int m_HashAttack = Animator.StringToHash("Attack");
    private readonly int m_HashRandom = Animator.StringToHash("Random");
    private readonly int m_HashDeath = Animator.StringToHash("Death");

    protected override void Awake()
    {
        base.Awake();
        m_ResetAttackTimer = m_AttackTimer;
    }

    private void Start()
    {
        if (m_HealthBar == null)
        {
            m_HealthBar = BossHealthBar.Instance;
            m_HealthBar.gameObject.SetActive(false);
        }
    }

    // Interface methods
    public void TakeDamage(int damageAmount)
    {
        m_HealthBar.Health.TakeDamage(damageAmount);
        if (m_HealthBar.Health.CurrentHealth <= 0f && !m_IsDeath)
        {
            ChangeState(State.Death);
        }
    }

    public override void OnIdleEnter()
    {
        m_Animator.SetBool(m_HashRun, false);
        StopMovement();
    }

    public override void OnIdleUpdate()
    {
        if (IsTargetInRange(m_ChaseThreshold))
            ChangeState(State.Chase);
    }

    public override void OnChaseEnter()
    {
        if (!m_IsHealthBarInit)
        {
            m_HealthBar.SetBossHealth("Dragon Soul Eater", 500);
            m_IsHealthBarInit = true;
        }
        m_Animator.SetBool(m_HashRun, true);
        GainMovement();
    }

    public override void OnChaseUpdate()
    {
        m_Agent.SetDestination(m_Target.transform.position);
        if (IsTargetInRange(m_AttackThreshold))
            ChangeState(State.Attack);
    }

    public override void OnAttackEnter()
    {
        m_Animator.SetBool(m_HashRun, false);
        StopMovement();
    }

    public override void OnAttackUpdate()
    {
        if (!IsTargetInRange(m_AttackThreshold))
        {
            m_Animator.ResetTrigger(m_HashAttack);
            ChangeState(State.Chase);
        }

        m_AttackTimer -= Time.deltaTime;
        if (m_AttackTimer <= 0f)
        {
            int rand = Random.Range(1, 4);
            transform.LookAt(m_Target.transform);
            m_Animator.SetInteger(m_HashRandom, rand);
            m_Animator.SetTrigger(m_HashAttack);
            m_AttackTimer = m_ResetAttackTimer;
        }
    }

    public override void OnDeathEnter()
    {
        m_Animator.SetTrigger(m_HashDeath);
        if (m_OnDeathEvent != null) m_OnDeathEvent.Invoke();
    }

    public override void OnDeathUpdate()
    {
        if (!m_IsDeath)
        {
            m_DespawnTimer -= Time.deltaTime;
            if (m_DespawnTimer <= 0f)
            {
                m_HealthBar.gameObject.SetActive(false);
                m_IsDeath = true;
                enabled = false;
            }
        }
    }
}