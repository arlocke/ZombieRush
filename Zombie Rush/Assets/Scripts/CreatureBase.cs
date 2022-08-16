using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamType {
    Zombies, //will have integer variable which will be team number, theoretically infinite team numbers per team type
    Humans,
}

public class CreatureBase : MonoBehaviour
{
    //Stats
    public float hp;
    public float moveSpeed;
    public float meleeAttack; //base melee damage when unequipped, or added to melee weapons 
    public float rangedAttack; //base ranged damage added to ranged weapons

    //Team data
    public TeamType teamType;
    public int teamFaction; //faction inside of teamType, potentially refactor to string

    //Assets 
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Animator animator;

    public void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Projectile") ) {
            ProjectileBase proj = other.gameObject.GetComponent<ProjectileBase>();
            if(proj.owner != this) {
                float startingHp = hp; 
                TakeDamage(proj.damage); //Need to add team dynamic shit
                proj.RemoveDamage(startingHp);
            }
        }
    }
    public void TakeDamage(float takenDamage) {
        hp -= takenDamage;
        if(hp <= 0) {
            //play death anim, destroy gameobject, spawn corpse sprite
            Destroy(gameObject);
        }
    }
}
