using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillSlot { Q, W, E, R }

public class SkillCaster : MonoBehaviour
{
    [Header("Common")]
    public Team team = Team.Player;
    public Transform firePos;         // �߻� ��ġ(������ ���� Transform)
    public GameObject arrowPrefab;    // ȭ�� ������

    [Header("Slots")]
    public SkillData skillQ, skillW, skillE, skillR;

    // ��ٿ�(remaining time)
    float cooldownQ;
    float cooldownW;
    float cooldownE;
    float cooldownR;
    
    void Update()
    {
        float dt = Time.deltaTime;
        if (cooldownQ > 0f) cooldownQ -= dt;
        if (cooldownW > 0f) cooldownW -= dt;
        if (cooldownE > 0f) cooldownE -= dt;
        if (cooldownR > 0f) cooldownR -= dt;
    }

    public bool TryCast(SkillSlot slot, Transform target = null)
    {
        SkillData data = GetData(slot);
        if (data == null) return false;

        float remain = GetRemainingCooldown(slot);
        if (remain > 0f) return false;

        SkillContext ctx = new SkillContext();
        ctx.caster   = this;
        ctx.team     = team;
        ctx.firePos  = (firePos != null) ? firePos : transform;
        ctx.target   = target;
        ctx.targetPos= (target != null) ? (Vector2)target.position : Vector2.zero;

        bool ok = data.Execute(ctx);
        if (ok)
        {
            SetCooldown(slot, data.cooldown);
            return true;
        }
        return false;
    }
    
    // ��ų �������� ȭ�� �� �� ȣ���ϴ� ����
    public void SpawnArrow(Vector2 start, Vector2 target,
        float speedMul = 1f, float scaleMul = 1f, int damageAdd = 0)
    {
        if (arrowPrefab == null) return;

        GameObject go = Instantiate(arrowPrefab, start, Quaternion.identity);

        if (Mathf.Abs(scaleMul - 1f) > 0.001f)
            go.transform.localScale *= scaleMul;

        Arrow ar = go.GetComponent<Arrow>();
        if (ar != null)
        {
            if (Mathf.Abs(speedMul - 1f) > 0.001f) ar.MulTravelSpeed(speedMul);
            if (damageAdd != 0) ar.SetDamageAdd(damageAdd);
            ar.Launch(start, target);
        }
    }

    // ---- ���� �޼���� (���� �Ϲ� switch) ----
    SkillData GetData(SkillSlot slot)
    {
        switch (slot)
        {
            case SkillSlot.Q: return skillQ;
            case SkillSlot.W: return skillW;
            case SkillSlot.E: return skillE;
            case SkillSlot.R: return skillR;
            default: return null;
        }
    }

    float GetRemainingCooldown(SkillSlot slot)
    {
        switch (slot)
        {
            case SkillSlot.Q: return cooldownQ;
            case SkillSlot.W: return cooldownW;
            case SkillSlot.E: return cooldownE;
            case SkillSlot.R: return cooldownR;
            default: return 0f;
        }
    }
    
    void SetCooldown(SkillSlot slot, float value)
    {
        switch (slot)
        {
            case SkillSlot.Q: cooldownQ = value; break;
            case SkillSlot.W: cooldownW = value; break;
            case SkillSlot.E: cooldownE = value; break;
            case SkillSlot.R: cooldownR = value; break;
        }
    }
}
