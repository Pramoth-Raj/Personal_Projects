using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HummingbirdAgent : Agent
{
    [Tooltip("Force to apply when moving")]
    public float moveForce = 2f;
    [Tooltip("Speed to pitch up or down")]
    public float pitchSpeed = 100f;
    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;
    [Tooltip("Transform at the tip of the beak")]
    public Transform beakTip;
    [Tooltip("The agent's camera")]
    public Camera agentCamera;
    [Tooltip("whether this is training mode or gameplay mode")]
    public bool trainingMode;
    private Rigidbody rigidBody;
    private FlowerArea flowerArea;
    private Flower nearestFlower;
    private float smoothPitchChange = 0f;
    private float smoothYawChange = 0f;
    private const float MaxPitchAngle = 80f;
    private const float BeakTipRadius = 0.008f;
    private bool frozen = false;
    public float NectarObtained { get; private set; }
    public override void Initialize()
    {
        rigidBody= GetComponent<Rigidbody>();
        flowerArea=GetComponentInParent<FlowerArea>();
        if (!trainingMode)
        {
            MaxStep = 0;
        }
    }
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            flowerArea.ResetFlowers();
        }
        NectarObtained = 0f;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        bool inFrontOfFlower = true;
        if (trainingMode)
        {
            inFrontOfFlower = UnityEngine.Random.value > 0.5f;
        }
        MoveToSafeRandomPosition(inFrontOfFlower);
        UpdateNearestFlower();
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (frozen) return;
        Vector3 move = new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]);
        rigidBody.AddForce(move*moveForce);
        Vector3 rotationVector = transform.rotation.eulerAngles;
        float pitchChange = actions.ContinuousActions[3];
        float yawChange = actions.ContinuousActions[4];
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange,yawChange,2f* Time.fixedDeltaTime);
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch,-MaxPitchAngle, MaxPitchAngle);
        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;
        transform.rotation= Quaternion.Euler(pitch,yaw,0);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        if(nearestFlower == null)
        {
            sensor.AddObservation(new float[10]);
            return;
        }
        sensor.AddObservation(transform.localRotation.normalized);
        Vector3 toFlower = nearestFlower.FlowerCenterPosition - beakTip.position;
        sensor.AddObservation(toFlower.normalized);
        //Observe a dot product to find is the beak tip is in front or back
        sensor.AddObservation(Vector3.Dot(toFlower.normalized, -nearestFlower.FlowerUpVector.normalized));
        //Observe dot product to find whether the beak is pointing at the flower
        sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -nearestFlower.FlowerUpVector.normalized));
        sensor.AddObservation(toFlower.magnitude / FlowerArea.AreaDiameter);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Vector3 forward= Vector3.zero;
        Vector3 left= Vector3.zero;
        Vector3 up= Vector3.zero;
        float pitch = 0f;
        float yaw = 0f;
        if (Input.GetKey(KeyCode.W)) forward = transform.forward;
        else if(Input.GetKey(KeyCode.S)) forward = -transform.forward;
        if (Input.GetKey(KeyCode.A)) left = -transform.right;
        else if (Input.GetKey(KeyCode.D)) left = transform.right;
        if (Input.GetKey(KeyCode.E)) up = transform.up;
        else if (Input.GetKey(KeyCode.Q)) up = -transform.up;
        pitch =5f* -Input.GetAxis("Mouse Y");
        yaw =5f * Input.GetAxis("Mouse X");
        Vector3 combined = (forward + left + up).normalized;
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0]= combined.x;
        continuousActionsOut[1]= combined.y;
        continuousActionsOut[2]= combined.z;
        continuousActionsOut[3] = pitch;
        continuousActionsOut[4] = yaw;

    }
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = true;
        rigidBody.Sleep();
    }
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = false;
        rigidBody.WakeUp();
    }
    private void MoveToSafeRandomPosition(bool inFrontOfFlower)
    {
        bool safePositionFound = false;
        int attempsRemaining = 100;
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        while(!safePositionFound&& attempsRemaining > 0)
        {
            attempsRemaining--;
            if (inFrontOfFlower)
            {
                Flower randomFlower = flowerArea.Flowers[UnityEngine.Random.Range(0, flowerArea.Flowers.Count)];
                float distanceFromFlower = UnityEngine.Random.Range(0.1f, 0.2f);
                potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distanceFromFlower;
                Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toFlower,Vector3.up);
            }
            else
            {
                float height = UnityEngine.Random.Range(1.2f, 2.5f);
                float radius = UnityEngine.Random.Range(2f, 7f);
                Quaternion direction = Quaternion.Euler(0, UnityEngine.Random.Range(-180f, 180f), 0f);
                potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;
                float pitch = UnityEngine.Random.Range(-60f, 60f);
                float yaw = UnityEngine.Random.Range(-180f, 180f);
                potentialRotation=Quaternion.Euler(pitch, yaw, 0f);
            }
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);
            safePositionFound = colliders.Length == 0;
        }
        Debug.Assert(safePositionFound, "Could not find a safe position to spawn");
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }
    private void UpdateNearestFlower()
    {
        foreach(Flower flower in flowerArea.Flowers)
        {
            if(nearestFlower == null && flower.HasNectar)
            {
                nearestFlower = flower;
            }
            else if (flower.HasNectar)
            {
                float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);
                float distanceToCurrentNearestFlower = Vector3.Distance(nearestFlower.transform.position, beakTip.position);
                if (!nearestFlower.HasNectar || distanceToFlower < distanceToCurrentNearestFlower)
                {
                    nearestFlower = flower;
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        TriggerEnterOrStay(other);
    }
    private void OnTriggerStay(Collider other)
    {
        TriggerEnterOrStay(other);
    }
    private void TriggerEnterOrStay(Collider collider)
    {
        if (collider.CompareTag("nectar"))
        {
            Vector3 closestPointToBeekTip= collider.ClosestPoint(beakTip.position);
            if(Vector3.Distance(beakTip.position, closestPointToBeekTip) < BeakTipRadius)
            {
                Flower flower = flowerArea.GetFlowerDictionary(collider);
                float nectarReceived = flower.Feed(0.01f);
                NectarObtained += nectarReceived;
                if (trainingMode)
                {
                    float bonus = 0.2f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized,-nearestFlower.FlowerUpVector.normalized));
                    AddReward(0.01f + bonus);
                }
                if (!flower.HasNectar)
                {
                    UpdateNearestFlower();
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(trainingMode && collision.collider.CompareTag("boundary"))
        {
            AddReward(-0.5f);
        }
    }
    private void Update()
    {
        if(nearestFlower != null)
        {
            Debug.DrawLine(beakTip.position, nearestFlower.FlowerCenterPosition, Color.green);
        }
    }
    private void FixedUpdate()
    {
        if(nearestFlower != null && !nearestFlower.HasNectar)
        {
            UpdateNearestFlower();
        }
    }
}
