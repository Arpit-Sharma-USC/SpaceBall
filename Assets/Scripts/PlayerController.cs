﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
enum HorizontalMovement
{
    None,
    Left,
    Right
}

public class PlayerController : MonoBehaviour
{
    private GameController gameController;
    public NotificationController notificationController; 

    public TileManager tile;
    public int maxHealth;
    public Slider healthSlider;
    public Image healthFillImage;
    public PlayerController play;

    public KeyCode moveL;
    public KeyCode moveR;
    public KeyCode moveSlow;
    private float horizSpeed = 9.0f;
    private float horizVelocity = 0;
    private float forwardSpeed = 10.0f;
    private float forwardSlowSpeed = 0.2f;
    public float verticalVelocity = 0.0f;
    public bool isFlying = false;
    public bool isShieldOn = false;
    public static int health = 10;

    private int numLanes;

    private Rigidbody rb;
    private HorizontalMovement horizontalMoveStatus = HorizontalMovement.None;
    private bool isLaneLockEnabled = true;

    private int currentLane;    // current lane
    private int targetLane;     // target lane of horizontal move
    private float targetXPos;   // target x position of horizontal move

    public GameObject magneticField;
    public GameObject shield;

    public Transform explodeObj;    //effect after collision with trap

    void Start()
    {
        health = maxHealth;
        healthSlider.maxValue = maxHealth;

        // Save a reference to the rigidbody object
        rb = GetComponent<Rigidbody>();

        // Save a reference to the main GameController object
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        notificationController = GameObject.FindWithTag("GameController").GetComponent<NotificationController>(); 

        // Retrieve the number of lanes in this game from GameController
        numLanes = gameController.numLanes;

        // Ball always starts at the center lane
        currentLane = (numLanes / 2) + 1;
        targetLane = currentLane;
        healthFillImage = healthSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
        healthFillImage.color = Color.green;
    }

    public void AddHealth(int x)
    {
        if (health + x >= maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += x;
        }

        notificationController.NotifyHealthChange(x);
    }

    void Update()
    {
        healthSlider.value = health;
        healthFillImage.color = Color.Lerp(Color.red, Color.green, (float)health / maxHealth);
        // If the ball is moving, check if movement is complete
        if (horizontalMoveStatus != HorizontalMovement.None)
        {
            CheckMoveComplete();
        }

        // If the ball is moving on its own
        else
        {
            float targetXPos = gameController.GetLaneCenterXPos(currentLane);

            // Check x position and adjust accordingly
            float offsetFromCenter = transform.position.x - targetXPos;

            // If the player crossed the boundary between lanes
            if ((Mathf.Abs(offsetFromCenter) > 0.5) && (currentLane != 1) && (currentLane != numLanes))
            {
                // Switch lane to either left or right
                if (offsetFromCenter > 0.5) currentLane++;
                else currentLane--;
            }

            else
            {
                if (isLaneLockEnabled)
                {
                    // Move the x position of the player towards the center of the lane
                    horizVelocity = -offsetFromCenter * 4;
                }
            }
        }

        if (isFlying)
        {
            rb.velocity = new Vector3(horizVelocity, verticalVelocity, forwardSpeed * 3);
        }

        else
        {
            rb.velocity = new Vector3(horizVelocity, verticalVelocity, forwardSpeed);
        }
    }

    void LateUpdate()
    {
        if ((transform.position.y >= 0.0f))
        {
            EnableLaneLock();
            horizSpeed = 10;
        }
        if (transform.position.y <= -15.0f)
        {
            gameController.GameOver(); 
        }
    }

    private void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.tag == "death" && !isFlying)
        {
            int damageAmount = -1; 

            SetHealth(GetHealth() + damageAmount);

            notificationController.NotifyHealthChange(damageAmount); 
        }

        // If player runs into a health box item
        else if (col.gameObject.tag == "ItemHealthBox")
        {
            Destroy(col.gameObject);
            AddHealth(2);
            Debug.Log("Healthbox");
        }

        else if (col.gameObject.tag == "ItemMagnet")
        {
            Destroy(col.gameObject);
            EnableMagneticField();
        }

        // If player runs into a 
        else if (col.gameObject.tag == "ItemWing")
        {
            Destroy(col.gameObject);
            Fly();
        }

        // If player runs into a shield item
        else if (col.gameObject.tag == "ItemShield")
        {
            Debug.Log("Shield");
            Destroy(col.gameObject);
            EnableShield();
        }

        else if (col.gameObject.tag == "speedAddRampToBall")
        {
            forwardSlowSpeed = 1.2f;
        }

        else
        {
            forwardSlowSpeed = 0.2f;
        }
    }

    void OnTriggerEnter(Collider col)
    {

    }

    // Move the player to the left lane
    public void MoveLeft()
    {
        if ((currentLane > 1) && horizontalMoveStatus != HorizontalMovement.Left)
        {
            horizontalMoveStatus = HorizontalMovement.Left;
            horizVelocity = -horizSpeed;
            targetLane = targetLane - 1;
            targetXPos = gameController.GetLaneCenterXPos(targetLane);
        }
    }

    // Move the player to the right lane
    public void MoveRight()
    {
        if ((currentLane < numLanes) && horizontalMoveStatus != HorizontalMovement.Right)
        {
            horizontalMoveStatus = HorizontalMovement.Right;
            horizVelocity = horizSpeed;
            targetLane = targetLane + 1;
            targetXPos = gameController.GetLaneCenterXPos(targetLane);
        }
    }

    // If the player is in the process of moving, check if lane shifting is complete
    public void CheckMoveComplete()
    {
        // Moving left
        if (currentLane > targetLane)
        {
            if (transform.position.x <= targetXPos)
            {
                OnHorizontalMoveComplete();
            }
        }

        // Moving right
        else if (currentLane < targetLane)
        {
            if (transform.position.x >= targetXPos)
            {
                OnHorizontalMoveComplete();
            }
        }
    }

    // Event handler when moving to a different lane is complete
    public void OnHorizontalMoveComplete()
    {
        horizontalMoveStatus = HorizontalMovement.None;
        currentLane = targetLane;
        horizVelocity = 0;
        transform.position = new Vector3(targetXPos, transform.position.y, transform.position.z);
    }

    public void MoveSlow()
    {
        rb.velocity = new Vector3(0, 0, forwardSlowSpeed);
    }

    public void EnableLaneLock()
    {
        isLaneLockEnabled = true;
    }

    public void DisableLaneLock()
    {
        isLaneLockEnabled = false;
    }

    public void EnableMagneticField()
    {
        EnableMagneticField(5.0f, true);
    }

    public void EnableMagneticField(float magneticFieldSize, bool automaticDisable)
    {
        magneticField.SetActive(true);
        magneticField.transform.localScale = new Vector3(magneticFieldSize, magneticFieldSize, magneticFieldSize);

        // If automatic disable option after x seconds is on, start a corutine
        if (automaticDisable)
        {
            StartCoroutine(DisableMagneticFieldAfterDelay());
        }
    }

    public void DisableMagneticField()
    {
        magneticField.SetActive(false);
    }

    IEnumerator DisableMagneticFieldAfterDelay()
    {
        yield return new WaitForSeconds(10);
        DisableMagneticField();
    }

    public void EnableShield()
    {
        EnableShield(true);
    }

    public void EnableShield(bool automaticDisable)
    {
        shield.SetActive(true);

        if (automaticDisable)
        {
            StartCoroutine(DisableShieldAfterDelay());
        }
    }

    public void DisableShield()
    {
        shield.SetActive(false);
    }

    IEnumerator DisableShieldAfterDelay()
    {
        yield return new WaitForSeconds(10);
        DisableShield();
    }

    public void AddSpeed(float modifier)
    {
        forwardSpeed = forwardSpeed + modifier;
    }

    public float GetSpeed()
    {
        return forwardSpeed;
    }

    // Start flying
    public void Fly()
    {
        // Actual flying motion is done through FlightController
        GetComponent<FlightController>().Fly();
        EnableMagneticField(20.0f, false);
    }

    public void OnGUI()
    {
        if (health <= 0)
        {
            gameController.GameOver(); 
        }
    }
    public int GetHealth()
    {
        return health;
    }
    public void SetHealth(int x)
    {
        health = x;
    }
}