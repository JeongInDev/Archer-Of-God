using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public Vector2 moveInput;
    public float moveSpeed;

    private Rigidbody2D rb;
    private Vector3 baseScale;
    private Animator anim;
    private SkillCaster caster;
    private Health health;

    // 발사 관련
    [Header("Shooting")] [SerializeField] private float enemyAimOffsetX = 0.5f;

    [Header("Effects")] [SerializeField] GameObject vfxSkillQ;
    [SerializeField] GameObject vfxSkillW;
    [SerializeField] GameObject vfxSkillE;
    [SerializeField] GameObject vfxSkillR;
    [SerializeField] float vfxLifetime = 5.0f;
    private bool vfxPlayed;

    // Skill
    private bool isCasting;
    private bool qCastReady;

    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        anim = GetComponent<Animator>();
        caster = GetComponent<SkillCaster>();
        health = GetComponent<Health>();

        if (health)
            health.OnDied += OnPlayerDied;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (!isCasting)
        {
            Vector2 nextVec = moveInput * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + nextVec);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // 치트 칸
        }
    }

    private void LateUpdate()
    {
        if (isDead) return;

        if (!isCasting)
        {
            // 플레이어 방향 설정
            var s = baseScale;
            s.x *= (moveInput.x < 0f) ? +1f : -1f;
            transform.localScale = s;
        }

        anim.SetFloat("isMoving", Mathf.Abs(moveInput.x));
    }

    void OnMove(InputValue value)
    {
        if (!isCasting)
            moveInput = value.Get<Vector2>();
    }

    void OnSkillQ()
    {
        if (caster == null || anim == null) return;

        if (!caster.CanCast(SkillSlot.Q))
        {
            Debug.Log("Q - 스킬 사용 불가");
            return;
        }

        Debug.Log("OnSkillQ");

        PlayVFXLocal(vfxSkillQ, new Vector3());

        // 시선 오른쪽
        var s = baseScale;
        s.x *= -1f;
        transform.localScale = s;

        anim.SetTrigger("SkillQ");
        qCastReady = true;
        isCasting = true;
    }

    void OnSkillW()
    {
        if (!caster.CanCast(SkillSlot.W))
        {
            Debug.Log("W - 스킬 사용 불가");
            return;
        }

        Debug.Log("OnSkillW");

        PlayVFXLocal(vfxSkillW, new Vector3());

        if (caster == null) return;
        caster.TryCast(SkillSlot.W);
    }

    void OnSkillE()
    {
        if (!caster.CanCast(SkillSlot.E))
        {
            Debug.Log("E - 스킬 사용 불가");
            return;
        }

        Debug.Log("OnSkillE");

        PlayVFXLocal(vfxSkillE, new Vector3());

        // 시선 오른쪽
        var s = baseScale;
        s.x *= -1f;
        transform.localScale = s;

        anim.SetTrigger("SkillE");
        qCastReady = true;
        isCasting = true;
    }

    void OnSkillR()
    {
        if (!caster.CanCast(SkillSlot.R))
        {
            Debug.Log("R - 스킬 사용 불가");
            return;
        }

        Debug.Log("OnSkillR");

        PlayVFXLocal(vfxSkillR, new Vector3());

        // 시선 오른쪽
        var s = baseScale;
        s.x *= -1f;
        transform.localScale = s;

        anim.SetTrigger("SkillR");
        qCastReady = true;
        isCasting = true;
    }

    /// <summary>
    /// 가장 가까운 적 찾기
    /// </summary>
    /// <returns></returns>
    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0)
            return null;

        Transform best = null;
        float bestSqr = float.MaxValue;
        Vector3 from = caster.firePos.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (!enemy.activeInHierarchy)
                continue;

            Health h = enemy.GetComponent<Health>();
            if (h == null || h.IsDead)
                continue;

            float d2 = (enemy.transform.position - from).sqrMagnitude;
            if (d2 < bestSqr)
                bestSqr = d2;
            best = enemy.transform;
        }

        return best;
    }

    // 애니메이션 이벤트
    void AE_FireArrow()
    {
        if (caster == null) return;

        Transform nearestEnemyTransform = FindNearestEnemy();
        if (nearestEnemyTransform == null) return;

        Vector2 start = (caster.firePos ? (Vector2)caster.firePos.position : (Vector2)transform.position);
        Vector2 target = (Vector2)nearestEnemyTransform.position + new Vector2(enemyAimOffsetX, 0f);

        caster.SpawnArrow(start, target);
    }

    void AE_SkillQ()
    {
        if (!qCastReady) return;

        Transform nearestEnemyTransform = FindNearestEnemy();
        caster.TryCast(SkillSlot.Q, nearestEnemyTransform);
        qCastReady = false;
    }

    void AE_SkillE()
    {
        if (!qCastReady) return;

        caster.TryCast(SkillSlot.E, transform);
        qCastReady = false;
    }

    void AE_SkillR()
    {
        if (!qCastReady) return;

        Transform nearestEnemyTransform = FindNearestEnemy();
        caster.TryCast(SkillSlot.R, nearestEnemyTransform);
        qCastReady = false;
    }

    void AE_EndSkill()
    {
        Debug.Log("플레이어 스킬 끄으으으으으읕");

        moveInput = Vector2.zero;
        isCasting = false;
    }

    void OnDisable()
    {
        isCasting = false;
        qCastReady = false;
    }

    void OnDestroy()
    {
        if (health) health.OnDied -= OnPlayerDied; // ★ 해제
    }

    void OnPlayerDied(Health _)
    {
        if (isDead) return;

        isDead = true;
        anim?.SetTrigger("isDead");
        DisableAllColliders();
        EnemyRegistry.TriggerVictoryAll();
    }

    void DisableAllColliders()
    {
        var cols = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
    }

    // 이펙트
    void PlayVFX(GameObject prefab)
    {
        PlayVFX(prefab, Vector3.zero);
    }

    void PlayVFX(GameObject prefab, Vector3 worldOffset)
    {
        if (vfxPlayed || !prefab) return;
        var pos = (Vector3)transform.position + worldOffset;
        var go = Instantiate(prefab, pos, Quaternion.identity);
        if (vfxLifetime > 0f) Destroy(go, vfxLifetime);
        vfxPlayed = true;
    }

    void PlayVFXLocal(GameObject prefab, Vector3 localOffset)
    {
        PlayVFX(prefab, transform.TransformPoint(localOffset) - transform.position);
    }
}