using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class EnemyRegistry
{
    static readonly HashSet<Enemy> _alive = new HashSet<Enemy>();
    public static event Action OnAllEnemiesDead;
    
    public static void Register(Enemy e)   { if (e != null) _alive.Add(e); }
    public static void Unregister(Enemy e) { if (e != null) _alive.Remove(e); }

    public static int AliveCount => _alive.Count;

    public static void TriggerVictoryAll()
    {
        foreach (var e in _alive)
            e.PlayVictory();
    }
}
