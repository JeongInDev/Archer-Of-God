using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageInfo
{
    public int amount;
    public Team sourceTeam;
    public string sourceId;
    public Vector2 hitPoint;
    public Object instigator;

    public DamageInfo(int amount, Team team, string id, Vector2 point, Object instigator=null)
    {
        this.amount = amount;
        this.sourceTeam = team;
        this.sourceId = id;
        this.hitPoint = point;
        this.instigator = instigator;
    }
}
