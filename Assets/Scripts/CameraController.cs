using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    static CameraController instance = null;

    [SerializeField]
    MeshFilter target;
    bool camMovedLastFrame = false;

    float distance = 20f; // !=0
    float mouseSensitivity = 0.2f;
    float scrollSensitivity = 5;
    Vector3 previousMousePosition;

    Vector3 cameraOffset = new Vector3(3, 1, 0);

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ResetCamera();
    }

    public void ResetCamera()
    {
        float startPositionOffset = distance / Mathf.Sqrt(2);
        transform.position = target.mesh.bounds.center + new Vector3(0, startPositionOffset, -startPositionOffset);
        transform.LookAt(target.mesh.bounds.center);

        transform.position += GetOffset();
    }

    Vector3 GetOffset()
    {
        return transform.right * cameraOffset.x + transform.up * cameraOffset.y + transform.forward * cameraOffset.z;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetButton("CameraEnabled")) {
            // camera movement using mouse
            if (Input.GetMouseButton(0))
            {
                if (!camMovedLastFrame) {
                    previousMousePosition = Input.mousePosition; 
                    camMovedLastFrame = true;
                }

                Vector3 mouseDirection = Input.mousePosition - previousMousePosition; // current direction. Goes towards 0 as previous position is updated
                
                if (mouseDirection.x != 0)
                {
                    transform.position -= GetOffset(); // undo offset, so the calculation here is correct.

                    Quaternion horizontalRotation = Quaternion.AngleAxis(mouseDirection.x * mouseSensitivity, Vector3.up);
                    Vector3 direction = horizontalRotation * (transform.position - target.mesh.bounds.center);

                    transform.position = target.mesh.bounds.center + direction.normalized * distance;
                    transform.LookAt(target.mesh.bounds.center);

                    transform.position += GetOffset();
                }

                if (mouseDirection.y != 0)
                {
                    transform.position -= GetOffset();

                    Quaternion verticalRotation = Quaternion.AngleAxis(-mouseDirection.y * mouseSensitivity, transform.right);
                    Vector3 direction = verticalRotation * (transform.position - target.mesh.bounds.center);

                    transform.position = target.mesh.bounds.center + direction.normalized * distance;
                    transform.LookAt(target.mesh.bounds.center);

                    transform.position += GetOffset();

                    if (transform.eulerAngles.x < 0 || transform.eulerAngles.x >= 90 - 10) /* || > 270*/
                    {
                        transform.position -= GetOffset();

                        direction = Quaternion.Inverse(verticalRotation) * (transform.position - target.mesh.bounds.center);
                        transform.position = target.mesh.bounds.center + direction.normalized * distance;
                        transform.LookAt(target.mesh.bounds.center);

                        transform.position += GetOffset();
                    }
                }

                previousMousePosition = Input.mousePosition;
            }
            else camMovedLastFrame = false;

            // camera movement using keyboard
            transform.position -= GetOffset();

            if (Input.GetButtonDown("Horizontal"))
                cameraOffset.x += Input.GetAxisRaw("Horizontal");
            if (Input.GetButtonDown("QE"))
                cameraOffset.y += -Input.GetAxisRaw("QE");
            if (Input.GetButtonDown("Vertical"))
                cameraOffset.z += Input.GetAxisRaw("Vertical");

            transform.position += GetOffset();

        }
        else camMovedLastFrame = false;

        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;

            if (distance < 1)
                distance = 1;

            transform.position -= GetOffset();

            Vector3 direction = transform.position - target.mesh.bounds.center;
            transform.position = target.mesh.bounds.center + direction.normalized * distance;

            transform.position += GetOffset();
        }
    }

    public static CameraController GetInstance() { return instance; }
}
