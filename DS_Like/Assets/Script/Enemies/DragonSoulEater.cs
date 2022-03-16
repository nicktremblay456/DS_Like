using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSoulEater : BaseEnemy
{
    #region Variables/Props
    [SerializeField] private Transform m_FireballSpawn;
    [SerializeField] private GameObject m_FireballObj;
    [SerializeField] private float m_FireballSpawnForce;
    [SerializeField] private MeleeWeapon m_Tail;
    [SerializeField] private MeleeWeapon m_Jaw;
    [Space]
    [SerializeField] private bool m_IsBoss = false;

    private Coroutine m_FireballRoutine;

    private float m_AttackTimer = 3f;
    private float m_ResetAttackTimer;

    private BossHealthBar m_HealthBar;
    private bool m_IsHealthBarInit = false;

    private bool m_IsCasting = false;
    private bool m_IsAttacking = false;

    private readonly int m_HashRun = Animator.StringToHash("IsRunning");
    private readonly int m_HashAttack = Animator.StringToHash("Attack");
    private readonly int m_HashRandom = Animator.StringToHash("Random");
    private readonly int m_HashDead = Animator.StringToHash("IsDead");
    #endregion

    protected override void Awake()
    {
        base.Awake();
        m_ResetAttackTimer = m_AttackTimer;
    }

    private void Start()
    {
        if (m_IsBoss && m_HealthBar == null)
        {
            m_HealthBar = BossHealthBar.Instance;
            //m_HealthBar.gameObject.SetActive(false);
        }
    }

    // IDamageable
    public override void TakeDamage(int damageAmount, bool ignoreRoll = false)
    {
        if (m_IsBoss)
        {
            m_HealthBar.Health.TakeDamage(damageAmount);
            if (m_HealthBar.Health.CurrentHealth <= 0f && !m_IsDeath) ChangeState(State.Death);
        }
        else base.TakeDamage(damageAmount, ignoreRoll);
    }

    #region States
    public override void OnIdleEnter()
    {
        m_Animator.SetBool(m_HashRun, false);
        StopMovement();
    }

    public override void OnIdleUpdate()
    {
        if (IsTargetInRange(m_ChaseThreshold) && !m_Target.IsDead)
            ChangeState(State.Chase);
    }

    public override void OnChaseEnter()
    {
        if (m_IsBoss && !m_IsHealthBarInit)
        {
            m_HealthBar.SetBossHealth("Dragon Soul Eater", m_MaxHealth);
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
        Attack();
    }

    public override void OnAttackUpdate()
    {
        if (!IsTargetInRange(m_AttackThreshold) && !m_IsCasting && !m_IsAttacking)
        {
            m_Animator.ResetTrigger(m_HashAttack);
            ChangeState(State.Chase);
        }
        if (m_Target.IsDead) ChangeState(State.Idle);

        if (!m_IsDeath)
        {
            m_AttackTimer -= Time.deltaTime;
            if (m_AttackTimer <= 0f)
            {
                Attack();
            }
        }
    }

    public override void OnAttackExit()
    {
        m_AttackTimer = m_ResetAttackTimer;
        m_Animator.ResetTrigger(m_HashAttack);
        DeactivateDamageTrigger();

        if (!m_IsDeath)
        {
            // Spawn Fireball
            m_Animator.SetInteger(m_HashRandom, 3);
            m_Animator.SetTrigger(m_HashAttack);
        }
    }

    public override void OnDeathEnter()
    {
        m_Animator.ResetTrigger(m_HashAttack);
        m_Animator.SetBool(m_HashDead, true);
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
                DeactivateDamageTrigger();
                enabled = false;
            }
        }
    }
    #endregion

    #region Combat
    private void Attack()
    {
        int rand = Random.Range(1, 3);
        transform.LookAt(m_Target.transform);
        m_Animator.SetInteger(m_HashRandom, rand);
        m_Animator.SetTrigger(m_HashAttack);
        m_AttackTimer = m_ResetAttackTimer;
    }

    private void DeactivateDamageTrigger()
    {
        if (m_Tail.Collider.enabled) m_Tail.Collider.enabled = false;
        if (m_Jaw.Collider.enabled) m_Jaw.Collider.enabled = false;
    }

    public void SpawnFireball()
    {
        if (m_FireballRoutine != null) StopCoroutine(m_FireballRoutine);
        m_FireballRoutine = StartCoroutine(FireballAttack());
    }

    private IEnumerator FireballAttack()
    {
        m_IsCasting = true;
        for (int i = 0; i < 5; i++)
        {
            Ray ray = new Ray(m_FireballSpawn.position, m_FireballSpawn.forward);
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit)) targetPoint = hit.point;
            else targetPoint = ray.GetPoint(75);

            Vector3 directionWithoutSpread = targetPoint - transform.position;
            transform.LookAt(m_Target.transform);
            Missile fireball = PoolMgr.Instance.Spawn(m_FireballObj.name, m_FireballSpawn.transform.position, Quaternion.identity).GetComponent<Missile>();
            fireball.TargetLayer = m_Target.gameObject.layer;

            fireball.Rigidbody.AddForce(directionWithoutSpread.normalized * m_FireballSpawnForce, ForceMode.Impulse);

            yield return new WaitForSeconds(0.25f);
        }
        m_IsCasting = false;
    }
    #endregion

    #region Animation Event Methods
    public void OnTailAttackStart()
    {
        m_IsAttacking = true;
        m_Tail.ActivateWeaponCollider();
    }

    public void OnTailAttackEnd()
    {
        m_IsAttacking = false;
        m_Tail.DeactivateWeaponCollider();
    }

    public void OnBiteAttackStart()
    {
        m_IsAttacking = true;
        m_Jaw.ActivateWeaponCollider();
    }

    public void OnBiteAttackEnd()
    {
        m_IsAttacking = false;
        m_Jaw.DeactivateWeaponCollider();
    }
    #endregion
}