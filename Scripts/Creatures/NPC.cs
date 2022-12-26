using Godot;
using System;

public class NPC : Character {
    //Dialogue Variables
    [Export]
    public Resource dialogueResource;

    public override void _Ready() {
        base._Ready();
    }

    public bool Interact(Player pc) {
        GetNode<CanvasLayer>("DialogueHandler").Call("initDialogue", dialogueResource);
        return true;
    }
}
