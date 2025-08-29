using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    Team Team { get; }
    bool IsAlive { get; }
    void ApplyDamage(DamageInfo info);
}
