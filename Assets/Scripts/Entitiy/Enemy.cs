using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ----- �̵�/��� -----
    [Header("Movement")] [SerializeField] float moveSpeed = 3f;
    [SerializeField] Transform leftBound; // �÷��� ���� ��
    [SerializeField] Transform rightBound; // �÷��� ������ ��
    [SerializeField] float arriveEpsilon = 0.3f; // ���� ���� ����

    // ----- ���� Ÿ�� ���� -----
    [Header("Attack Time")] [SerializeField]
    float basicTimeMin = 1f; // �Ϲݰ��� Ÿ�� �ּ�

    [SerializeField] float basicTimeMax = 4f; // �Ϲݰ��� Ÿ�� �ִ�
    [SerializeField] float skillChance = 0.35f; // ����Ÿ�� ���� �� ��ų ���� Ȯ��(0~1)
    [SerializeField] bool randomIncludeQ = true; // ���� ��ų �ĺ��� Q ���� ����


    // ���� ����
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
    private float targetX; // �̵� ��ǥ X
    private float attackEndTime; // AttackBasic ���� �ð�
    private bool isAttacking; // Attack �ִ� ��(�̺�Ʈ�� on/off)

    void Awake()
    {
        caster = GetComponent<SkillCaster>();
        rb = GetComponent<Rigidbody2D>(); // ��� OK
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

        // �÷��̾� ���� �ٶ󺸱� (�⺻ ��������Ʈ�� '����' ����)
        var s = baseScale;
        s.x = (playerTransform.position.x < transform.position.x) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    // ====== ���� ó�� ======
    void TickMove()
    {
        // ��ǥ���� �̵�
        float dir = Mathf.Sign(targetX - transform.position.x);
        if (Mathf.Abs(targetX - transform.position.x) <= arriveEpsilon) 
            dir = 0f;

        SetMove(dir);

        // �����ϸ� ����Ÿ�� ����
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

    // �⺻ ����
    void TickAttackBasic()
    {
        // �̵� ����
        SetMove(0f);

        // �� ������ ������ �⺻���� �ִ� ���� �� �̺�Ʈ���� �߻�
        if (!isAttacking && caster != null && caster.CanCast(SkillSlot.Q))
        {
            if (anim) 
                anim.SetTrigger("Attack");
            isAttacking = true;
        }

        // �ð� ������ �̵����� ��ȯ
        if (Time.time >= attackEndTime)
        {
            state = State.Move;
            PickNewTargetX();
        }
    }

    void TickAttackSkill()
    {
        // �̵� ����
        SetMove(0f);

        // ������ ��ų �ϳ� ��� ����
        TryCastRandomSkillOnce();

        // �ٷ� �̵� ���·�
        state = State.Move;
        PickNewTargetX();
    }

    // ====== �̵�/��ƿ ======
    void SetMove(float dirX)
    {
        // ��� ������ �� ������ Clamp
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
            targetX = transform.position.x; // ��谡 ������ ���ڸ�(����)
    }

    void TryCastRandomSkillOnce()
    {
        if (caster == null) return;

        // �ĺ� ����
        SkillSlot candidatesBuffer4 = SkillSlot.Q; // ���� �ʱ�ȭ
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

        // ��κ��� ��ų�� Ÿ�� �ʿ� �� �÷��̾� ����
        caster.TryCast(slot, playerTransform);
    }

    // ====== �ִϸ��̼� �̺�Ʈ ======
    // �⺻���� �߻� �����ӿ��� ȣ��
    public void AE_Enemy_Shoot()
    {
        
        
        
        // if (caster == null || playerTransform == null) return;
        // caster.TryCast(SkillSlot.Q, playerTransform); // Q ������ '�� �⺻���' SO�� �����ص���
    }

    // ���� �ִ� �� �����ӿ��� ȣ��
    public void AE_Enemy_EndAttack()
    {
        isAttacking = false;
    }
}