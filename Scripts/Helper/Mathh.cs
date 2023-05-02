using Godot;
using System;

public static class Mathh {
    ///<summary>
    ///Checks if the inner rect is outside the outer rect, and if so returns the sides of the outer rect that have been passed by the inner as a bit mask.
    ///The order of bits is Top, Bottom, Left, Right </summary>
    public static int Rect2OutOfBoundsSidesMask(Rect2 outer, Rect2 inner) {
        int result = 0;
        if(inner.Position.Y < outer.Position.Y) //Top   0000 0001
            result |= 1;
        if(inner.End.Y > outer.End.Y) //Bottom          0000 0010
            result |= 2;
        if(inner.Position.X < outer.Position.X) //Left  0000 0100
            result |= 4;
        if(inner.End.X > outer.End.X) //Right           0000 1000
            result |= 8;
        return result;
    }
}
