using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class PlayerController : MonoBehaviour {
    //Interaction Data
    public List<Interactable> currentInteractables;
    public Text interactableText;


    //Animation Data
    public bool isHolding = false; //all this affects is idle animation

    //Movement variables
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f; // Distance from rigidbody to check for collisions
    public ContactFilter2D movementFilter;

    //Children
    public GameObject arm;
    public GunBase gun;

    PlayerInputActions controls;

    Vector2 movementInput;
    SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    Animator animator;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();


    private void Awake() {
        controls = new PlayerInputActions();

    }

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        arm = transform.Find("Arm").gameObject;

        //controls.Player.Look.performed += _ => Look();
    }

    private void OnEnable() {
        controls.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Look.performed += OnLook;
        controls.Player.Look.canceled += OnLook;

        controls.Player.Shoot.performed += OnPullTrigger;
        controls.Player.Shoot.canceled += OnReleaseTrigger;
    }

    private void OnDisable() {
        controls.Disable();
    }

    private void FixedUpdate() {
        if (movementInput != Vector2.zero) {
            bool success = TryToMove(movementInput);

            if (!success) {
                // Attempts to "slide" when colliding in the X direction
                success = TryToMove(new Vector2(movementInput.x, 0));

                if (!success) {
                    // Attempts to "slide" when colliding in the Y direction
                    success = TryToMove(new Vector2(0, movementInput.y));
                }
            }

            animator.SetBool("isMoving", true);

            //Changing direction of walk anim when unequipped
            if (!isHolding && movementInput.x > 0) {
                spriteRenderer.flipX = false;
            }
            if (!isHolding && movementInput.x < 0) {
                spriteRenderer.flipX = true;
            }


            //Changing walk anim when equipped
            if (isHolding && movementInput != Vector2.zero) {
                animator.SetFloat("xDirection", movementInput.x);
                animator.SetBool("isMoving", true);
            }
        } else {
            animator.SetBool("isMoving", false);
        }

        if (isHolding) {
            animator.SetBool("isHolding", true);
            arm.SetActive(true);

        }
        if (!isHolding) {
            animator.SetBool("isHolding", false);
            arm.SetActive(false);
        }

        if (currentInteractables.Count > 0) {
            Interactable closestInteractable = currentInteractables[0];
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
        if (count == 0) {
            transform.position = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            return true;
        } else {
            return false;
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    //
    public void OnLook(InputAction.CallbackContext context) {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float angle = Mathf.Rad2Deg * Mathf.Atan((mousePos.y - arm.transform.position.y) / (mousePos.x - arm.transform.position.x)) + (mousePos.x < arm.transform.position.x ? 180:0);
        arm.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnPullTrigger(InputAction.CallbackContext context) {
        gun.PullTrigger();
    }

    void OnReleaseTrigger(InputAction.CallbackContext context) {
        gun.ReleaseTrigger();
    }
}
