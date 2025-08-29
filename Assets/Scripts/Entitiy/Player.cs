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
    
    // 발사 관련
    [Header("Shooting")]
    [SerializeField] private float enemyAimOffsetX = 0.5f;
    
    // Skill
    private bool isCasting;
    private bool qCastReady;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        anim = GetComponent<Animator>();
        caster = GetComponent<SkillCaster>();
    }
    
    private void FixedUpdate()
    {
        if (!isCasting)
        {
            Vector2 nextVec= moveInput * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + nextVec);
        }
        
        // Vector2 input = isCasting ? Vector2.zero : moveInput;

    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            
        }
    }

    private void LateUpdate()
    {
        if (!isCasting)
        {
            // 플레이어 방향 설정
            var s = baseScale;
            s.x *= (moveInput.x < 0f) ? +1f : -1f;
            transform.localScale = s;
        }
        
        // 이동 애니메이션 설정 
        anim.SetFloat("isMoving", Mathf.Abs(moveInput.x));
    }

    void OnMove(InputValue value)
    {
        if(!isCasting)
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
        // qTargetSnapshot = FindNearestEnemy();
        
        // 시선 오른쪽
        var s = baseScale;
        s.x *= -1f;
        transform.localScale = s;
        
        // 애니메이션 트리거
        anim.SetTrigger("SkillQ");
        qCastReady = true;
        isCasting = true;
    }
    
    void OnSkillW()
    {
        Debug.Log("OnSkillW");
        
        if (caster == null) return;
        Transform target = FindNearestEnemy(); // 임시 타깃 선정
        caster.TryCast(SkillSlot.W, target);
    }
    
    void OnSkillE()
    {
        Debug.Log("OnSkillE");
     
        anim.SetTrigger("SkillE");
        qCastReady = true;
        isCasting = true;
    }
    
    void OnSkillR()
    {
        Debug.Log("OnSkillR");
        
        anim.SetTrigger("SkillR");
        qCastReady = true;
        isCasting = true;
    }

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
            if (!enemies[i].activeInHierarchy) 
                continue;
            float d2 = (enemies[i].transform.position - from).sqrMagnitude;
            if (d2 < bestSqr) { bestSqr = d2; best = enemies[i].transform; }
        }
        return best;
    }
    
    // 애니메이션 이벤트
    void AE_FireArrow()
    {
        if (caster == null) return;                     // ← SkillCaster 필수
        // var enemyObj = GameObject.FindWithTag("Enemy");
        // if (enemyObj == null) return;

        Transform nearestEnemyTransform = FindNearestEnemy();      // ★ 매번 갱신
        if (nearestEnemyTransform == null) return;
        
        Vector2 start  = (caster.firePos ? (Vector2)caster.firePos.position : (Vector2)transform.position);
        Vector2 target = (Vector2)nearestEnemyTransform.position + new Vector2(enemyAimOffsetX, 0f);

        caster.SpawnArrow(start, target);
    }

    void AE_SkillQ()
    {
        if(!qCastReady) return;
        
        Transform nearestEnemyTransform = FindNearestEnemy();      // ★ 매번 갱신
        caster.TryCast(SkillSlot.Q, nearestEnemyTransform);
        qCastReady = false;
    }
    
    void AE_SkillE()
    {
        if(!qCastReady) return;
        
        caster.TryCast(SkillSlot.E, transform);
        qCastReady = false;
    }    
    
    void AE_SkillR()
    {
        if(!qCastReady) return;
        
        caster.TryCast(SkillSlot.R, transform);
        qCastReady = false;
    }

    void AE_EndSkill()
    {
        Debug.Log("플레이어 스킬 끄으으으으으읕");

        moveInput = Vector2.zero;
        isCasting = false;
    }
    
    // 안전장치: 객체 비활성/상태 전환 시 잔여 플래그 제거
    void OnDisable()
    {
        isCasting = false;
        qCastReady = false;
    }
}
