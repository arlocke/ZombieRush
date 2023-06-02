using Godot;

public partial class Interactable:Area2D {
    public bool canInteract;

    public string interactionName;
    public override void _Ready() {
        Connect("body_entered", new Callable(this, "OnCreatureEntered"));
        Connect("body_exited", new Callable(this, "OnCreatureExited"));
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
