﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerationController : MonoBehaviour {
    public GameObject coinPrefab;
    public GameObject itemHealthBoxPrefab;
    public GameObject itemMagnetPrefab;
    public GameObject itemShieldPrefab;
    public GameObject itemWingPrefab;
    public GameObject itemMissilePrefab; 
    
    private PlayerController playerController;

    private float lastGeneratedPlayerZPos = 0; 

    // Use this for initialization
    void Start() {
        playerController = GetComponent<PlayerController>(); 

        StartCoroutine(GenerateContinuously());
    }
    
    void LateUpdate()
    {
        
    }

    void Generate(GameObject prefab)
    {
        GameObject newRewardObj = Instantiate(prefab) as GameObject;

        float spawnZPos = transform.position.z + playerController.GetSpeed() * 4;

        newRewardObj.transform.position = new Vector3(Mathf.Round(Random.Range(-2.0f, 2.0f)), 5, spawnZPos);
    }

    IEnumerator GenerateContinuously()
    {
        while(gameObject != null)
        {
            if (transform.position.z > Mathf.Max(lastGeneratedPlayerZPos + 2.0f, 0))
            {
                float randVal = Random.Range(0, 100);

                if (randVal < 2)
                {
                    Generate(itemWingPrefab);
                }

                else if (randVal < 5)
                {
                    Generate(itemMagnetPrefab);
                }

                else if (randVal < 8)
                {
                    Generate(itemShieldPrefab); 
                }

                else if (randVal < 11)
                {
                    Generate(itemHealthBoxPrefab); 
                }

                else if (randVal < 15)
                {
                    Generate(itemMissilePrefab); 
                }

                else
                {
                    Generate(coinPrefab);
                }
                
                lastGeneratedPlayerZPos = transform.position.z; 
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

}
