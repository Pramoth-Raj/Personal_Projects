using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private float pullStrength = 0f;
    private float pullSpeed = 1.2f;
    private Vector3 launchVector;
    [SerializeField] private GameObject redBirdPrefab;
    [SerializeField] private GameObject elasticBand;
    [SerializeField] private GameObject PauseMenu;
    public bool inputsEnabled = true;
    private float delay = 7f;
    private bool isLaunched = false;
    public GameObject lastInstanceRedBird;
    [SerializeField] private Camera mainCamera;
    private Vector3 mousePosition;
    private Vector3 radialMousePosition;
    BandTransformation elasticBandref;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float maxRadius;
    private void Awake()
    {
        elasticBandref=elasticBand.GetComponent<BandTransformation>();
        InstantiatePrefab();
    }

    private void Update()
    {
        if (inputsEnabled)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (pullStrength <= 3)
                {
                    pullStrength += Time.deltaTime*pullSpeed;
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPullStrength();
                DestroyLastPrefab();
                InstantiatePrefab();
            }
            if (Input.GetKeyDown(KeyCode.E) && lastInstanceRedBird!=null)
            {
                if (pullStrength > 0.1)
                {
                    DisableInputs();
                    isLaunched = true;
                    EnableInputsAfterDelay();
                } 
                
            }
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
            {
                mousePosition = raycastHit.point;
            }
            radialMousePosition = mousePosition - elasticBand.transform.position;
            if (radialMousePosition.magnitude >= maxRadius)
            {
                radialMousePosition = maxRadius * radialMousePosition.normalized;
            }
            launchVector = new Vector3(-radialMousePosition.x, -radialMousePosition.y, 1.2f).normalized;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.SetActive(true);
            DisableInputs();
        }
        


    }
    public float GetPullStrength()
    {
        return pullStrength;
    }
    public Vector3 GetLaunchVector()
    {
        return launchVector;
    }
    void DisableInputs()
    {
        inputsEnabled = false;
    }
    public void EnableInputs()
    {
        inputsEnabled = true;
    }

    void EnableInputsAfterDelay()
    {
        Invoke("EnableInput", delay);
        Invoke("ResetPullStrength", delay);
        Invoke("LaunchReset", delay);
        Invoke("DestroyLastPrefab", delay);
        Invoke("ResetT", delay);
        Invoke("ResetIsOnFirst", delay);
        Invoke("NullLastInstance", delay);
    }
    void ResetPullStrength()
    {
        pullStrength = 0;
    }
    void LaunchReset()
    {
        isLaunched = false;
    }
    public bool GetIsLaunched()
    {
        return isLaunched;
    }
    public Vector3 GetMousePosition()
    {
        return mousePosition;
    }
    public Vector3 GetRadialMousePosition()
    {
        return radialMousePosition;
    }
    public Vector3 GetLaunchDirection()
    {
        return launchVector;
    }
    private void DestroyLastPrefab()
    {
        if (lastInstanceRedBird != null && lastInstanceRedBird.transform.position.z <= elasticBand.transform.position.z)
        {
            Destroy(lastInstanceRedBird);
        }
    }
    private void ResetT()
    {
        elasticBandref.ResetTime();
    }
    private void ResetIsOnFirst()
    {
        elasticBandref.isOnFirst = true;
    }
    private void InstantiatePrefab()
    {
        lastInstanceRedBird = Instantiate(redBirdPrefab, elasticBand.transform.position - 0.4f * elasticBand.transform.forward, elasticBand.transform.rotation);
        elasticBandref.redBirdRB=lastInstanceRedBird.GetComponent<Rigidbody>();
    }
    private void NullLastInstance()
    {
        lastInstanceRedBird = null;
        elasticBandref.redBirdRB = null;
    }
    private void EnableInput()
    {
        if (PauseMenu.activeSelf==false)
        {
            inputsEnabled = true;
        }
    }
    
}
