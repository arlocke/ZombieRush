using Godot;
public class PickupGun:Pickup {

    public override void _Ready() {
        base._Ready();
        if(payload != null) {
            itemSize = (payload as Gun).itemSize;
            interactionName = (payload as Gun).itemName;
        }
    }
    public override bool Interact(Player pc) {
        pc.EquipGun(payload as Gun);
        payload = null;
        QueueFree();
        return true;
    }
}
