using System.Collections.Generic;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    public GameObject cloudPrefab;
    public int numberOfClouds = 10;
    public Camera mainCamera;
    public float spawnDistance = 50f;
    public float cloudHeight = 20f;
    public float cloudSpeed = 5f;

    private readonly List<GameObject> clouds = new();

    void Start()
    {
        GenerateClouds();
    }

    void Update()
    {
        MoveCloudsWithCamera();
        RotateCloudsTowardsPlayer();
    }

    void GenerateClouds()
    {
        for (int i = 0; i < numberOfClouds; i++)
        {
            Vector3 spawnDirection = mainCamera.transform.forward;

            float randomXOffset = Random.Range(-20f, 20f);
            float randomZOffset = Random.Range(-20f, 20f);
            Vector3 offset = new(randomXOffset, 0, randomZOffset);

            Vector3 spawnPosition = mainCamera.transform.position + spawnDirection * spawnDistance + offset;
            spawnPosition.y = cloudHeight;

            GameObject cloud = Instantiate(cloudPrefab, spawnPosition, cloudPrefab.transform.rotation);
            clouds.Add(cloud);
        }
    }

    void MoveCloudsWithCamera()
    {
        foreach (GameObject cloud in clouds)
        {
            Vector3 cloudDirection = mainCamera.transform.forward;
            cloud.transform.position += cloudSpeed * Time.deltaTime * cloudDirection;

            float targetHeight = cloudHeight - 5f;
            cloud.transform.position = new Vector3(cloud.transform.position.x, Mathf.Lerp(cloud.transform.position.y, targetHeight, Time.deltaTime), cloud.transform.position.z);

            if (Vector3.Distance(cloud.transform.position, mainCamera.transform.position) > spawnDistance + 20f)
            {
                Vector3 newSpawnDirection = mainCamera.transform.forward;
                cloud.transform.position = mainCamera.transform.position + newSpawnDirection * spawnDistance;

                float randomXOffset = Random.Range(-20f, 20f);
                float randomZOffset = Random.Range(-20f, 20f);
                cloud.transform.position = new Vector3(randomXOffset, cloudHeight, randomZOffset);
            }
        }
    }

    void RotateCloudsTowardsPlayer()
    {
        foreach (GameObject cloud in clouds)
        {
            Vector3 playerPosition = mainCamera.transform.position;
            Vector3 cloudPosition = cloud.transform.position;

            Vector3 directionToPlayer = playerPosition - cloudPosition;

            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                cloud.transform.rotation = Quaternion.Euler(90, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
            }
        }
    }

}
