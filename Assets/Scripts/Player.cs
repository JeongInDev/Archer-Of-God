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

    Rigidbody2D rb;
    Vector3 baseScale;
    [SerializeField] Animator anim;
    
    // �߻� ����
    [Header("Shooting")]
    // [SerializeField] GameObject arrowPrefab;
    // [SerializeField] Transform firePos;
    [SerializeField] float enemyAimOffsetX = 0.5f;
    
    [Header("Skills")]
    [SerializeField] SkillCaster caster;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        anim = GetComponent<Animator>();
    }
    
    private void FixedUpdate()
    {
        Vector2 nextVec= moveInput * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + nextVec);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            AE_FireArrow();
        }
    }

    private void LateUpdate()
    {
        // �÷��̾� ���� ����
        var s = baseScale;
        s.x *= (moveInput.x < 0f) ? +1f : -1f;
        transform.localScale = s;
        
        // �̵� �ִϸ��̼� ���� 
        anim.SetFloat("isMoving", Mathf.Abs(moveInput.x));
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnSkillQ()
    {
        Debug.Log("OnSkillQ");
        
        if (caster == null) return;
        Transform target = FindNearestEnemy(); // �ӽ� Ÿ�� ����
        caster.TryCast(SkillSlot.Q, target);
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
        var enemyObj = GameObject.FindWithTag("Enemy");
        if (enemyObj == null) return;

        Vector2 start  = (caster.firePos ? (Vector2)caster.firePos.position : (Vector2)transform.position);
        Vector2 target = (Vector2)enemyObj.transform.position + new Vector2(enemyAimOffsetX, 0f);

        // �� ���� �� �ٷ� �߻� (���߿� W ���� �� �ڵ� �ݿ���)
        caster.SpawnArrow(start, target);
        
        // if (!arrowPrefab || !firePos) return;
        // var enemyObj = GameObject.FindWithTag("Enemy");
        // if (!enemyObj) return;
        //
        // Vector2 start  = firePos.position;
        // Vector2 target = enemyObj.transform.position;
        // target += Vector2.right * enemyAimOffsetX;
        //
        // var go = Instantiate(arrowPrefab, start, Quaternion.identity);
        // var arrow = go.GetComponent<Arrow>();
        // if (arrow) arrow.Launch(start, target);
    }
}
