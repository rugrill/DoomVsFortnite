using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharController : MonoBehaviour
{
    public bool canMove = true;
    public bool isSprinting = false;
    private bool isDashing = false;
    private bool canDash = true;
    private bool activeGrapple = false; 
    private bool enableMoveOnFirstTouch = false;

    private int numJumps = 0;
    [Header("Move Parameter")]

    public float maxHealth = 100f;
    private float health;

    [Header("Move Parameter")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float jumpPower = 5f;
    public float boostedJumpPower = 10f;
    public float gravity = 9.81f;

    [Header("Ladder Parameter")]
    public float ladderGrabDistance = 0.9f; //Raycast strats in middle of Player, so Player radius (0.5) + 0.4 outside of Player
    private bool isOnLadder = false;
    public float ladderSpeed = 4f;
    private Vector3 firstDirectionOnContactLadder;

    private CharacterController characterController;
    public PlayerHealthBar playerHealthBar;
    private Vector3 moveVelocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 move;
    private float rotation;

    [Header("Camera Parameter")]
    public Camera fpsCamera;
    public float lookSeed = 2f;
    public float lookLimit = 45f;
    public float initFov = 60f;
    private float grappleFov = 75f;
    // Start is called before the first frame update

    [Header("Dash Parameter")]
    private float dashDistance = 1f;
    private float dashDuration = 0.15f;
    public float dashCoolDown = 5f;
    void Start()
    {
        health = maxHealth;
        fpsCamera.fieldOfView = initFov;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Camera
        if (canMove) {
            rotation += -lookInput.y * lookSeed;
            rotation = Mathf.Clamp(rotation, -lookLimit, lookLimit);

            fpsCamera.transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSeed, 0);
        }

        //When Grabbling the player should not be able to move
        if (activeGrapple)
        {
            Debug.Log("Active Grapple, moving: " + move.x + " " + move.y + " " + move.z);
            characterController.Move(move * Time.deltaTime);
            return;
        }

        

         
        //Player Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        if (!isOnLadder) {
        float currentSpeedX = canMove ? (isSprinting ? sprintSpeed : walkSpeed) * moveInput.y : 0f;
        float currentSpeedY = canMove ? (isSprinting ? sprintSpeed : walkSpeed) * moveInput.x : 0f;

        float movementVelocity = moveVelocity.y;
        
            moveVelocity = (forward * currentSpeedX) + (right * currentSpeedY);
        
        

        //Jumping

        
        moveVelocity.y = movementVelocity;
        if (!characterController.isGrounded) {
            moveVelocity.y -= gravity * Time.deltaTime;
        }
        }

        if (!isOnLadder)
        {
            if (Physics.Raycast(transform.position, forward, out RaycastHit raycastHit, ladderGrabDistance))
            {
                if (raycastHit.transform.CompareTag("Ladder"))
                {
                    GrabOnLadder(forward);
                    Debug.Log("Ladder Grab on: " + raycastHit.transform);
                }
            }

        } else {
            if (Physics.Raycast(transform.position, firstDirectionOnContactLadder, out RaycastHit raycastHit, ladderGrabDistance))
            {
                
                 
            } else {
                LeaveLadder();
                Debug.Log("Ladder leave on 2");
            }

            if (Vector3.Dot(moveVelocity, Vector3.up) < 0) {
                if (characterController.isGrounded) {
                    LeaveLadder();
                    Debug.Log("Ladder leave on 3");
                    characterController.Move(forward * -0.4f );
                }
                
            }
             
            Debug.DrawLine(transform.position, transform.position + firstDirectionOnContactLadder*ladderGrabDistance , Color.red);
        }
         Debug.DrawLine(transform.position, transform.position + forward*ladderGrabDistance, Color.red);


        if (isOnLadder) {
           moveVelocity.x = 0;
           moveVelocity.y = ladderSpeed*moveInput.y;
           moveVelocity.z = 0; 
        }

        //Debug.Log("Move Velocity: " + moveVelocity);
        characterController.Move(moveVelocity * Time.deltaTime);
    }

    public void TakeDamage(float damage) {
        health -= damage;
        playerHealthBar.SetPlayerHealthBarSize(health / maxHealth);
        if (health <= 0) {
            Die();
        }
    }

    public void Die() {
        Debug.Log("Player died");
        GameManager.Instance.OnPlayerDeath();
    }

    public void HandleMoveInput(InputAction.CallbackContext ctx) {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void GrabOnLadder(Vector3 direction) {
        firstDirectionOnContactLadder = direction;
        isOnLadder = true;
    }

    private void LeaveLadder() {
        isOnLadder = false;
    }   

    public void HandleSprintInput(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            isSprinting = true;
        }else if(ctx.canceled) {
            isSprinting = false;
        }
    }

    public void HandleLookInput(InputAction.CallbackContext ctx) {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void HandleJumpInput(InputAction.CallbackContext ctx) {
        if (ctx.performed && characterController.isGrounded) {
            numJumps = 0;
        }
        if ((ctx.performed && canMove) && numJumps < 2) {
            moveVelocity.y = jumpPower;
            numJumps++;
        }

        LeaveLadder();
    }

    public void HandleDashInput(InputAction.CallbackContext ctx) {
        if (ctx.performed && canDash) {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash() {
        isDashing = true;
        fpsCamera.fieldOfView = grappleFov;
        canDash = false;
        float startTime = Time.time;
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 dashVelocity = Vector3.zero;

        float currentSpeedY = dashDistance * moveInput.x;

        dashVelocity = (forward * dashDistance) + (right * currentSpeedY);
        while (Time.time < startTime + dashDuration) {
            characterController.Move(dashVelocity);
            yield return null;
        }
        isDashing = false;
        fpsCamera.fieldOfView = initFov;
        yield return new WaitForSeconds(dashCoolDown);
        canDash = true;
    }


    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        Debug.Log("Jumping to position " + targetPosition.x + " " + targetPosition.y + " " + targetPosition.z);
        Debug.Log("JUmping from position " + transform.position.x + " " + transform.position.y + " " + transform.position.z);
        activeGrapple = true;
        Vector3 direction = targetPosition - transform.position;
        if (direction.magnitude < 0.1f)
        {
            Debug.Log("Reached Target Position");
            Invoke(nameof(ResetRestrictions), 3f);
            return; // Exit if close enough
        }
        move = direction.normalized * 10f;
        //move = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);

        Invoke(nameof(SetVelocityForGrabble), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private void SetVelocityForGrabble() {
        enableMoveOnFirstTouch = true;
    }

    private void ResetRestrictions() {
        Debug.Log("Resetting restrictions");
        activeGrapple = false;
        enableMoveOnFirstTouch = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (enableMoveOnFirstTouch)
        {
            Debug.Log("First touch when Grabbling");
            ResetRestrictions();
            GetComponent<GrabbleHook>().StopGrapple();
        }

        if (hit.gameObject.CompareTag("Jumper"))
        {
            moveVelocity.y = boostedJumpPower;   
        }
    }


    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
