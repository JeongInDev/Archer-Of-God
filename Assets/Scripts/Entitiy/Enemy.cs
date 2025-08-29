using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ----- 이동/경계 -----
    [Header("Movement")] [SerializeField] float moveSpeed = 3f;
    [SerializeField] float arriveEpsilon = 0.3f; // 도착 판정 오차
    [SerializeField] Transform leftBound; // 플랫폼 왼쪽 끝
    [SerializeField] Transform rightBound; // 플랫폼 오른쪽 끝

    // ── 정지(공격) 시간 ──
    [Header("Stop (Attack) Time")]
    [SerializeField] float stopTimeMin = 1f;
    [SerializeField] float stopTimeMax = 4f;

    // ── 스킬 사용 ──
    [Header("Skills During Stop")]
    [SerializeField] float skillIntervalSeconds = 4f; // 4초마다 시도
    [SerializeField] bool includeQInRandom = true;    // Q도 후보에 포함할지
    [SerializeField] bool includeWInRandom = true;    // Q도 후보에 포함할지
    [SerializeField] bool includeEInRandom = true;    // Q도 후보에 포함할지
    [SerializeField] bool includeRInRandom = true;    // Q도 후보에 포함할지
    
    [Header("Shooting")]
    [SerializeField] private float playerAimOffsetX = -0.5f;
    
    // 내부 상태
    enum State { Move, Stop }
    State state = State.Move;

    private SkillCaster caster;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private Vector3 baseScale;

    private float minX, maxX;
    private bool hasBounds;
    private float targetX;              // 이동 목표 X
    private float stopEndTime;        // 정지 종료 시각
    private float nextSkillTime;      // 다음 스킬 시도 시각

    // ── 스킬 캐스팅 상태(추가) ──
    bool isCasting = false;        // 스킬 시전 중이면 이동/평타/정지 종료 모두 멈춤
    SkillSlot queuedSkill;              // 이번에 쓸 스킬 저장
    
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
        nextSkillTime = Time.time + skillIntervalSeconds;
    }

    void FixedUpdate()
    {
        if (playerTransform == null) 
            return;

        if (state == State.Move) 
            TickMove();
        else                     
            TickStop();
    }

    void LateUpdate()
    {
        if (playerTransform == null) 
            return;

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

        // 도착 → 정지(공격) 타임 진입
        if (dir == 0f)
        {
            state = State.Stop;
            float stopDur = Random.Range(stopTimeMin, stopTimeMax);
            stopEndTime = Time.time + stopDur;
            // 스킬은 전역 타이머(nextSkillTime)가 도달했을 때만 정지 중에 시전
        }
    }
    
    void TickStop()
    {
        // 멈춰 서 있기(애니메이터가 isMoving=0이면 자동으로 공격 애니가 돌도록 구성)
        SetMove(0f);

        // ★ 스킬 캐스팅 중이면 아무 것도 하지 않음(정지 타임도 연장)
        if (isCasting)
            return;
        
        // 정지 중에만 스킬 시도: 4초 간격
        if (Time.time >= nextSkillTime)
        {
            TryCastRandomSkillOnce();                // 가능한 스킬 1개 시도
            nextSkillTime = Time.time + skillIntervalSeconds; // 다음 시도 예약
        }

        // 정지 시간 종료 → 다음 목표로 이동
        if (Time.time >= stopEndTime)
        {
            state = State.Move;
            PickNewTargetX();
        }
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
        if (!caster || !playerTransform) return;

        // Q/W/E/R 중 '쿨다운 가능한' 후보만 모으기
        SkillSlot[] pool = new SkillSlot[4];
        int count = 0;

        if (includeQInRandom && caster.skillQ && caster.CanCast(SkillSlot.Q)) 
            pool[count++] = SkillSlot.Q;
        if (includeWInRandom && caster.skillW && caster.CanCast(SkillSlot.W)) 
            pool[count++] = SkillSlot.W;
        if (includeEInRandom && caster.skillE && caster.CanCast(SkillSlot.E)) 
            pool[count++] = SkillSlot.E;
        if (includeRInRandom && caster.skillR && caster.CanCast(SkillSlot.R)) 
            pool[count++] = SkillSlot.R;
        if (count == 0) return;     // 사용 가능 스킬 없으면 패스

        var slot = pool[Random.Range(0, count)];
        Debug.Log("몬스터 스킬 실행");
        
        if (slot == SkillSlot.W)
        {
            // 타깃 필요 없으면 null로 둬도 됨. (지금 구조는 playerTransform 전달해도 무해)
            caster.TryCast(SkillSlot.W, playerTransform);

            // 즉시 이동 복귀 + 다음 스킬 타이머 갱신
            state = State.Move;
            PickNewTargetX();
            nextSkillTime = Time.time + skillIntervalSeconds;
            return;
        }
        
        queuedSkill = slot;
        isCasting = true;
        
        if (anim)
        {
            switch (queuedSkill)
            {
                case SkillSlot.Q: 
                    anim.SetTrigger("SkillQ"); break;
                case SkillSlot.E: 
                    anim.SetTrigger("SkillE"); break;
                case SkillSlot.R: 
                    anim.SetTrigger("SkillR"); break;
            }
        }
    }

    // ====== 애니메이션 이벤트 ======
    public void AE_Enemy_Shoot()
    {
        if (!caster || !playerTransform) return;
        Vector2 start  = (caster.firePos != null) ? (Vector2)caster.firePos.position : (Vector2)transform.position;
        Vector2 target = (Vector2)playerTransform.position + new Vector2(playerAimOffsetX, 0f);
        caster.SpawnArrow(start, target);
    }
    
    public void AE_Enemy_SkillQ()
    {
        if (!caster) return;

        Debug.Log("몬스터 스킬 Q");

        caster.TryCast(SkillSlot.Q, playerTransform);
    }
    
    public void AE_Enemy_SkillE()
    {
        if (!caster) return;

        Debug.Log("몬스터 스킬 E");

        caster.TryCast(SkillSlot.E, playerTransform);
    }
    
    public void AE_Enemy_SkillR()
    {
        if (!caster) return;

        Debug.Log("몬스터 스킬 R");

        caster.TryCast(SkillSlot.R, playerTransform);
    }
    
    // 스킬/공격 애니 끝(스킬 애니의 마지막 프레임 이벤트)
    public void AE_Enemy_EndSkill()
    {
        Debug.Log("몬스터 스킬 끝");

        isCasting = false;
        
        state = State.Move;
        PickNewTargetX();
        
        nextSkillTime = Time.time + skillIntervalSeconds;
    }
}