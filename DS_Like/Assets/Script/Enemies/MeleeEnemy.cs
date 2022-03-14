using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    [SerializeField] private MeleeWeapon m_Sword;

    private float m_AttackTimer = 2.5f;
    private float m_ResetTimer;

    private bool m_IsAttacking = false;

    private readonly int m_HashWalk = Animator.StringToHash("IsWalking");
    private readonly int m_HashAttack = Animator.StringToHash("Attack");
    private readonly int m_HashDeath = Animator.StringToHash("Death");

    protected override void Awake()
    {
        base.Awake();
        m_ResetTimer = m_AttackTimer;
    }

    public override void OnIdleEnter()
    {
        m_Animator.SetBool(m_HashWalk, false);
        StopMovement();
    }

    public override void OnIdleUpdate()
    {
        if (IsTargetInRange(m_ChaseThreshold) && !m_Target.IsDead)
            ChangeState(State.Chase);
    }

    public override void OnChaseEnter()
    {
        m_Animator.SetBool(m_HashWalk, true);
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
        m_Animator.SetBool(m_HashWalk, false);
        StopMovement();
    }

    public override void OnAttackUpdate()
    {
        if (!IsTargetInRange(m_AttackThreshold) && !m_IsAttacking)
        {
            m_Animator.ResetTrigger(m_HashAttack);
            ChangeState(State.Chase);
        }
        if (m_Target.IsDead && !m_IsAttacking) ChangeState(State.Idle);

        m_AttackTimer -= Time.deltaTime;
        if (m_AttackTimer <= 0f)
        {
            Attack();
        }
    }

    public override void OnAttackExit()
    {
        m_Animator.ResetTrigger(m_HashAttack);
        m_Sword.DeactivateWeaponCollider();
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
                m_Sword.DeactivateWeaponCollider();
                m_IsDeath = true;
                enabled = false;
            }
        }
    }

    private void Attack()
    {
        transform.LookAt(m_Target.transform);
        m_Animator.SetTrigger(m_HashAttack);
        m_AttackTimer = m_ResetTimer;
    }

    public void OnAttackStart()
    {
        m_IsAttacking = true;
        m_Sword.ActivateWeaponCollider();
    }

    public void OnAttackEnd()
    {
        m_IsAttacking = false;
        m_Sword.DeactivateWeaponCollider();
    }
}