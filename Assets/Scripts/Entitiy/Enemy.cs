using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ----- 이동/경계 -----
    [Header("Movement")] [SerializeField] float moveSpeed = 3f;
    [SerializeField] Transform leftBound; // 플랫폼 왼쪽 끝
    [SerializeField] Transform rightBound; // 플랫폼 오른쪽 끝
    [SerializeField] float arriveEpsilon = 0.3f; // 도착 판정 오차

    // ----- 공격 타임 설정 -----
    [Header("Attack Time")] [SerializeField]
    float basicTimeMin = 1f; // 일반공격 타임 최소

    [SerializeField] float basicTimeMax = 4f; // 일반공격 타임 최대
    [SerializeField] float skillChance = 0.35f; // 공격타임 시작 시 스킬 선택 확률(0~1)
    [SerializeField] bool randomIncludeQ = true; // 랜덤 스킬 후보에 Q 포함 여부


    // 내부 상태
    enum State
    {
        Move,
        AttackBasic,
        AttackSkill
    }

    State state = State.Move;

    private SkillCaster caster;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private Vector3 baseScale;

    private float minX, maxX;
    private bool hasBounds;
    private float targetX; // 이동 목표 X
    private float attackEndTime; // AttackBasic 종료 시간
    private bool isAttacking; // Attack 애니 중(이벤트로 on/off)

    void Awake()
    {
        caster = GetComponent<SkillCaster>();
        rb = GetComponent<Rigidbody2D>(); // 없어도 OK
        anim = GetComponent<Animator>();
        baseScale = transform.localScale;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerTransform = p.transform;
        if (caster) caster.team = Team.Enemy;

        if (leftBound && rightBound)
        {
            minX = Mathf.Min(leftBound.position.x, rightBound.position.x);
            maxX = Mathf.Max(leftBound.position.x, rightBound.position.x);
            hasBounds = true;
        }

        PickNewTargetX();
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        switch (state)
        {
            case State.Move:
                TickMove();
                break;

            case State.AttackBasic:
                TickAttackBasic();
                break;

            case State.AttackSkill:
                TickAttackSkill();
                break;
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 플레이어 방향 바라보기 (기본 스프라이트가 '왼쪽' 가정)
        var s = baseScale;
        s.x = (playerTransform.position.x < transform.position.x) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    // ====== 상태 처리 ======
    void TickMove()
    {
        // 목표까지 이동
        float dir = Mathf.Sign(targetX - transform.position.x);
        if (Mathf.Abs(targetX - transform.position.x) <= arriveEpsilon) 
            dir = 0f;

        SetMove(dir);

        // 도착하면 공격타임 시작
        if (dir == 0f)
        {
            float r = Random.value;
            if (r < skillChance)
            {
                state = State.AttackSkill;
            }
            else
            {
                state = State.AttackBasic;
                float dur = Random.Range(basicTimeMin, basicTimeMax);
                attackEndTime = Time.time + dur;
            }
        }
    }

    // 기본 공격
    void TickAttackBasic()
    {
        // 이동 금지
        SetMove(0f);

        // 쿨 가능할 때마다 기본공격 애니 시작 → 이벤트에서 발사
        if (!isAttacking && caster != null && caster.CanCast(SkillSlot.Q))
        {
            if (anim) 
                anim.SetTrigger("Attack");
            isAttacking = true;
        }

        // 시간 끝나면 이동으로 전환
        if (Time.time >= attackEndTime)
        {
            state = State.Move;
            PickNewTargetX();
        }
    }

    void TickAttackSkill()
    {
        // 이동 금지
        SetMove(0f);

        // 가능한 스킬 하나 즉시 시전
        TryCastRandomSkillOnce();

        // 바로 이동 상태로
        state = State.Move;
        PickNewTargetX();
    }

    // ====== 이동/유틸 ======
    void SetMove(float dirX)
    {
        // 경계 밖으로 못 나가게 Clamp
        float nextX = transform.position.x + dirX * moveSpeed * Time.fixedDeltaTime;
        if (hasBounds)
            nextX = Mathf.Clamp(nextX, minX, maxX);

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            rb.MovePosition(new Vector2(nextX, rb.position.y));
        else
            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

        if (anim)
            anim.SetFloat("isMoving", Mathf.Abs(dirX));
    }

    void PickNewTargetX()
    {
        if (hasBounds)
            targetX = Random.Range(minX, maxX);
        else
            targetX = transform.position.x; // 경계가 없으면 제자리(안전)
    }

    void TryCastRandomSkillOnce()
    {
        if (caster == null) return;

        // 후보 수집
        SkillSlot candidatesBuffer4 = SkillSlot.Q; // 더미 초기화
        SkillSlot[] pool = new SkillSlot[4];
        int count = 0;

        if (randomIncludeQ && caster.skillQ != null && caster.CanCast(SkillSlot.Q))
            pool[count++] = SkillSlot.Q;
        if (caster.skillW != null && caster.CanCast(SkillSlot.W))
            pool[count++] = SkillSlot.W;
        if (caster.skillE != null && caster.CanCast(SkillSlot.E))
            pool[count++] = SkillSlot.E;
        if (caster.skillR != null && caster.CanCast(SkillSlot.R))
            pool[count++] = SkillSlot.R;

        if (count == 0) return;

        int idx = Random.Range(0, count);
        SkillSlot slot = pool[idx];

        // 대부분의 스킬은 타깃 필요 → 플레이어 전달
        caster.TryCast(slot, playerTransform);
    }

    // ====== 애니메이션 이벤트 ======
    // 기본공격 발사 프레임에서 호출
    public void AE_Enemy_Shoot()
    {
        
        
        
        // if (caster == null || playerTransform == null) return;
        // caster.TryCast(SkillSlot.Q, playerTransform); // Q 슬롯을 '적 기본사격' SO로 세팅해두자
    }

    // 공격 애니 끝 프레임에서 호출
    public void AE_Enemy_EndAttack()
    {
        isAttacking = false;
    }
}