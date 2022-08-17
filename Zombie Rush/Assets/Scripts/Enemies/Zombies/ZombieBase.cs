using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BehaviorState {
    Idle,
    Wandering,
    Chasing,
    Searching,
}

public class ZombieBase : CreatureBase
{
    public NavMeshAgent agent;
    public CreatureBase targetCreature;
    public NavMeshPath pathToTarget;

    public float aggroRadius = 3; //
    public bool aggro; //not used

    public GameObject aggroCollider;

    public BehaviorState state;

    // Start is called before the first frame update
    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = moveSpeed;

        pathToTarget = new NavMeshPath();

        targetCreature = GameObject.FindGameObjectWithTag("Player").GetComponent<CreatureBase>();
    }

    public void FixedUpdate() {
        switch(state) {
            case BehaviorState.Idle:
                //standing still jacking off, mashing head against wall etc...
            case BehaviorState.Wandering:
                //walking around n shit
                CheckForEnemy();
                break;
            case BehaviorState.Chasing: 
                agent.SetDestination(targetCreature.transform.position);
                break;  
        }

    }

    public bool CheckForEnemy() {
        ContactFilter2D cf = new ContactFilter2D();
        cf.SetLayerMask(LayerMask.GetMask("Creatures"));
        Collider2D[] collisionResults = new Collider2D[32];
        Physics2D.OverlapCircle(transform.position, aggroRadius, cf, collisionResults);
        foreach (Collider2D c in collisionResults) {
            if (c) {
                CreatureBase creature = c.GetComponent<CreatureBase>();
                if (creature && (creature.teamType != teamType || creature.teamFaction != teamFaction)) {
                    targetCreature = creature;
                    state = BehaviorState.Chasing;
                    return true;
                }
            }
        }
        return false;
    }
}
