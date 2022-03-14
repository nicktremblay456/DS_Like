using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using TNT.StateMachine;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected int m_MaxHealth;
    [SerializeField] protected float m_ChaseThreshold;
    [SerializeField] protected float m_AttackThreshold;
    [Space]
    [SerializeField] protected float m_DespawnTimer = 2.5f;

    [SerializeField] protected UnityEvent m_OnDeathEvent;

    protected bool m_IsDeath = false;

    protected enum State
    {
        Idle, Chase, Attack, Death,
    }

    protected StateMachine m_SM;
    protected Animator m_Animator;
    protected NavMeshAgent m_Agent;
    protected PlayerController m_Target;
    protected Health m_Health;

    protected virtual void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        m_Target = FindObjectOfType<PlayerController>();

        m_Health = new Health(m_MaxHealth, 0);

        InitSM();
    }

    protected virtual void Update()
    {
        m_SM.UpdateSM();
    }

    protected virtual void InitSM()
    {
        m_SM = new StateMachine();

        m_SM.AddState((int)State.Idle);
        m_SM.AddState((int)State.Chase);
        m_SM.AddState((int)State.Attack);
        m_SM.AddState((int)State.Death);

        m_SM.OnEnter((int)State.Idle, OnIdleEnter);
        m_SM.OnEnter((int)State.Chase, OnChaseEnter);
        m_SM.OnEnter((int)State.Attack, OnAttackEnter);
        m_SM.OnEnter((int)State.Death, OnDeathEnter);

        m_SM.OnUpdate((int)State.Idle, OnIdleUpdate);
        m_SM.OnUpdate((int)State.Chase, OnChaseUpdate);
        m_SM.OnUpdate((int)State.Attack, OnAttackUpdate);
        m_SM.OnUpdate((int)State.Death, OnDeathUpdate);

        m_SM.OnExit((int)State.Attack, OnAttackExit);

        m_SM.Init((int)State.Idle);
    }

    protected void GainMovement() => m_Agent.isStopped = false;

    protected void StopMovement()
    {
        m_Agent.isStopped = true;
        m_Agent.velocity = Vector3.zero;
    }

    protected void ChangeState(State state)
    {
        if (m_SM.CurrentState != (int)state) m_SM.ChangeState((int)state);
    }

    protected bool IsTargetInRange(float threshold)
    {
        return Vector3.Distance(transform.position, m_Target.transform.position) <= threshold;
    }

    // IDamageable methods
    public virtual void TakeDamage(int damageAmount, bool ignoreRoll = false)
    {
        m_Health.TakeDamage(damageAmount);
        if (m_Health.CurrentHealth <= 0f) ChangeState(State.Death);
    }

    #region Abstract Methods
    public abstract void OnIdleEnter();
    public abstract void OnIdleUpdate();

    public abstract void OnChaseEnter();
    public abstract void OnChaseUpdate();

    public abstract void OnAttackEnter();
    public abstract void OnAttackUpdate();
    public abstract void OnAttackExit();

    public abstract void OnDeathEnter();
    public abstract void OnDeathUpdate();
    #endregion
}