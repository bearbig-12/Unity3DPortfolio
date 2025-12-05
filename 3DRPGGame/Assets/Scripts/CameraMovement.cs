using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform objectToFollow;
    public float followSpeed = 10f;
    public float mouseSensitivity = 100f;
    public float clampAngle = 70f;

    private float rotX = 0f;
    private float rotY = 0f;

    public Transform realCamera;
    public Vector3 dirNormalized;
    public Vector3 finalDir;
    
    public float minDistance = 0.5f;
    public float maxDistance = 4f;
    public float finalDistance;
    public float smoothness = 10f;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;
        

        // Normalize initial camera direction
        dirNormalized = realCamera.localPosition.normalized;
        finalDistance = realCamera.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        rotX += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotY += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;

    }

    void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, objectToFollow.position, followSpeed * Time.deltaTime);

        finalDir = transform.TransformDirection(dirNormalized);

        RaycastHit hit;

        if (Physics.Linecast(transform.position, finalDir, out hit))
        {
            float distance = hit.distance;
            finalDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }
        realCamera.localPosition = Vector3.Lerp(realCamera.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
    }
}
