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
    public float[] gears = { -10f, 9f, 6f, 4.5f, 3f, 2.5f };
    public WheelData[] wheels;

    [Header("Physics settings")]
    public GameObject com;

    [Header("Debug information")]
    public ControlInput cInput; // Receive inputs from a separate script
    public EngineData engine;
    public int currentGear = 1;

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
        if (Input.GetKeyDown("page up"))
        {
            ShiftUp();
        }
        if (Input.GetKeyDown("page down"))
        {
            ShiftDown();
        }

        Debug.Log(rb.velocity.magnitude);
    }

    // handle shifting a gear up
    public void ShiftUp()
    {
        // check if we can shift up
        if (currentGear < gears.Length - 1)
        {
            currentGear++;
        }
    }

    // handle shifting a gear down
    public void ShiftDown()
    {
        // check if we can shift down (note gear 0 is reverse)
        if (currentGear > 0)
        {
            currentGear--;
        }
    }

    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        float rpm = 0f;
        int motorizedWheels = 0;

        foreach (WheelData wheel in wheels) // Framerate-sensitive wheel updates.
        {
            // variable = (condition) ? consequenceValue : alternativeValue;
            wheel.collider.steerAngle = (wheel.steering) ? cInput.steering * maxSteeringAngle : 0f;
            wheel.collider.brakeTorque = (wheel.handbrake && cInput.handbrake) ? maxHandbrakeForce : 0f;
            if (cInput.throttle < 0.0f)
            {
                // if we try to decelerate we brake.
                wheel.collider.brakeTorque = -cInput.throttle * maxBrakeForce;
                cInput.throttle = 0.0f;
                engine.wantedRPM = 0.0f;
            }

            if (wheel.motor)
            {
                rpm += wheel.collider.rpm;
                motorizedWheels++;
            }
        }

        // RPM calculation method from JCar. See https://wiki.unity3d.com/index.php/JCar
        engine.wantedRPM = (engine.maxRPM * cInput.throttle) * 0.1f + engine.wantedRPM * 0.9f;

        if (motorizedWheels > 1)
        {
            rpm = rpm / motorizedWheels;
        }

        engine.motorRPM = 0.95f * engine.motorRPM + 0.05f * Mathf.Abs(rpm * gears[currentGear]);
        if (engine.motorRPM > 5500.0f) engine.motorRPM = 5500.0f;

        // calculate the 'efficiency' (low or high rpm have lower efficiency then the
        // ideal efficiency, say 2000RPM, see table
        int efficiencyIndex = (int)(engine.motorRPM / engine.efficiencyTableStep);
        if (efficiencyIndex >= engine.efficiencyTable.Length) efficiencyIndex = engine.efficiencyTable.Length - 1;
        if (efficiencyIndex < 0) efficiencyIndex = 0;

        float newTorque = engine.torque * gears[currentGear] * engine.efficiencyTable[efficiencyIndex];

        foreach (WheelData w in wheels)
        {
            WheelCollider col = w.collider;

            // of course, only the wheels connected to the engine can get engine torque
            if (w.motor)
            {
                // only set torque if wheel goes slower than the expected speed
                if (Mathf.Abs(col.rpm) > Mathf.Abs(engine.wantedRPM))
                {
                    // wheel goes too fast, set torque to 0
                    col.motorTorque = 0;
                }
                else
                {
                    // 
                    float curTorque = col.motorTorque;
                    col.motorTorque = curTorque * 0.9f + newTorque * 0.1f;
                }
            }
        }
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
    [Range(-1f, 1f)] public float throttle;
    [Range(0f, 1f)] public float brake;
    public bool handbrake;
}

[System.Serializable]
public class EngineData
{
    public float torque = 150;
    public float motorRPM;
    public float maxRPM = 5500f;
    public float wantedRPM;

    // table of efficiency at certain RPM, in tableStep RPM increases, 1.0f is 100% efficient
    // at the given RPM, current table has 100% at around 2000RPM
    public float[] efficiencyTable = { 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 1.0f, 1.0f, 0.95f, 0.80f, 0.70f, 0.60f, 0.5f, 0.45f, 0.40f, 0.36f, 0.33f, 0.30f, 0.20f, 0.10f, 0.05f };

    // the scale of the indices in table, so with 250f, 750RPM translates to efficiencyTable[3].
    public float efficiencyTableStep = 250.0f;
}
