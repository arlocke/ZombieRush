using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ClassicClass : Classes
{
    public string GetClass()
    {
        return this.GetType().Name;
    }
    public override int GetHealth()
    {
        return _health;
    }
    public override float GetMovementIncrease()
    {
        return _movementIncrease;
    }
    public override float GetSkillIncrease()
    {
        return _skill;
    }
    public override int GetAimIncrease()
    {
        return _aim;
    }
    public override int GetAttackIncrease()
    {
        return _attack;
    }
    public override int GetArmorIncrease()
    {
        return _armor;
    }
}
