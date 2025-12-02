using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool pcInput;

    private Animator animator;

    private float HInput = 0, VInput = 0;
    private float tempH = 0, tempV = 0;
    [SerializeField]
    private float lerpSpeed = 10;
    [SerializeField]
    private float movementThreshold = .01f;
    private Vector3 velocity = Vector3.zero;
    public float speed = 1.2f;

    public float rotationStartSpeed = 250;
    public float slowRotationSpeed = 50;
    [HideInInspector]
    public float selfRotationSpeed;
    [HideInInspector]
    public float oponentRotationSpeed;
    public bool canSelfRotate = true;
    public bool canOponentRotate = true;


    [HideInInspector]
    public Vector3 jumpForce;
    public int jumpUpForce = 5;
    public int jumpDashForce = 5;
    public byte airSpeedMultiplier = 6;

    private Rigidbody rb;

    public Joystick joystick;

    public Transform oponent;

    [SerializeField]
    private float oponentDistance;

    public bool canMove = true;
    public bool isGrounded = true;
    public bool isJumping = false;

    public bool isSelfDefending = false;
    public bool isOponentDefending = false;
    
    private Vector3 direction;
    private Quaternion rotation;
    public float stopRotationWaitTime = .2f;

    // Start is called before the first frame update
    void Start()
    {
        selfRotationSpeed = rotationStartSpeed;
        pcInput = FindFirstObjectByType<Manager>().pcInput;
        if (!pcInput)
        {
            joystick = FindFirstObjectByType<Manager>().joystick.GetComponent<Joystick>();
            joystick.gameObject.SetActive(true);
        }
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        
        // Ensure player starts as grounded
        isGrounded = true;
        
        //animator.SetFloat("Speed", speed);
    }

    private void OnDestroy()
    {
        if (!pcInput)
            joystick.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (pcInput)
            GetPcInput();
        else
            GetAndroidInput();

        SmoothInput();
        
        // Backup ground check using raycast (in case GroundCheck triggers don't work)
        CheckGroundWithRaycast();
        
        if (oponent != null)
        {
            //oponentDistance = Vector3.Distance(transform.position, oponent.position);
            RotatePlayer();
        }


        //animator.SetFloat("HInput", HInput, .04f, Time.deltaTime);
        //animator.SetFloat("VInput", VInput, .04f, Time.deltaTime);
        animator.SetFloat("VInput", VInput);
        animator.SetFloat("HInput", HInput);

        //if(canMove) MovePlayer();

    }
    
    void CheckGroundWithRaycast()
    {
        // Cast a ray downward to check if we're on ground
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 0.3f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                if (!isGrounded && !isJumping)
                {
                    isGrounded = true;
                    if (animator != null)
                    {
                        animator.SetBool("IsGrounded", true);
                        animator.applyRootMotion = true;
                    }
                }
                
                // Reset jumping state when grounded
                if (isJumping && rb.linearVelocity.y <= 0.1f)
                {
                    isJumping = false;
                    if (animator != null)
                        animator.SetBool("IsJumping", false);
                }
            }
        }
        else
        {
            // Not touching ground
            if (isGrounded && !isJumping)
            {
                isGrounded = false;
                if (animator != null)
                    animator.SetBool("IsGrounded", false);
            }
        }
    }

    public void Jump()
    {
        Debug.Log($"Jump called! isGrounded: {isGrounded}, isJumping: {isJumping}");
        
        if (!isGrounded || isJumping)
        {
            Debug.LogWarning($"Can't jump! isGrounded: {isGrounded}, isJumping: {isJumping}");
            return;
        }
        
        Debug.Log("JUMPING!");
        isJumping = true;
        jumpForce = (new Vector3(HInput * jumpDashForce, jumpUpForce, VInput * jumpDashForce).normalized)*airSpeedMultiplier;
        animator.applyRootMotion = false;
        animator.SetBool("IsJumping", true);
        rb.AddRelativeForce(jumpForce, ForceMode.Impulse);
    }

    private void RotatePlayer()
    {
        if (canSelfRotate)
        {
            //AngularDistance = Quaternion.Angle(transform.rotation, oponent.rotation);
            direction = oponent.position - transform.position;
            direction.y = 0;
            rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, selfRotationSpeed * Time.deltaTime);
            //transform.rotation = rotation;
            //print("local: "+AngularDistance + "|" + direction + "|" + rotation);
            
        }
        if (canOponentRotate)
        {
            //AngularDistance = Quaternion.Angle(oponent.rotation, transform.rotation);
            direction = transform.position - oponent.position;
            direction.y = 0;
            rotation = Quaternion.LookRotation(direction);
            oponent.rotation = Quaternion.RotateTowards(oponent.rotation, rotation, oponentRotationSpeed * Time.deltaTime);
            //oponent.rotation = rotation;
            //print("oponent: " + AngularDistance + "|" + direction + "|" + rotation);
        }
    }

    public void StopSelfRotation()
    {
        StartCoroutine(DisableRotation(true));
    }

    public void StopOponentRotation()
    {
        StartCoroutine(DisableRotation(false));
    }

    IEnumerator DisableRotation(bool isSelf)
    {
        yield return new WaitForSeconds(stopRotationWaitTime);
        if (isSelf)
            canSelfRotate = false;
        else
            canOponentRotate = false;
    }

    public void SlowSelfRotation()
    {
        //Debug.Log("Slowing Self Rotation");
        selfRotationSpeed = slowRotationSpeed;
        canSelfRotate = true;

    }

    public void SlowOponentRotation()
    {
        //Debug.Log("Slowing Oponent Rotation");
        oponentRotationSpeed = slowRotationSpeed;
        canOponentRotate = true;
    }

    public void ResetSelfRotation()
    {
        //Debug.Log("Resetting Self Rotation");
        selfRotationSpeed = rotationStartSpeed;
        canSelfRotate = true;
    }

    public void ResetOponentRotation()
    {
        //Debug.Log("Resetting Oponent Rotation");
        oponentRotationSpeed = rotationStartSpeed;
        canOponentRotate = true;
    }

    private void GetAndroidInput()
    {
        tempH = joystick.Horizontal;
        if (tempH > .33) tempH = 1;
        else if (tempH < -.33) tempH = -1;
        else tempH = 0;
        tempV = joystick.Vertical;
        if (tempV > .33) tempV = 1;
        else if (tempV < -.33) tempV = -1;
        else tempV = 0;
    }

    private void GetPcInput()
    {
        tempH = Input.GetAxisRaw("Horizontal");
        tempV = Input.GetAxisRaw("Vertical");

        //VInput = Input.GetAxisRaw("Vertical");
        //HInput = Input.GetAxisRaw("Horizontal");

        //HInput = Input.GetAxis("Horizontal");
        //VInput = Input.GetAxis("Vertical");
    }

    public void SmoothInput()
    {
        HInput = Mathf.Lerp(HInput, tempH, lerpSpeed * Time.deltaTime);
        VInput = Mathf.Lerp(VInput, tempV, lerpSpeed * Time.deltaTime);
        if (HInput < movementThreshold && HInput > -movementThreshold && tempH == 0)
            HInput = 0;

        if (VInput < movementThreshold && VInput > -movementThreshold && tempV == 0)
            VInput = 0;
    }

    private void MovePlayer()
    {
        Vector3 moveHorizontal = transform.right * HInput;
        Vector3 moveVertical = transform.forward * VInput;

        velocity = (moveHorizontal + moveVertical).normalized * speed;
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.deltaTime);
        }
    }

    public void FreezePosition()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
    }

    public void ResetConstraints()
    {

        rb.constraints = RigidbodyConstraints.None;
    }

}
