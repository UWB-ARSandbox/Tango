using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class virtualCameraMovement : MonoBehaviour
{
    /// <summary>
    /// this Virtual Camera Movement script is mean't for the Android Version of the UWB UR SYSTEM.
    /// 
    /// This script takes input from a phone's multiple sensors (gyroscope, accelerometer, etc.) and 
    /// translates those movements into virtual camera movement in 3D space
    /// </summary>
    /// 
    // Use this for initialization

    public TextMesh debugOutput;
    //private Gyroscope gyro;
    //private bool gyroInitialized = false;
    private bool hasGyroSensor = false;

    double startUpTimeInSeconds = 2;
    bool NotStarted = true;

    void Start()
    {
        //Check if this android device has a gyroscope sensor
        hasGyroSensor = SystemInfo.supportsGyroscope;

        //DO A CHECK FOR HASGYRO OR NOT
        if (hasGyroSensor)
        {
            //manually activate the gyroscope
            //initialize the gyroscope
            // enable the gyroscope
            Input.gyro.enabled = true;
            //gyroInitialized = true;

            // set the update interval to it's highest value (60 Hz)
            Input.gyro.updateInterval = 0.0167f;

            debugOutput.text = "Gyroscope: True";
            //GuiTextDebug.debug("Gyroscope: TRUE");
        }
        else
        {
            debugOutput.text = "Gyroscope: FALSE";
            //GuiTextDebug.debug("Gyroscope: FALSE");
        }

        //initialize the gyroscope
        // enable the gyroscope
        //Input.gyro.enabled = true;
        //gyroInitialized = true;

        

    }

    // Update is called once per frame
    void Update()
    {
        //DON'T USE THIS FOR PHYSICS OR CAMERA MOVEMENT!!!!
        if (NotStarted)
        {
            if (startUpTimeInSeconds > 0)
            {
                startUpTimeInSeconds -= Time.deltaTime;
            }
            else { NotStarted = false; }
        }
    }

    //Fixed update is set of a fixed interval, USE THIS FOR CAMERA MOVEMENTS
    void FixedUpdate()
    {
        //Match the transform of this camera with the rotation of the yroscope
        //Input.gyro.enabled = true;
        if (NotStarted == false)
        {
            Quaternion current = this.readGyroscopeRotation();
            this.GetComponent<Transform>().rotation = current;
            debugOutput.text = "Gyroscope: True, " + current.ToString();
            //GuiTextDebug.debug("Gyroscope: True, " + current.ToString());
        }
    }


    private Quaternion readGyroscopeRotation()
    {
        //READ NEW ROTATION FROM GYROSCOPE SENSOR
        //return new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * 
        //    Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
        return (new Quaternion(0.5f, 0.5f, -0.5f, 0.5f)) * Input.gyro.attitude * (new Quaternion(0, 0, 1, 0));
    }

}
