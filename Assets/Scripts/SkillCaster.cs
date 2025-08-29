using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillSlot { Q, W, E, R }

public class SkillCaster : MonoBehaviour
{
    [Header("Common")]
    public Team team = Team.Player;
    public Transform firePos;         // 발사 위치(없으면 본인 Transform)
    public GameObject arrowPrefab;    // 화살 프리팹

    [Header("Slots")]
    public SkillData skillQ;
    public SkillData skillW;
    public SkillData skillE;
    public SkillData skillR;

    // 쿨다운(remaining time)
    private float cooldownQ;
    private float cooldownW;
    private float cooldownE;
    private float cooldownR;
    
    // ── W 버프 상태 ──
    private bool wBuffActive = false;
    private float wBuffEndTime = 0;
    private float wSpeedMulCache = 1f;
    private float wScaleMulCache = 1f;
    private int   wDamageAddCache = 0;
    
    void Update()
    {
        float dt = Time.deltaTime;
        if (cooldownQ > 0f) cooldownQ -= dt;
        if (cooldownW > 0f) cooldownW -= dt;
        if (cooldownE > 0f) cooldownE -= dt;
        if (cooldownR > 0f) cooldownR -= dt;
        
        // 버프 만료 처리
        if (wBuffActive && Time.time > wBuffEndTime)
        {
            wBuffActive = false;
        }
    }

    public bool TryCast(SkillSlot slot, Transform target = null)
    {
        SkillData data = GetData(slot);
        if (data == null) return false;

        if (GetRemainingCooldown(slot) > 0f)
            return false;

        SkillContext ctx = new SkillContext();
        ctx.caster    = this;
        ctx.team      = team;
        ctx.firePos   = (firePos != null) ? firePos : transform;
        ctx.target    = target;
        ctx.targetPos = (target != null) ? (Vector2)target.position : Vector2.zero;

        bool ok = data.Execute(ctx);
        if (ok)
        {
            SetCooldown(slot, data.cooldown);
            return true;
        }
        return false;
    }
    
    // 스킬 구현에서 화살 쏠 때 호출하는 헬퍼
    public void SpawnArrow(Vector2 start, Vector2 target,
        float speedMul = 1f, float scaleMul = 1f, int damageAdd = 0)
    {
        // W 버프 활성 시 병합
        if (wBuffActive && Time.time <= wBuffEndTime)
        {
            speedMul *= wSpeedMulCache;
            scaleMul *= wScaleMulCache;
            damageAdd += wDamageAddCache;
        }
        else
        {
            wBuffActive = false;
        }
        
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
    
    // W 버프 적용(지속시간/배수/보정치)
    public void ApplyWBuff(float duration, float speedMul, float scaleMul, int damageAdd)
    {
        wBuffActive      = true;
        wBuffEndTime     = Time.time + duration;
        wSpeedMulCache   = speedMul;
        wScaleMulCache   = scaleMul;
        wDamageAddCache  = damageAdd;
    }

    // ---- 보조 메서드들 (전부 일반 switch) ----
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
