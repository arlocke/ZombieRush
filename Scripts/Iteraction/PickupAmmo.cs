using Godot;

public partial class PickupAmmo:AutoPickup {
    [Export]
    public int amt;
    [Export]
    public AmmoType ammoType;

    public override bool Interact(Player pc) {
        int remainder = pc.AddAmmo(ammoType, amt);
        if(remainder < amt) {
            if(remainder > 0) {
                DropRandomDirection(true, height);
                amt = remainder;
            } else {
                QueueFree();
                return true;
            }
        }
        return false;
    }
}
