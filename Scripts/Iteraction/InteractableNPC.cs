using Godot;
using System;

public partial class InteractableNPC:Interactable {

    public override bool Interact(Player pc) {


        return (Owner as NPC).Interact(pc);
    }

}
