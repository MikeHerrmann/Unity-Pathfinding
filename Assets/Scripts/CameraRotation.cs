using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {

    public Camera cam;

    float currentMouseX = 0f;
    float oldMouseX = 0f;
    float currentMouseY = 0f;
    float oldMouseY = 0f;

    float rotationFactor = 3f;
    float zoomFactor = 60f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(2))
        {
            currentMouseX = Input.mousePosition.x;
            oldMouseX = currentMouseX;
            currentMouseY = Input.mousePosition.y;
            oldMouseY = currentMouseY;
        }

        //-------------------------------

            if (Input.GetMouseButton(2))
        {
            currentMouseX = Input.mousePosition.x;
            float diffMouseX = currentMouseX - oldMouseX;
            transform.Rotate(Vector3.up * Time.deltaTime * diffMouseX * rotationFactor, Space.World);
            oldMouseX = currentMouseX;
            
            currentMouseY = Input.mousePosition.y;
            float diffMouseY = oldMouseY - currentMouseY;
            transform.Rotate(Vector3.right * Time.deltaTime * diffMouseY * rotationFactor, Space.Self);
            oldMouseY = currentMouseY;

        }
            cam.transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomFactor);
    }
}
