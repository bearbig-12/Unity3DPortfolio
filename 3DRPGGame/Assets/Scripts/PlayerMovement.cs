using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Animator _animator;
    Camera _camera;
    CharacterController _characterController;

    public float speed = 5.0f;
    public float runSpeed = 10.0f;
    public float finalSpeed;
    public bool isRunning;

    // 배그에서 토글 카메라 회전 기능
    public bool toggleCameraRotation;
    public float smoothness = 10f;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main;
        _characterController = this.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftAlt))
        {
            toggleCameraRotation = true;
        }
        else
        {
            toggleCameraRotation = false;
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
        InputMovement();
    }
    void LateUpdate()
    {
        if(toggleCameraRotation != true)
        {
            Vector3 playerRotate = Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate), Time.deltaTime * smoothness);

        }
    }

    void InputMovement()
    {
        finalSpeed = isRunning ? runSpeed : speed;

        Vector3 forword = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 moveDirection = forword * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");

        _characterController.Move(moveDirection.normalized * finalSpeed * Time.deltaTime);

        float percent = (isRunning ? 1.0f : 0.5f) * moveDirection.magnitude;
        _animator.SetFloat("Blend", percent, 0.1f, Time.deltaTime);
    }
}
