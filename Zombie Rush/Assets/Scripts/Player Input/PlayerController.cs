using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerController : CreatureBase {
    //Interaction Data
    public List<Interactable> currentInteractables;
    public Interactable closestInteractable;
    public Text interactableText;

    //Animation Data
    public bool holding = false; //all this affects is idle animation

    //Movement variables
    //public float moveSpeed = 1f;
    public float collisionOffset = 0.05f; // Distance from rigidbody to check for collisions
    public ContactFilter2D movementFilter;

    //Children
    public GameObject arm;
    public Transform hand1Point;
    public GunBase heldGun;

    //Inventory
    public Dictionary<AmmoType, int> heldAmmo;
    public Dictionary<AmmoType, int> maxAmmo;
    public List<int> heldAmmoList;

    PlayerInputActions controls;

    Vector2 movementInput;
    //public SpriteRenderer spriteRenderer;
    //public Rigidbody2D rb;
    //Animator animator;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    //Prefabs
    public GameObject pickupGunRef;

    private void Awake() {
        controls = new PlayerInputActions();
    }

    // Start is called before the first frame update
    void Start() {
        heldAmmo = new Dictionary<AmmoType, int>() {
            { AmmoType.Cal50ae, 10 },
            { AmmoType.Cal762, 100 },
            { AmmoType.Cal9mm, 30 },
        };
        maxAmmo = new Dictionary<AmmoType, int>() {
            { AmmoType.Cal50ae, 50 },
            { AmmoType.Cal762, 100 },
            { AmmoType.Cal9mm, 150 },
        };

        heldAmmoList = new List<int>(heldAmmo.Values);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        arm = transform.Find("Arm").gameObject;

        
        //holding = heldGun;

        //controls.Player.Look.performed += _ => Look();
    }

    private void OnEnable() {
        controls.Enable();

        controls.Player.Move.performed += OnMoveInput;
        controls.Player.Move.canceled += OnMoveInput;

        controls.Player.Look.performed += OnLookInput;
        //controls.Player.Look.canceled += OnLookInput;

        controls.Player.Shoot.performed += OnPullTrigger;
        controls.Player.Shoot.canceled += OnReleaseTrigger;

        controls.Player.Interact.performed += OnInteractInput;

        controls.Player.Reload.performed += OnReloadInput;
    }

    private void OnDisable() {
        controls.Disable();
    }

    private void FixedUpdate() {
        if (movementInput != Vector2.zero) {
            bool success = TryToMove(movementInput);

            if (!success) {
                // Attempts to "slide" when colliding in the X direction
                if(movementInput.x != 0) {
                    success = TryToMove(new Vector2(movementInput.x, 0));
                }
                

                if (!success && movementInput.y != 0) {
                    // Attempts to "slide" when colliding in the Y direction
                    success = TryToMove(new Vector2(0, movementInput.y));
                }
            }

            animator.SetBool("isMoving", success);

            //Changing direction of walk anim when unequipped
            if (!holding && movementInput.x > 0) {
                spriteRenderer.flipX = false;
            }
            if (!holding && movementInput.x < 0) {
                spriteRenderer.flipX = true;
            }

        } else {
            animator.SetBool("isMoving", false);
        }

        animator.SetBool("isHolding", holding);
        arm.SetActive(holding);


        if (currentInteractables.Count > 0) {
            closestInteractable = currentInteractables[0];
            float closestDst = Vector2.Distance(transform.position, closestInteractable.transform.position);
            closestInteractable.canInteract = true;

            for (int i = 1; i < currentInteractables.Count; i++) {
                float dst = Vector2.Distance(transform.position, currentInteractables[i].transform.position);

                if (dst < closestDst) {
                    closestDst = dst;
                    closestInteractable.canInteract = false;
                    closestInteractable = currentInteractables[i];
                    closestInteractable.canInteract = true;
                } else {
                    currentInteractables[i].canInteract = false;
                }
            }
            interactableText.enabled = true;
            interactableText.text = "Press E to pickup " + closestInteractable.name;

        } else {
            interactableText.enabled = false;
            closestInteractable = null;
        } 
    }

    // Casts the rigidbody of the player character in the Vector2 direction the player inputs 
    // and returns a bool if no collisions occur
    private bool TryToMove(Vector2 direction) {
        // Check for any collisions
        int count = GetComponent<BoxCollider2D>().Cast(
                direction, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
                movementFilter, // The settings that determine where a collision can happen on such layers to collide
                castCollisions, // List of collisions where the found collisions after the cast has finished
                moveSpeed * Time.fixedDeltaTime + collisionOffset); // The amount to cast equal to the movement plus an offset
        if(count == 0) {
            transform.position = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            return true;
        } else {
            return false;
        }
    }

    public void EquipGun(GunBase g){
        if(g){
            if(heldGun){
                g.GetComponent<SpriteRenderer>().flipY = heldGun.GetComponent<SpriteRenderer>().flipY;
                g.GetComponent<SpriteRenderer>().flipX = heldGun.GetComponent<SpriteRenderer>().flipX;
                g.GetComponent<SpriteRenderer>().sortingOrder = heldGun.GetComponent<SpriteRenderer>().sortingOrder;
                DropGun(heldGun);
            }
            heldGun = g;
            heldGun.owner = this;
            heldGun.transform.SetParent(arm.transform, false);
            heldGun.transform.localPosition = hand1Point.localPosition + heldGun.hand1Point.localPosition * -1;
            heldGun.transform.localRotation = Quaternion.identity;
            //something for 2 handed guns
            holding = true;
            spriteRenderer.flipX = false;
        }
    }

    public void DropGun(GunBase g){
        PickupGun newGunPickup = Instantiate(pickupGunRef, g.transform.position, Quaternion.identity).GetComponent<PickupGun>();
        g.transform.SetParent(newGunPickup.transform, false);
        //g.transform.localPosition = Vector3.zero;
        g.transform.localRotation = arm.transform.localRotation;
        if(arm.transform.localScale.x < 0)
            g.GetComponent<SpriteRenderer>().flipX = true;
        g.ReleaseTrigger();
        g.owner = null;
        newGunPickup.payload = g.transform;
        newGunPickup.name = g.name;
        newGunPickup.DropRandomDirection(false);
        holding = false;
    }
    public int AddAmmo(AmmoType t, int amount) {
        int remainder = Mathf.Max(amount - (maxAmmo[t] - heldAmmo[t]), 0);

        if(remainder < amount) {
            heldAmmo[t] += amount - remainder;
        }
        heldAmmoList = new List<int>(heldAmmo.Values);
        return remainder;
    }
    public int RemoveAmmo(AmmoType t, int amount) {
        int amtRemoved = Mathf.Min(amount , heldAmmo[t]);
        heldAmmo[t] -= amtRemoved;
        heldAmmoList = new List<int>(heldAmmo.Values);
        return amtRemoved;
    }

    public int CheckAmmo(AmmoType t) {
        return heldAmmo[t];
    }

    public void OnMoveInput(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
        //animator.SetBool("isMoving", movementInput.sqrMagnitude < .1);
    }

    
    public void OnLookInput(InputAction.CallbackContext context) {
      
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float angle = Mathf.Rad2Deg * Mathf.Atan((mousePos.y - arm.transform.position.y) / (mousePos.x - arm.transform.position.x));
        arm.transform.rotation = Quaternion.Euler(0, 0, angle);
        arm.transform.localScale = new Vector3((mousePos.x > arm.transform.position.x ? 1 : -1),1,1);
        //(mousePos.x < arm.transform.position.x ? 180 : 0)
        bool facingRight = mousePos.x > arm.transform.position.x;
        arm.GetComponent<SpriteRenderer>().sortingOrder = facingRight ? 2:-2;
        if(heldGun) {
            heldGun.GetComponent<SpriteRenderer>().sortingOrder = facingRight ? 1 : -1;
            //heldGun.GetComponent<SpriteRenderer>().flipY = !facingRight;
            //heldGun.transform.localPosition = new Vector3(heldGun.transform.localPosition.x, heldGun.transform.localPosition.y * -1);
        }

        //Changing walk anim when equipped
        animator.SetBool("facingRight", facingRight);
        
    }

    void OnPullTrigger(InputAction.CallbackContext context) {
        if(heldGun) { 
            heldGun.PullTrigger(); 
        } 
        //reload when empty
    }

    void OnReleaseTrigger(InputAction.CallbackContext context) {
        if(heldGun) {
            heldGun.ReleaseTrigger();
        }
    }

    void OnInteractInput(InputAction.CallbackContext context) {
        if(closestInteractable) {
            closestInteractable.Interact(this);
        }
    }

    void OnReloadInput(InputAction.CallbackContext context) {
        if(heldGun && heldGun.currentMagSize < heldGun.magMaxSize) {
            int amt = RemoveAmmo(heldGun.ammoType, heldGun.magMaxSize - heldGun.currentMagSize);
            //reload animation

            heldGun.FinishReload(heldGun.currentMagSize + amt); //implement ammo at some point
        }
    }
}
