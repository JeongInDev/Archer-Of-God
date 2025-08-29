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
    
    // �߻� ����
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
            // �÷��̾� ���� ����
            var s = baseScale;
            s.x *= (moveInput.x < 0f) ? +1f : -1f;
            transform.localScale = s;
        }
        
        // �̵� �ִϸ��̼� ���� 
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
            Debug.Log("Q - ��ų ��� �Ұ�");
            return;
        }
        Debug.Log("Q - ��ų ���");
        // qTargetSnapshot = FindNearestEnemy();
        
        // �ü� ������
        var s = baseScale;
        s.x *= -1f;
        transform.localScale = s;
        
        // �ִϸ��̼� Ʈ����
        anim.SetTrigger("SkillQ");
        qCastReady = true; // �̺�Ʈ������ ���� �߻��ϵ��� '����'
        isCasting = true;
    }
    
    void OnSkillW()
    {
        Debug.Log("OnSkillW");
        
        if (caster == null) return;
        Transform target = FindNearestEnemy(); // �ӽ� Ÿ�� ����
        caster.TryCast(SkillSlot.W, target);
    }
    
    void OnSkillE()
    {
        Debug.Log("OnSkillE");
        
        if (caster == null) return;
        caster.TryCast(SkillSlot.E);
    }
    
    void OnSkillR()
    {
        Debug.Log("OnSkillR");
        
        if (caster == null) return;
        Transform target = FindNearestEnemy(); // �ӽ� Ÿ�� ����
        caster.TryCast(SkillSlot.R, target);
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
    
    // �ִϸ��̼� �̺�Ʈ
    void AE_FireArrow()
    {
        if (caster == null) return;                     // �� SkillCaster �ʼ�
        // var enemyObj = GameObject.FindWithTag("Enemy");
        // if (enemyObj == null) return;

        Transform nearestEnemyTransform = FindNearestEnemy();      // �� �Ź� ����
        if (nearestEnemyTransform == null) return;
        
        Vector2 start  = (caster.firePos ? (Vector2)caster.firePos.position : (Vector2)transform.position);
        Vector2 target = (Vector2)nearestEnemyTransform.position + new Vector2(enemyAimOffsetX, 0f);

        caster.SpawnArrow(start, target);
    }

    void AE_SkillQ()
    {
        if(!qCastReady) return;
        
        Transform nearestEnemyTransform = FindNearestEnemy();      // �� �Ź� ����
        caster.TryCast(SkillSlot.Q, nearestEnemyTransform);
        qCastReady = false;
    }

    void AE_EndSkillQ()
    {
        Debug.Log("��ų Q ĳ���� ��������������");

        moveInput = Vector2.zero;
        isCasting = false;
    }
    
    // ������ġ: ��ü ��Ȱ��/���� ��ȯ �� �ܿ� �÷��� ����
    void OnDisable()
    {
        isCasting = false;
        qCastReady = false;
    }
}
