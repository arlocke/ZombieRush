using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSheetPlayerOneShot : SpriteSheetPlayer{
    public override void AnimationComplete() {
        Destroy(gameObject);
    }
}
