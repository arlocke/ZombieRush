using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Movement variables
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f; // Distance from rigidbody to check for collisions
    public ContactFilter2D movementFilter;

    //Character Data
    public bool isHolding = false;

    /*
     * public GameObject<Gun> heldGun;
     * public GameObject<Melee> heldMelee;
     * public GameObject<Util> heldUtil;
     * 
     * List<GameObject> bag = new List<GameObject>(9);
     */


    Vector2 movementInput;
    Rigidbody2D rb;
    Animator animator;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if(movementInput != Vector2.zero)
        {
            bool success = TryToMove(movementInput);

            if (!success)
            {
                // Attempts to "slide" when colliding in the X direction
                success = TryToMove(new Vector2(movementInput.x, 0));

                if (!success)
                {
                    // Attempts to "slide" when colliding in the Y direction
                    success = TryToMove(new Vector2(0, movementInput.y));
                }
            }

            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);

        }
    }

    // Casts the rigidbody of the player character in the Vector2 direction the player inputs 
    // and returns a bool if no collisions occur
    private bool TryToMove(Vector2 direction)
    {
        // Check for any collisions
        int count = rb.Cast(
                direction, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
                movementFilter, // The settings that determine where a collision can happen on such layers to collide
                castCollisions, // List of collisions where the found collisions after the cast has finished
                moveSpeed * Time.fixedDeltaTime + collisionOffset); // Teh amount to cast equal to the movement plus an offset

        if (count == 0)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            return true;
        } else {
            return false;
        }
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
