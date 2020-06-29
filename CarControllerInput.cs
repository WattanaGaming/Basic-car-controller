using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarControllerInput : MonoBehaviour
{
    private CarController controller;
    
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CarController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        controller.cInput.steering = Input.GetAxis("Horizontal");
        controller.cInput.throttle = Input.GetAxis("Vertical");
        controller.cInput.handbrake = Input.GetKey(KeyCode.Space);
    }
}
