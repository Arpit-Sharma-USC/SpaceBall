﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    private GameObject player;

    public bool isSpinning;
    public int rewards;
    public float cubeZAxis=0;

    private bool isInMagneticField = false;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindWithTag("Player");

        cubeZAxis = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpinning)
        {
            transform.Rotate(new Vector3(3, 3, 3));
        }

        if (isInMagneticField)
        {
            float speed = player.GetComponent<Rigidbody>().velocity.z * 2; 
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "Player")
        {
                rewards = col.GetComponent<PlayerController>().rewards++;
                Destroy(gameObject);
                if (rewards == 3)
                {
                    
                }
        }

        else if (col.gameObject.tag == "MagneticField")
        {
            isInMagneticField = true;
        }
    }
}