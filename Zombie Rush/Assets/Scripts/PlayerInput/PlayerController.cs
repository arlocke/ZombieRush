using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour {
    //Character Data
    public bool isHolding = false;

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
    Rigidbody2D rb;
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

        controls.Player.Shoot.performed += OnShoot;
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
    }

    // Casts the rigidbody of the player character in the Vector2 direction the player inputs 
    // and returns a bool if no collisions occur
    private bool TryToMove(Vector2 direction) {
        // Check for any collisions
        int count = rb.Cast(
                direction, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
                movementFilter, // The settings that determine where a collision can happen on such layers to collide
                castCollisions, // List of collisions where the found collisions after the cast has finished
                moveSpeed * Time.fixedDeltaTime + collisionOffset); // Teh amount to cast equal to the movement plus an offset

        if (count == 0) {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            return true;
        } else {
            return false;
        }
    }

    /* void OnMove(InputValue movementValue)
     {
         movementInput = movementValue.Get<Vector2>();
     }*/

    void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    //
    void OnLook(InputAction.CallbackContext context) {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float angle = Mathf.Rad2Deg * Mathf.Atan((mousePos.y - arm.transform.position.y) / (mousePos.x - arm.transform.position.x)) + (mousePos.x < arm.transform.position.x ? 180:0);
        arm.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnShoot(InputAction.CallbackContext context) {
        gun.Shoot();
    }
}
