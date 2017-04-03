using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class accessAndroidCamera : MonoBehaviour
{
    WebCamTexture cameraFeed;
    private Material renderImage;
	// Use this for initialization
	void Start ()
    {
        cameraFeed = new WebCamTexture();
        //GetComponent<Camera>().targetTexture = cameraFeed;
        //GetComponent<SpriteRenderer>().material.mainTexture = cameraFeed;
        //GetComponent<Renderer>().material.mainTexture = cameraFeed;
        renderImage.mainTexture = cameraFeed;
        //GetComponent<GUITexture>().texture = (RenderTexture)renderImage.mainTexture;

        //TEST FOR A PLANE
        GetComponent<Renderer>().material.mainTexture = (RenderTexture)renderImage.mainTexture;


        cameraFeed.Play();
	}
	
	// Update is called once per frame
	void Update ()
    {

    }
}
