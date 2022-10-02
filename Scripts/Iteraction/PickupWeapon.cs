using Godot;
public class PickupWeapon:Pickup {

    public override void _Ready() {
        base._Ready();
        if(payload != null) {
            itemSize = (payload as Weapon).itemSize;
            interactionName = (payload as Weapon).itemName;
        }
    }
    public override bool Interact(Player pc) {
        pc.PickUpItem(payload as Weapon);
        payload = null;
        QueueFree();
        return true;
    }
}
