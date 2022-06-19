using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract class Classes
{
    protected int _health = 1; //True Health
    protected int _armor = 0; //Temp Health
    protected int _aim = 0; //Ranged Damage
    protected int _attack = 0; //Melee Damage

    protected float _skill = 0; //Skill Damage
    protected float _movementIncrease = 1; //Movement Changes
    public abstract int GetHealth();
    public abstract int GetAttackIncrease();
    public abstract int GetAimIncrease();
    public abstract int GetArmorIncrease();
    public abstract float GetMovementIncrease();
    public abstract float GetSkillIncrease();
}
