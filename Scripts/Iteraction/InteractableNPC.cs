using Godot;
using System;

public class InteractableNPC : Interactable {
    
    public override bool Interact(Player pc) {


        return (Owner as NPC).Interact(pc);
    }

}
