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
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform firePos;
    [SerializeField] float enemyAimOffsetX = 0.5f;
    
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


    // �ִϸ��̼� �̺�Ʈ
    void AE_FireArrow()
    {
        if (!arrowPrefab || !firePos) return;
        var enemyObj = GameObject.FindWithTag("Enemy");
        if (!enemyObj) return;

        Vector2 start  = firePos.position;
        Vector2 target = enemyObj.transform.position;
        target += Vector2.right * enemyAimOffsetX;
        
        var go = Instantiate(arrowPrefab, start, Quaternion.identity);
        var arrow = go.GetComponent<Arrow>();
        if (arrow) arrow.Launch(start, target);
    }
}
