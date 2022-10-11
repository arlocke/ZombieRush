using Godot;
using System.Collections.Generic;

public class AICoordinator:Node {
    ///<summary>Creatures and the creatures that are chasing them</summary>
    public Dictionary<Creature, List<AICreature>> chasingCreatures = new Dictionary<Creature, List<AICreature>>();
    ///<summary>Creatures being directed by this coordinator.</summary>
    public List<AICreature> directedCreatures = new List<AICreature>();
    [Export]
    public float updateCreaturesTimer;
    [Export]
    public int updateCreaturesIndex;
    public override void _PhysicsProcess(float dt) {
        if(directedCreatures.Count > 0) {
            //Update Creatures
            updateCreaturesTimer -= dt;
            if(++updateCreaturesIndex > 4)
                updateCreaturesIndex = 0;
            if(updateCreaturesTimer < 0) {
                updateCreaturesTimer += 0.1f;   //Updates each creature every .5 seconds.
                for(int i = updateCreaturesIndex; i < directedCreatures.Count; i += 5) {
                    AICreature c = directedCreatures[i];
                    if(IsInstanceValid(c)) {
                        c.UpdateTargets();
                        c.navAgent.SetTargetLocation(c.targetCreature.Position);
                    }
                }
            }
        }
    }
    public void StartChase(AICreature chaser, Creature chased, Creature oldChased = null) {
        EndChase(chaser, oldChased);
        if(IsInstanceValid(chaser) && IsInstanceValid(chased)) {
            List<AICreature> l;
            if(chasingCreatures.TryGetValue(chased, out l)) {   //Check if the chased already exists in the dict
                l.Add(chaser);
            } else {
                l = new List<AICreature> { chaser };
                chasingCreatures.Add(chased, l);
            }
            chaser.Chase(chased);
        }
    }
    public void EndChase(AICreature chaser, Creature chased) {
        if(IsInstanceValid(chased)) {
            List<AICreature> l;
            if(chasingCreatures.TryGetValue(chased, out l)) {
                l.Remove(chaser);
                if(l.Count == 0)    //If there aren't any creatures chasing this creature, we don't have to worry about it anymore. Remove.
                    chasingCreatures.Remove(chased);
            }
        }
    }
}
