using Godot;
using System;

public partial class NPC:Character {
    //Dialogue Variables
    [Export]
    public Resource dialogueResource;

    public override void _Ready() {
        base._Ready();
    }

    public bool Interact(Player pc) {
        GetNode<Control>("DialogueHandler").Call("initDialogue", "succy_talk", dialogueResource);
        return true;
    }
}
