using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : PoolableObject
{
    [SerializeField] private GameObject m_ImpactEffect;
    [Space]
    [Range(0f, 1f)]
    [SerializeField] private float m_Bounciness;
    [SerializeField] private bool m_UseGrabity = true;
    [Space]
    [SerializeField] private int m_DamageAmount;
    [SerializeField] private float m_ExplosionRange;
    [SerializeField] private int m_MaxCollisions;
    [SerializeField] private bool m_ExplodeOnTouch = true;
    [Space]
    [SerializeField] private bool m_IgnoreRoll = true;

    private LayerMask m_TargetLayer;
    private PhysicMaterial m_PhysicsMat;
    private Rigidbody m_Rigidbody;
    private int m_Collisions;
    private bool m_HasExploded = false;

    public LayerMask TargetLayer { get => m_TargetLayer; set => m_TargetLayer = value; }
    public Rigidbody Rigidbody { get => m_Rigidbody; }

    protected override void Awake()
    {
        base.Awake();

        SetUp();
    }

    protected override void Update()
    {
        if (m_Collisions > m_MaxCollisions)
        {
            Explode();
        }

        m_LifeTime -= Time.deltaTime;
        if (m_LifeTime <= 0f)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_Collisions++;
        if (0 != (m_TargetLayer.value & 1 << collision.gameObject.layer) && m_ExplodeOnTouch)
        {
            Explode();
        }
    }

    private void SetUp()
    {
        m_PhysicsMat = new PhysicMaterial();
        m_PhysicsMat.bounciness = m_Bounciness;
        m_PhysicsMat.frictionCombine = PhysicMaterialCombine.Minimum;
        m_PhysicsMat.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<Collider>().material = m_PhysicsMat;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.useGravity = m_UseGrabity;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        m_Collisions = 0;
        m_Rigidbody.velocity = Vector3.zero;
        m_HasExploded = false;
    }

    private void Explode()
    {
        if (m_HasExploded) return;

        if (m_ImpactEffect != null)
        {
            PoolMgr.Instance.Spawn(m_ImpactEffect.name, transform.position, Quaternion.identity);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, m_ExplosionRange, m_TargetLayer);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].GetComponent<IDamageable>() != null) enemies[i].GetComponent<IDamageable>().TakeDamage(m_DamageAmount, m_IgnoreRoll);
        }

        m_HasExploded = true;
        ClearObject();
    }
}