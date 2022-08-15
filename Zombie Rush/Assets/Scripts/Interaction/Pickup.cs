using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : Interactable{
    public Transform payload;   //Whatever it is the player is picking up.
    public Vector2 payloadOffset;
    public bool canDespawn; //can despawn
    public bool storeable; //has to be pickedup with e and can be put in inventory
    //Movement vars for when pickups drop
    public bool moving;
    public Vector3 vel;     //Velocity
    public float startingZVel = 2f;
    public float startingHVel = 0.15f;  //Horizontal velocity. Relative to X-Y Plane
    public float startingHeight = 0.6f;
    public float bounceVelAbsorbtion = 0.6f;
    public float bounceVelMin = -0.05f;
    public float gravity = 9.8f;
    public float height;    //Theoretical Z position that we're simulating
    public float shadowCastMaxHeight;
    public int itemSizeMaxShadowFactor;
    public int itemSize = 1;
    public Animator animator;

    public void Start() {
        animator = GetComponent<Animator>();
        if(transform.childCount > 0)
            payload = transform.GetChild(0);
        if(payload){
            SetPayloadOffset();
        }
    }
    public void FixedUpdate(){
        if(moving){
            vel.z -= gravity*Time.deltaTime;
            height += vel.z*Time.deltaTime;
            Quaternion targetRot = (payload.localRotation.eulerAngles.z < 90 || payload.localRotation.eulerAngles.z > 270) ? Quaternion.identity:Quaternion.Euler(0,0,180);
            payload.localRotation = Quaternion.Lerp(payload.localRotation, targetRot, Time.deltaTime*3);
            if(height <= 0){    //Hits the ground
                if(vel.z < bounceVelMin){   //Still got some bounce left
                    vel.z *= -bounceVelAbsorbtion;
                    height = 0;
                }else{
                    moving = false;
                    vel = new Vector3(0,0,1);
                    height = 0;
                    payload.rotation = targetRot;
                    startingHeight *= 0.25f;
                    payload.localPosition = payloadOffset;
                    return;
                }
            }
            transform.position = new Vector3(transform.position.x+vel.x*Time.deltaTime,transform.position.y+vel.y*Time.deltaTime,0);
            payload.localPosition = new Vector3(0,height,0) + (Vector3)payloadOffset;
        }else{
            float bobDstFactor = height / startingHeight;
            height += vel.z * Time.deltaTime/6;
            if(vel.z < 0 && height < 0){
                vel.z = 1;
            }else if(vel.z > 0 && height > startingHeight){
                vel.z = -1;
            }
            //\left\{x<0.5:\left(\frac{\left(x\cdot2\right)^{2}}{2}\right),1-\left(\frac{\left(\left(1-x\right)\cdot2\right)^{2}}{2}\right)\right\}
            float easeDst;
            if(bobDstFactor < 0.5){
                easeDst = Mathf.Pow(bobDstFactor*2,2)/2;
            }else{
                easeDst = 1-Mathf.Pow((1-bobDstFactor) * 2, 2) / 2;
            }
            payload.localPosition = new Vector3(0,easeDst*startingHeight,0) + (Vector3)payloadOffset;
        }
        float maxShadowSize = (float)itemSize/(float)itemSizeMaxShadowFactor;
        animator.SetFloat("Time", Mathf.Lerp(0,maxShadowSize,1-Mathf.Clamp(height/shadowCastMaxHeight,0,1)));
    }
    public void DropRandomDirection(bool onGround){
        moving = true;
        startingHeight = 0.5f;//onGround ? 0.125f : 0.5f;
        if (!onGround) {
            height = startingHeight;
            Vector3 heightOffset = new Vector3(0, height, 0);
            transform.position -= heightOffset;
        }
        SetPayloadOffset();
        payload.localPosition = payloadOffset;
        float a = Random.Range(0,6.28318530718f);   //6.28318530718 is 2 * pi, which is the number of radians in a circle
        vel = new Vector3(Mathf.Sin(a)*startingHVel,Mathf.Cos(a)*startingHVel/1.41421356f,onGround ? startingZVel * 1.5f : startingZVel );    //1.41421356 is the square root of 2 which makes up for the 45 degree angle perspective
    }

    void SetPayloadOffset(){
        SpriteRenderer payloadSpriteRenderer = payload.GetComponent<SpriteRenderer>();
        payloadOffset = new Vector2(0, payloadSpriteRenderer.sprite.pivot.y / 32);
    }
}
