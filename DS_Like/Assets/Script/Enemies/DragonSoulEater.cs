using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSoulEater : BaseEnemy, IDamageable
{
    private BossHealthBar m_HealthBar;

    private readonly int m_HashRun = Animator.StringToHash("IsRunning");
    private readonly int m_HashDeath = Animator.StringToHash("Death");

    private void Start()
    {
        if (m_HealthBar == null)
        {
            m_HealthBar = BossHealthBar.Instance;
            m_HealthBar.SetBossHealth("Dragon Soul Eater", 500);
        }
    }

    // Interface methods
    public void TakeDamage(int damageAmount)
    {
        m_HealthBar.Health.TakeDamage(damageAmount);
        if (m_HealthBar.Health.CurrentHealth <= 0f && !m_IsDeath)
        {
            Debug.Log("Is Dead");
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
            ChangeState(State.Chase);
    }

    public override void OnDeathEnter()
    {
        m_Animator.SetTrigger(m_HashDeath);
        if (m_OnDeathEvent != null) m_OnDeathEvent.Invoke();
    }

    public override void OnDeathUpdate()
    {
        
    }
}