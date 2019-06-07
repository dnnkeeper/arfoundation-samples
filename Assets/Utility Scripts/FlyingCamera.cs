using UnityEngine;
using System.Collections;

#if CROSS_PLATFORM_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif	

namespace Utility.CameraUtil
{
    public class FlyingCamera : MonoBehaviour
    {

        public string horizontalAxisName = "Horizontal";
        public string verticalAxisName = "Vertical";
        public string forwardAxisName = "Jump";

        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;


        //bool rightMouseDown;

        float speedMod = 1f;

        public float maxSpeed = 100f;
        public float accelerationRate = 10f;
        public float decelerationRate = 10f;

        public float targetSpeed = 5f;

        Vector3 moveDirection, targetVelocity;

        Vector3 targetPosition;

        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
        }

        float x, y;

        void Update()
        {

            float deltaTime = Time.deltaTime;

            if (!enabled)
                return;

            if (Input.GetMouseButtonDown(1))
            {
                moveDirection = Vector3.zero;

                targetVelocity = Vector3.zero;
            }

            if (Input.GetMouseButton(1))
            {
                targetPosition = transform.position;

                moveDirection = Vector3.zero;

                bool move = false;

                if (Input.GetKey(KeyCode.W))
                {
                    move = true;
                    moveDirection += Vector3.forward;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    move = true;
                    moveDirection -= Vector3.forward;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    move = true;
                    moveDirection -= Vector3.right;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    move = true;
                    moveDirection += Vector3.right;
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    move = true;
                    moveDirection -= Vector3.up;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    move = true;
                    moveDirection += Vector3.up;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    speedMod = 3f;
                }
                else
                    speedMod = 1f;

                Vector3 mousePos = Input.mousePosition;
                mousePos.x = (mousePos.x / Screen.width) - 0.5f;
                mousePos.y = (mousePos.y / Screen.height) - 0.5f;

                if (Screen.fullScreen || (Input.GetMouseButton(0) && Cursor.visible))
                {
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                }

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                transform.rotation = rotation;

                if (move)
                {
                    targetSpeed = Mathf.Clamp(targetSpeed * (1f + Input.mouseScrollDelta.y * 0.05f), 0f, maxSpeed);

                    targetVelocity = Vector3.Lerp(targetVelocity, moveDirection * targetSpeed, accelerationRate * deltaTime);
                }
                else
                {
                    targetVelocity = Vector3.Lerp(targetVelocity, moveDirection * targetSpeed, decelerationRate * deltaTime);
                }

                targetPosition = transform.position + transform.TransformVector(targetVelocity * deltaTime * speedMod);

                transform.position = targetPosition;
            }
            else
            {
                moveDirection = Vector3.zero;
            }
        }


    }
}