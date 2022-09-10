using Godot;

public class Interactable:Area2D {
    public bool canInteract;

    public string interactionName;
    public override void _Ready() {
        Connect("body_entered", this, "OnCreatureEntered");
        Connect("body_exited", this, "OnCreatureExited");
    }
    public virtual bool Interact(Player pc) {
        //Do nothing and override in children
        return false;
    }
    public virtual void OnCreatureEntered(Creature other) {
        if(other != null && other.IsInGroup("Players")) {
            ((Player)other).currentInteractables.Add(this);
        }
    }
    public virtual void OnCreatureExited(Creature other) {
        if(other != null && other.IsInGroup("Players")) {
            ((Player)other).currentInteractables.Remove(this);
            canInteract = false;
        }
    }
}
