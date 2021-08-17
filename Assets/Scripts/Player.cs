using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Transform cam;

    public float walkSpeed = 15.0f;
    public float dashSpeed = 20.0f;
    public float dashDistance = 5.0f;
    public float fallAccel = 60.0f;
    public float maxFallSpeed = 25.0f;
    public float diveAccel = 180.0f;
    public float maxDiveSpeed = 70.0f;
    public float diveExplosionSpeed = 60.0f;
    public float jumpSpeed = 20.0f;
    public float jumpTime = 0.25f;
    public float coyoteTime = 0.1f;

    public bool canHover = true;
    public bool canAirDash = false;
    public bool canDive = true;

    public Damager meleeAttack;

    private bool hasAirDash = false;
    private bool isDashing = false;
    private bool hasDiveExplosion = false;
    private bool isHovering = false;
    private float jumpLeft = 0.0f;
    private float dashLeft = 0.0f;
    private float coyoteing = 0.0f;
    private Vector2 dashDirection = Vector2.zero;

    private CharacterController controller;
    private PlayerInput input;
    private Animator animator;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        meleeAttack.active = false;
    }

    void Update()
    {
        bool onGround = controller.isGrounded;
        bool coyoteOnGround = onGround || coyoteing < coyoteTime;
        float fallSpeed = controller.velocity.y;
        Vector2 horizontalVelocity = new Vector2(controller.velocity.x, controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        meleeAttack.active = input.actions["Melee"].ReadValue<float>() != 0;
        animator.SetBool("Melee", meleeAttack.active);
        if (isDashing)
        {
            if (currentSpeed == 0 || dashLeft <= 0 || input.actions["Dash"].ReadValue<float>() == 0)
            {
                isDashing = false;
            }
            else
            {
                fallSpeed = 0;
                if (dashDirection.magnitude < currentSpeed)
                {
                    dashDirection = dashDirection.normalized * currentSpeed;
                }
                else
                {
                    currentSpeed = dashDirection.magnitude;
                }
                dashLeft -= Time.deltaTime * currentSpeed;
            }
        }
        else if ((coyoteOnGround || hasAirDash) && input.actions["Dash"].triggered)
        {
            dashDirection = horizontalVelocity;
            if (dashDirection.magnitude < dashSpeed)
            {
                if (dashDirection.magnitude == 0)
                {
                    dashDirection = new Vector2(transform.forward.x, transform.forward.z).normalized * dashSpeed;
                }
                dashDirection = dashDirection.normalized * dashSpeed;
            }
            isDashing = true;
            dashLeft = dashDistance;
            if (!coyoteOnGround)
            {
                hasAirDash = false;
            }
        }
        if (input.actions["Jump"].ReadValue<float>() != 0.0f && jumpLeft > 0)
        {
            coyoteing = coyoteTime;
            jumpLeft -= Time.deltaTime;
            fallSpeed = Mathf.Max(jumpSpeed, fallSpeed);
            if (isDashing)
            {
                isDashing = false;
            }
        }
        else
        {
            if (input.actions["Dive"].ReadValue<float>() != 0)
            {
                isHovering = false;
                fallSpeed -= diveAccel * Time.deltaTime;
                fallSpeed = Mathf.Max(fallSpeed, maxDiveSpeed * -1);
                hasDiveExplosion = fallSpeed < diveExplosionSpeed * -1;
            }
            else if (input.actions["Jump"].triggered && canHover)
            {
                isHovering = !isHovering;
                fallSpeed = 0;
                if (isDashing)
                {
                    isDashing = false;
                    if (!onGround)
                    {
                        dashLeft = 0;
                    }
                }
            }
            else if (isHovering)
            {
                fallSpeed = 0;
            }
            else
            {
                fallSpeed -= fallAccel * Time.deltaTime;
                fallSpeed = Mathf.Max(fallSpeed, maxFallSpeed * -1);
            }
            if (onGround)
            {
                if (input.actions["Jump"].ReadValue<float>() == 0.0f)
                {
                    jumpLeft = jumpTime;
                }
                if (hasDiveExplosion)
                {
                    // Spawn dive explosion
                    hasDiveExplosion = false;
                    Debug.Log("Dive explosion");
                }
                hasAirDash = true;
                coyoteing = 0;
            }
            else if (coyoteOnGround)
            {
                coyoteing += Time.deltaTime;
            }
            else
            {
                jumpLeft = 0;
            }
        }

        if (fallSpeed > diveExplosionSpeed * -1)
        {
            hasDiveExplosion = false;
        }

        if (isDashing)
        {
            Vector3 newVelocity = new Vector3(dashDirection.x, fallSpeed, dashDirection.y);
            controller.Move(newVelocity * Time.deltaTime);
        }
        else
        {
            Vector2 moveValue = input.actions["Move"].ReadValue<Vector2>();
            Vector3 cameraMove = cam.transform.rotation * new Vector3(moveValue.x, 0, moveValue.y);
            cameraMove.y = 0;
            float newSpeed = 0;
            if (cameraMove.magnitude > 0)
            {
                cameraMove.Normalize();
                float moveMagnitude = moveValue.magnitude;
                // If the magnitude is above a certain threshold, bump it up to max
                if (moveMagnitude > 0.9f)
                {
                    moveMagnitude = 1.0f;
                }
                newSpeed = walkSpeed * moveMagnitude;
                float targetAngle = Mathf.Atan2(cameraMove.x, cameraMove.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, targetAngle - 90, 0);
            }
            Vector3 newVelocity = new Vector3(cameraMove.x * newSpeed, fallSpeed, cameraMove.z * newSpeed);
            controller.Move(newVelocity * Time.deltaTime);
        }
    }
}
