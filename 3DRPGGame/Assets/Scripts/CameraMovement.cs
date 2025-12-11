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

    [Header("Lock On")]
    public float lockOnRadius = 15f;
    public LayerMask lockOnLayer;
    private Transform lockOnTarget;
    public bool IsLockOn
    {
        get
        {
            if(lockOnTarget != null)
            {
                return true;
            }
            else
            {
                 return false;

            }
        }
        
    }


    public void ToggleLockOn(Transform player)
    {
        if(IsLockOn)
        {
            //락온 해제
            lockOnTarget = null;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(player.position, lockOnRadius, lockOnLayer);

        if(hits.Length > 0)
        {
            //가장 가까운 적 찾기
            float closestDistance = Mathf.Infinity;

            foreach(var hit in hits)
            {
                float distance = Vector3.Distance(player.position, hit.transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    lockOnTarget = hit.transform;
                }
            }
        }
    }


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
        if (!IsLockOn)
        {
            rotX += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            rotY += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

            Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
            transform.rotation = rot;
        }
        else if(lockOnTarget != null)
        {
            Vector3 dir = lockOnTarget.position - transform.position;
            dir.y = 0; // 수직 회전 방지

            if ((dir.sqrMagnitude > 0.001f))
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,  followSpeed * Time.deltaTime);

                Vector3 euler = transform.rotation.eulerAngles;
                rotX = Mathf.Clamp(euler.x, -clampAngle, clampAngle);
                rotY = euler.y;
            }
        }

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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = (objectToFollow != null) ? objectToFollow.position : transform.position;
        Gizmos.DrawWireSphere(center, lockOnRadius);
    }
}
