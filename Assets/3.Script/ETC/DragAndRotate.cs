using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndRotate : MonoBehaviour
{
    private bool rotating;
    private float rotateSpeed = 30.0f;
    private Vector3 mousePos;
    private Vector3 offset;
    private Vector3 rotation;
    // Start is called before the first frame update
    void OnMouseDown()
    {
        rotating = true;

        mousePos = Input.mousePosition;
    }

    void OnMouseUp()
    {
        rotating = false;
    }
    void Update()
    {
        if (rotating)
        {
            offset = (Input.mousePosition - mousePos);

            rotation.y = -(offset.x + offset.y) * Time.deltaTime * rotateSpeed;

            transform.Rotate(rotation);

            mousePos = Input.mousePosition;
        }
    }
}
