using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    [SerializeField] private MeleeWeapon m_Weapon;

    [SerializeField] private float m_AttackDelay = 2.5f;
    private float m_ResetTimer;

    private Collider m_Collider;
    private bool m_IsAttacking = false;

    private readonly int m_HashSpeed = Animator.StringToHash("Speed");
    private readonly int m_HashAttack = Animator.StringToHash("Attack");
    private readonly int m_HashTakeDmg = Animator.StringToHash("TakeDamage");
    private readonly int m_HashDead = Animator.StringToHash("IsDead");

    protected override void Awake()
    {
        base.Awake();
        m_Collider = GetComponentInChildren<Collider>();
        if (m_IsDead) m_Collider.enabled = false;
        m_ResetTimer = m_AttackDelay;
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
        if (!m_IsPatrol) StopMovement();
    }

    protected override void OnIdleUpdate()
    {
        if (m_IsPatrol) Patrol();
        if (m_Fov.CanSeePlayer && !m_Target.IsDead || m_IsEngaged) ChangeState(State.Chase);
    }

    protected override void OnChaseEnter()
    {
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
        //Attack();
    }

    protected override void OnAttackUpdate()
    {
        if (!IsTargetInRange(m_AttackThreshold) && !m_IsAttacking)
        {
            m_Animator.ResetTrigger(m_HashAttack);
            ChangeState(State.Chase);
        }
        if (m_Target.IsDead && !m_IsAttacking) ChangeState(State.Idle);

        m_AttackDelay -= Time.deltaTime;
        if (m_AttackDelay <= 0f) Attack();
    }

    protected override void OnAttackExit()
    {
        m_Animator.ResetTrigger(m_HashAttack);
        m_Weapon.DeactivateWeaponCollider();
    }

    protected override void OnDeathEnter()
    {
        m_IsDead = true;
        m_Weapon.DeactivateWeaponCollider();
        m_Animator.SetBool(m_HashDead, m_IsDead);
        if (m_OnDeathEvent != null) m_OnDeathEvent.Invoke();
    }

    protected override void OnDeathUpdate()
    {

    }

    private void Attack()
    {
        transform.LookAt(m_Target.transform);
        m_Animator.SetTrigger(m_HashAttack);
        m_AttackDelay = m_ResetTimer;
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
}