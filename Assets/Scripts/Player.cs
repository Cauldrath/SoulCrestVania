using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Transform cam;

    private CharacterController controller;
    private PlayerInput input;

    public float walkSpeed = 15.0f;
    public float dashSpeed = 20.0f;
    public float dashDistance = 5.0f;
    public float maxGrappleDistance = 10.0f;
    public float grappleDelay = 0.1f;
    public float grappleFallAccel = 60.0f;
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
    public bool canGrapple = false;
    public bool grappleAnySurface = false;

    private bool hasAirDash = false;
    private bool isDashing = false;
    private bool hasDiveExplosion = false;
    private bool isGrappling = false;
    private bool isHovering = false;
    private float jumpLeft = 0.0f;
    private float dashLeft = 0.0f;
    private float coyoteing = 0.0f;
    private float grappleTimer = 0.0f;
    private float grappleDistance = 0.0f;
    private Vector3 grapplePoint = Vector3.zero;
    private Vector2 dashDirection = Vector2.zero;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        bool onGround = controller.isGrounded;
        if (isGrappling)
        {
            if (onGround || input.actions["Grapple"].ReadValue<float>() == 0)
            {
                isGrappling = false;
            }
            else
            {
                isDashing = false;
                jumpLeft = 0;
                Vector3 fallingVelocity = controller.velocity + (Vector3.down * grappleFallAccel * Time.deltaTime);
                Vector3 grappleDirection = grapplePoint - controller.transform.position;
                Vector3 rightVector = Vector3.Cross(fallingVelocity.normalized, grappleDirection.normalized);
                Vector3 newVelocity = Vector3.Cross(grappleDirection.normalized, rightVector) * fallingVelocity.magnitude;
                Vector3 tetherDirection = (grapplePoint - grappleDirection.normalized * grappleDistance) - transform.position;
                controller.Move((newVelocity + tetherDirection).normalized * newVelocity.magnitude * Time.deltaTime);
            }
        }
        else
        {
            if (input.actions["Grapple"].ReadValue<float>() != 0)
            {
                grappleTimer += Time.deltaTime;
                if (grappleTimer > grappleDelay)
                {
                    isGrappling = true;
                    // TODO: actually grapple to surface instead of an arbitrary point
                    grapplePoint = controller.transform.position + (controller.transform.forward.normalized + Vector3.up).normalized * maxGrappleDistance;
                    grappleDistance = (grapplePoint - controller.transform.position).magnitude;
                }
            }
            else
            {
                grappleTimer = 0;
            }
            bool coyoteOnGround = onGround || coyoteing < coyoteTime;
            float fallSpeed = controller.velocity.y;
            Vector2 horizontalVelocity = new Vector2(controller.velocity.x, controller.velocity.z);
            float currentSpeed = horizontalVelocity.magnitude;
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
                    transform.rotation = Quaternion.Euler(0, targetAngle, 0);
                }
                Vector3 newVelocity = new Vector3(cameraMove.x * newSpeed, fallSpeed, cameraMove.z * newSpeed);
                controller.Move(newVelocity * Time.deltaTime);
            }
        }
    }
}
