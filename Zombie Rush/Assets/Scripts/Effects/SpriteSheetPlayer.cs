using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSheetPlayer : MonoBehaviour {
    [Tooltip("Width and Height of the sheet in cells.")]
    public Vector2 size = new Vector2(1,1);
    [Tooltip("Current Tile being rendered.")]
    public float currentTile;
    //Total number of tiles to play
    public byte totalTiles;
    [Tooltip("Tiles per second.")]
    public float speed;
    public Material mat;

    void Start(){
        mat = GetComponent<SpriteRenderer>().material;
        mat.SetFloat("_Width",size.x);
        mat.SetFloat("_Height", size.y);
    }

    void Update() {
        currentTile += Time.deltaTime*speed;
        if(currentTile >= totalTiles){
            currentTile = totalTiles-1;
            AnimationComplete();
        }
        mat.SetFloat("_CurrentTile",currentTile);
    }

    public virtual void AnimationComplete(){

    }
}
