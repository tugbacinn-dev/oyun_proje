using UnityEngine;

namespace PatininIzinde.Characters
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonCameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float lookSensitivity = 2.2f;
        [SerializeField] private bool useGravity;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float minLookAngle = -70f;
        [SerializeField] private float maxLookAngle = 75f;
        [SerializeField] private KeyCode cursorToggleKey = KeyCode.Escape;

        private CharacterController controller;
        private float verticalLook;
        private float verticalVelocity;
        private bool cursorLocked = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraPivot == null && Camera.main != null)
            {
                cameraPivot = Camera.main.transform;
            }
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(cursorToggleKey))
            {
                SetCursorLocked(!cursorLocked);
            }

            if (cursorLocked)
            {
                Look();
            }

            Move();
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            verticalLook = Mathf.Clamp(verticalLook - mouseY, minLookAngle, maxLookAngle);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(verticalLook, 0f, 0f);
            }
        }

        private void Move()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                vertical += 1f;
            }

            Vector3 input = new Vector3(horizontal, 0f, vertical);
            input = Vector3.ClampMagnitude(input, 1f);

            Vector3 movement = transform.TransformDirection(input) * moveSpeed;

            if (!useGravity)
            {
                verticalVelocity = 0f;
                movement.y = 0f;
                controller.Move(movement * Time.deltaTime);
                return;
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            movement.y = verticalVelocity;

            controller.Move(movement * Time.deltaTime);
        }

        private void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
