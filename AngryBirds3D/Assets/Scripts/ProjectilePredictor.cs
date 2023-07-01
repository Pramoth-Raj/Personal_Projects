using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ProjectilePredictor : MonoBehaviour
{
    [SerializeField] private GameObject inputManager;
    [SerializeField] private GameObject elasticBand;
    [SerializeField] private GameObject projectileMarkerPrefab;
    private GameObject[] markerPrefabs = new GameObject[8];
    InputManager inputManagerref;
    BandTransformation bandTransformationref;
    private Vector3 velocityVector;
    private float xComponent;
    private float yComponent;
    private float zComponent;
    private float markerSpacing=3;
    private float time;
    private float gravitationalAccelaration=5;
    private Vector3 markerPosition;
    private void Awake()
    {
        inputManagerref = inputManager.GetComponent<InputManager>();
        bandTransformationref = elasticBand.GetComponent<BandTransformation>();
        Physics.gravity=new Vector3(0f,-gravitationalAccelaration,0f);
    }
   
    private void Update()
    {
        DestroyMarkers();
        SpawnMarkers();
    }
    private Vector3 GetProjectileMarkerPosition(int i)
    {
        velocityVector = inputManagerref.GetLaunchDirection() * bandTransformationref.GetVelocityMagnitude();
        xComponent =markerSpacing* (i + 1) * inputManagerref.GetLaunchVector().x;
        zComponent =markerSpacing* (i + 1) * inputManagerref.GetLaunchVector().z;
        time = zComponent / velocityVector.z;
        yComponent = velocityVector.y * time -  gravitationalAccelaration * time * time/2;
        markerPosition = elasticBand.transform.position - 0.4f * elasticBand.transform.forward + new Vector3(xComponent,yComponent,zComponent);
    
        return markerPosition;
    }
    private void SpawnMarkers()
    {

        if (inputManagerref.GetIsLaunched() == false && inputManagerref.GetPullStrength() > 0.1)
        {
            markerSpacing = Mathf.Lerp(0.2f, 3, 1 - (1 / (1 + inputManagerref.GetPullStrength() * inputManagerref.GetPullStrength())));
            for (int i = 0; i < 8; i++)
            {
                markerPrefabs[i] = Instantiate(projectileMarkerPrefab, GetProjectileMarkerPosition(i), Quaternion.identity);
            }
        }
    }
    private void DestroyMarkers()
    {
        for (int i = 0; i < 8; i++)
        {
            if (markerPrefabs[i] != null)
            {
                Destroy(markerPrefabs[i]);
            }
        }
    }
    
}
