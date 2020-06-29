using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car settings")]
    public float maxSteeringAngle = 30f;
    public float maxEngineTorque = 700f;
    public float maxBrakeForce = 1500f;
    public float maxHandbrakeForce = 1500f;
    public WheelData[] wheels;

    [Header("Physics settings")]
    public GameObject com;

    [Header("Debug information")]
    public ControlInput cInput; // Receive inputs from a separate script
    public EngineData engine;

    private Vector3 wheelPosition;
    private Quaternion wheelRotation;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (com)
        {
            rb.centerOfMass = com.transform.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (WheelData wheel in wheels)
        {
            // Update visual wheels. Doesn't have to be in FixedUpdate() because it's just the visual stuff.
            wheel.collider.GetWorldPose(out wheelPosition, out wheelRotation);

            wheel.wheelModel.position = wheelPosition;
            wheel.wheelModel.rotation = wheelRotation;
        }
    }

    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        
        foreach (WheelData wheel in wheels) // Framerate-sensitive WheelData updates.
        {
            // variable = (condition) ? consequenceValue : alternativeValue;
            wheel.collider.motorTorque = (wheel.motor) ? cInput.throttle * maxEngineTorque : 0f;
            wheel.collider.steerAngle = (wheel.steering) ? cInput.steering * maxSteeringAngle : 0f;
            wheel.collider.brakeTorque = (wheel.handbrake && cInput.handbrake) ? maxHandbrakeForce : 0f;
        }

        engine.wantedRPM = (5500f * cInput.throttle) * 0.1f + engine.wantedRPM * 0.9f;
    }
}

[System.Serializable]
public struct WheelData // Stolen codes
{
    public WheelCollider collider;
    public Transform wheelModel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
    public bool handbrake;
}

[System.Serializable] // Serialized for input override from inspector
public struct ControlInput
{
    [Range(-1f, 1f)] public float steering;
    [Range(0f, 1f)] public float throttle;
    [Range(0f, 1f)] public float brake;
    public bool handbrake;
}

[System.Serializable]
public struct EngineData
{
    public float RPM;
    public float wantedRPM;
}
