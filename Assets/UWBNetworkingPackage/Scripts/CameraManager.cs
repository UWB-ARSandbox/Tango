using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tango;

public class CameraManager : MonoBehaviour
{
    //Enums for camera types
    private const int ARCameraEnum = 0;
    private const int VRCameraEnum = 1;

    private bool isARMode = true;
    private bool isVRMode = false;
    
    //reference to the cameras
    public GameObject VRCamera;
    public GameObject ARCamera;

    //Enums for control type
    public const int phoneControlMode = 0;
    public const int gazeControlMode = 1;
    public const int experimentalMode = 2;

    //boolean flags for control types
    private bool isPhoneControlMode = false;
    private bool isGazeControlMode = false;
    private bool isExperimentalControlMode = false;

    //boolean flags for object manipulations
    private bool objectIsSelected = false;

 
    //Reference to display Text for displaying information
    public Text infoDisplay;

    //Reference to debug output text to know what selection mode is currently being selected
    public Text currentSelectionDisplay;

    //float to denote how far the object should be placed in front of the user when an object is selected via experimental control mode 
    public float objectDistanceDuringSelection = .5f;

    //used for standard phone controls-------------------------------------
    RaycastHit touchHit;
    Vector3 objCenter;
    Vector3 touchPosition;
    Vector3 offset;
    bool draggingMode = false;
    public GameObject scrollPlane;

    //used to hold a reference to the selected object
    private GameObject selectedItem;
    private Color originalObjectColor;

    //the touch screen space of the mobile device 
    //use this to avoid touch activatation when user accidentally touches edge of screen
    private Rect activeTouchSpace = new Rect(100,100,Screen.width - 200,Screen.height - 200);

    // Use this for initialization
    void Start ()
    {
        //Set the VR camera to off so that it doesn't conflict with the AR camera
        VRCamera.SetActive(false);

        //Clear the information display area
        infoDisplay.text = "";

        //launch the program with standard phone controls
        this.SwitchControlMode(phoneControlMode);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Raycast hit to see what the user is looking at
        RaycastHit hit;
        Ray ray;

        if (isARMode)
        {
            //create new ray in the directon the user is looking based on the AR Camera
            ray = new Ray(ARCamera.transform.position, ARCamera.transform.forward);
        }
        else
        {
            //create new ray in the directon the user is looking based on the VR Camera
            ray = new Ray(VRCamera.transform.position, VRCamera.transform.forward);
        }

        //standard controls don't use raycasting from camera to center spot
        if (isPhoneControlMode)
        {
            //string holder for object that was touched
            string touchObject;

            for (int i = 0; i < Input.touchCount; i++) //go through all touches
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    //cast from camera to the position of the finger touch on screen
                    Ray touchRay = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);

                    if (Physics.SphereCast(touchRay, 0.01f, out touchHit)) //if user touches something with finger
                    {
                        //BAD WAY TO DO THIS FOR EFFICEINTY....BUT GOOD FOR SPEED OF IMPLEMENTATION...
                        //string touchObjectName = touchHit.collider.gameObject.name;
                        touchObject = touchHit.collider.gameObject.name;

                        selectedItem = GameObject.Find(touchObject);

                        originalObjectColor = new Color(selectedItem.GetComponent<MeshRenderer>().material.color.r,
                            selectedItem.GetComponent<MeshRenderer>().material.color.g,
                            selectedItem.GetComponent<MeshRenderer>().material.color.b,
                            selectedItem.GetComponent<MeshRenderer>().material.color.a);

                        selectedItem.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, .1f);

                        //Unlock the constraints CANNOT USE GRAVITY HERE-----------------------------------------
                        selectedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;


                        //get the position of the object that was touched
                        //objCenter = selectedItem.transform.position;
                        //touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                        //offset = touchPosition - objCenter;

                        //set the plane so that it's perpendicular to the camera
                        scrollPlane.transform.position = selectedItem.transform.position;
                        scrollPlane.transform.LookAt(ARCamera.transform.position);
                        //scrollPlane.transform.forward = Camera.main.transform.forward;
                        //scrollPlane.transform.Rotate(scrollPlane.transform.up, 90);

                        scrollPlane.GetComponent<BoxCollider>().enabled = true;
                        scrollPlane.GetComponent<MeshRenderer>().enabled = true;

                        draggingMode = true;
                        objectIsSelected = true;
                    }
                }

                if (Input.GetTouch(i).phase == TouchPhase.Moved) //user drags their finger across the screen
                {
                    if (draggingMode)
                    {
                        //////touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                        ////////Vector3 newObjCenter = touchPosition - offset;
                        ////////selectedItem.transform.position = new Vector3(newObjCenter.x, newObjCenter.y, newObjCenter.z);
                        //////selectedItem.transform.position = touchPosition;

                        Ray touchRay = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);

                        if (Physics.Raycast(touchRay,out touchHit, 10f, 1 << LayerMask.NameToLayer("scrollPlane"))) //it hit the plane
                        {
                            selectedItem.transform.position = touchHit.transform.position;
                        }
                    }
                }


                if (Input.GetTouch(i).phase == TouchPhase.Ended) //user lifts up their finger
                {
                    //Recolor the shader to the way it was
                    selectedItem.GetComponent<MeshRenderer>().material.color = originalObjectColor;

                    //set off the plane
                    
                    scrollPlane.GetComponent<BoxCollider>().enabled = false;
                    scrollPlane.GetComponent<MeshRenderer>().enabled = false;
                    scrollPlane.transform.position = new Vector3(100, 100, 100);
                    ////lock the positions
                    //selectedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

                    draggingMode = false;

                    //desyn the object to be in place and no longer in front of the camera
                    selectedItem = null;

                    objectIsSelected = false;

                }
            }
        }



        //IF THE RAYCAST FROM THE CAMERA CENTER HIT SOMETHING
        if (Physics.Raycast(ray, out hit))
        {
            string objectName = hit.collider.gameObject.name;

            //set the name of the display text to the object name
            infoDisplay.text = objectName;

            //if (isGazeControlMode)
            if(isExperimentalControlMode)
            {

            }
            //else if (isExperimentalControlMode)//------------------------------------------------controls for experimental mode
            else if (isGazeControlMode)
            {
                //check if the user is touching the screen
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (activeTouchSpace.Contains(Input.GetTouch(i).position))
                    {

                        if (Input.GetTouch(i).phase == TouchPhase.Began)
                        {
                            //check if the user has an object already selected
                            if (objectIsSelected)
                            {
                                //user already has an object selected, so we assume they want to set the object down

                                UnityEngine.Debug.Log("object is DE-selected");

                                //Recolor the shader to the way it was
                                selectedItem.GetComponent<MeshRenderer>().material.color = originalObjectColor;

                                //////lock the positions
                                ////selectedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;


                                //desyn the object to be in place and no longer in front of the camera
                                selectedItem = null;

                                objectIsSelected = false;
                            }
                            else //no object has been selected, place the focused object half a meter from the camera
                            {
                                //turn off the 


                                UnityEngine.Debug.Log("object selected");

                                //BAD WAY TO DO THIS FOR EFFICEINTY....BUT GOOD FOR SPEED OF IMPLEMENTATION...
                                //string touchObjectName = touchHit.collider.gameObject.name;
                                selectedItem = GameObject.Find(objectName);

                                originalObjectColor = new Color(selectedItem.GetComponent<MeshRenderer>().material.color.r,
                                    selectedItem.GetComponent<MeshRenderer>().material.color.g,
                                    selectedItem.GetComponent<MeshRenderer>().material.color.b,
                                    selectedItem.GetComponent<MeshRenderer>().material.color.a);

                                selectedItem.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, .1f);

                                //Unlock the constraints CANNOT USE GRAVITY HERE-----------------------------------------
                                selectedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

                                objectIsSelected = true;
                            }

                        }
                        else //the user is not touching the screen
                        {

                        }
                    }
                }
            }//-----------------------------------------------------------------------end of experimental controls
            else
            {
                UnityEngine.Debug.Log("ERROR during USING CONTROLS, Controls not properly set");
            }
        }
        else // the ray cast did not hit anything
        {
            infoDisplay.text = "";
        }





        //////SHIFT THE FOCUSED OBJECT while using Experimental control mode----------------------------------------------------------
        //////if ((selectedItem != null) && (objectIsSelected) && isExperimentalControlMode)
        ////if ((selectedItem != null) && (objectIsSelected) && isGazeControlMode)
        ////    {
        ////    if (isARMode)
        ////    {
        ////        selectedItem.transform.position = ARCamera.transform.position + (ARCamera.transform.forward * objectDistanceDuringSelection);

        ////        //make the object look at the current camera
        ////        selectedItem.transform.LookAt(ARCamera.transform);
        ////    }
        ////    else
        ////    {
        ////        selectedItem.transform.position = VRCamera.transform.position + (VRCamera.transform.forward * objectDistanceDuringSelection);

        ////        //make the object look at the correct current camera
        ////        selectedItem.transform.LookAt(VRCamera.transform);
        ////    }
        ////}
    }


    //LATE UPDATE DRAWS LAST TO AVOID THE FLICKERING ISSUE
    private void LateUpdate()
    {
        //SHIFT THE FOCUSED OBJECT while using Experimental control mode----------------------------------------------------------
        //if ((selectedItem != null) && (objectIsSelected) && isExperimentalControlMode)
        if ((selectedItem != null) && (objectIsSelected) && isGazeControlMode)
        {
            if (isARMode)
            {
                selectedItem.transform.position = ARCamera.transform.position + (ARCamera.transform.forward * objectDistanceDuringSelection);

                //make the object look at the current camera
                selectedItem.transform.LookAt(ARCamera.transform);
            }
            else
            {
                selectedItem.transform.position = VRCamera.transform.position + (VRCamera.transform.forward * objectDistanceDuringSelection);

                //make the object look at the correct current camera
                selectedItem.transform.LookAt(VRCamera.transform);
            }
        }
    }



    //public function for the "Switch AR/VR Mode Button"
    public void flipARVRCameraMode()
    {
        if (isARMode) //switch to VR mode
        {
            this.SwitchCameraMode(VRCameraEnum);
        }
        else //switch to AR mode
        {
            this.SwitchCameraMode(ARCameraEnum);
        }
    }

    //Helper function for flipping the camera mode for the function (flipARVRCameraMode())
    private void SwitchCameraMode(int cameraEnum)
    {
        if ((cameraEnum == ARCameraEnum) && (isARMode == false))
        {
            if (isVRMode)
            {
                //Transform curPosition = VRCamera.transform;

                //Set the position of the ARCamera the same as the VR Camera
                ARCamera.transform.position = new Vector3(VRCamera.transform.position.x, VRCamera.transform.position.y, VRCamera.transform.position.z);
                ARCamera.transform.rotation = new Quaternion(VRCamera.transform.rotation.x, VRCamera.transform.rotation.y, VRCamera.transform.rotation.z, VRCamera.transform.rotation.w);

                //set the camera's on and off
                VRCamera.SetActive(false);
                ARCamera.SetActive(true);

                //set the flags accordingly
                isVRMode = false;
                isARMode = true;
            }
        }
        else if ((cameraEnum == VRCameraEnum) && (isVRMode == false))
        {
            if (isARMode)
            {
                //Set the position of the ARCamera the same as the VR Camera
                VRCamera.transform.position = new Vector3(ARCamera.transform.position.x, ARCamera.transform.position.y, ARCamera.transform.position.z);
                VRCamera.transform.rotation = new Quaternion(ARCamera.transform.rotation.x, ARCamera.transform.rotation.y, ARCamera.transform.rotation.z, ARCamera.transform.rotation.w);

                //set the camera's on and off
                ARCamera.SetActive(false);
                VRCamera.SetActive(true);

                //set the flags accordingly
                isARMode = false;
                isVRMode = true;
            }
        }
        else
        {
            UnityEngine.Debug.Log("ERROR in switching camera mode, unknown camera enum found");
        }

    }


    public void SwitchControlMode(int controlEnum)
    {
        if ((controlEnum == phoneControlMode) && (isPhoneControlMode == false))
        {
            isPhoneControlMode = true;
            isGazeControlMode = false;
            isExperimentalControlMode = false;
            currentSelectionDisplay.text = "obj. manipulation mode:\n - standardPhoneControls";
            currentSelectionDisplay.color = Color.green;
        }
        else if ((controlEnum == gazeControlMode) && (isGazeControlMode == false))
        {
            isGazeControlMode = true;
            isPhoneControlMode = false;
            isExperimentalControlMode = false;
            currentSelectionDisplay.text = "obj. manipulation mode:\n - AR/VR Gaze Controls";
            currentSelectionDisplay.color = Color.blue;
        }
        else if ((controlEnum == experimentalMode) && (isExperimentalControlMode == false))
        {
            isExperimentalControlMode = true;
            isPhoneControlMode = false;
            isGazeControlMode = false;
            currentSelectionDisplay.text = "obj. manipulation mode:\n - ExperimentalAugControls";
            currentSelectionDisplay.color = Color.red;
        }
        else
        {
            UnityEngine.Debug.Log("ERROR during changing control modes: bad enum found or already on proper controls");
        }
    }


   

 



}
