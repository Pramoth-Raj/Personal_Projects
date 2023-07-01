using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BandTransformation : MonoBehaviour
{
    [SerializeField] private GameObject inputManager;
    InputManager inputManagerref;
    private float springConstant=1000;
    public Rigidbody redBirdRB;
    private float w=10;
    private float t=0;
    public bool isOnFirst=true;
    private void Awake()
    {
        inputManagerref=inputManager.GetComponent<InputManager>();
        //w = Mathf.Pow(springConstant / redBirdMass,1/2);
    }
   
    private void Update()
    {
        if (inputManagerref.GetIsLaunched() == false)
        {
            gameObject.transform.localScale = new Vector3(1, 1, (1 + inputManagerref.GetPullStrength()));
            transform.rotation = Quaternion.LookRotation(inputManagerref.GetLaunchDirection());
            if (inputManagerref.lastInstanceRedBird != null)
            {
                inputManagerref.lastInstanceRedBird.transform.position = gameObject.transform.position - 0.4f * (1 + inputManagerref.GetPullStrength()) * gameObject.transform.forward;
                inputManagerref.lastInstanceRedBird.transform.rotation = Quaternion.LookRotation(inputManagerref.GetLaunchDirection());
            }
        }
        else
        {
            t += Time.deltaTime;
            if (isOnFirst)
            {
                if (inputManagerref.GetPullStrength() != 0 && gameObject.transform.localScale.z >1 && inputManagerref.lastInstanceRedBird!=null)
                {
                    gameObject.transform.localScale = new Vector3(1, 1, 1 + inputManagerref.GetPullStrength() * (Mathf.Cos(w*t)));
                    inputManagerref.lastInstanceRedBird.transform.position = gameObject.transform.position - 0.4f * (1 + inputManagerref.GetPullStrength() * (Mathf.Cos(w*t))) * gameObject.transform.forward;
                }
                else
                {
                    if (redBirdRB != null)
                    {
                        inputManagerref.lastInstanceRedBird.transform.position = gameObject.transform.position - 0.4f * gameObject.transform.forward;
                        redBirdRB.velocity = inputManagerref.GetLaunchDirection() * GetVelocityMagnitude();
                    }
                    
                    isOnFirst= false;
                }
            }
            inputManagerref.lastInstanceRedBird.transform.rotation = Quaternion.LookRotation(redBirdRB.velocity);
            
        }
        
    }
    public float GetSpringConstant()
    {
        return springConstant;
    }
    public float GetVelocityMagnitude()
    {
        float velocityMagnitude;
        velocityMagnitude = w * 0.4f * inputManagerref.GetPullStrength();
        return velocityMagnitude;
    }
    public void ResetTime()
    {
        t = 0;
    }
       
}
