using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBoundaries : MonoBehaviour
{
    [SerializeField] bool doDrawGizmos = true;

    [SerializeField] Transform background;
    [SerializeField] Transform topTransform;
    [SerializeField] Transform rightTransform;
    [SerializeField] Transform bottomTransform;
    [SerializeField] Transform leftTransform;

    Camera cam;
    Transform cameraTransform;
    Vector3 prevPosCamera;

    Vector3 top;
    Vector3 right;
    Vector3 bottom;
    Vector3 left;
    float height;
    float width;

    void OnDrawGizmos()
    {
        if (doDrawGizmos)
        {
            UpdateValues();
            Gizmos.color = Color.white;
            Gizmos.DrawCube(top, new Vector3(width, 0.1f, 0));
            Gizmos.DrawCube(right, new Vector3(0.1f, height, 0));
            Gizmos.DrawCube(bottom, new Vector3(width, 0.1f, 0));
            Gizmos.DrawCube(left, new Vector3(0.1f, height, 0));
        }
    }

    void OnValidate()
    {
        cam = Camera.main;
        cameraTransform = cam.transform;
    }

    void FixedUpdate()
    {
        // if (prevPosCamera != cameraTransform.position)
        // {
        UpdateValues();
        AssignToObjects();
        // }
    }

    void UpdateValues()
    {
        Vector3 tl = cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight, 0));
        Vector3 tr = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        Vector3 br = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0, 0));
        Vector3 bl = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        height = tl.y - bl.y;
        width = tr.x - tl.x;

        top = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth / 2, cam.pixelHeight, 0));
        top.z = 0;
        right = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight / 2, 0));
        right.z = 0;
        bottom = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth / 2, 0, 0));
        bottom.z = 0;
        left = cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight / 2, 0));
        left.z = 0;
    }

    void AssignToObjects()
    {
        topTransform.transform.localScale = new Vector3(width, 0.01f, 1);
        topTransform.transform.position = top;

        // assign right
        rightTransform.transform.localScale = new Vector3(0.01f, height, 1);
        rightTransform.transform.position = right;

        // assign bottom
        leftTransform.transform.localScale = new Vector3(0.01f, height, 1);
        bottomTransform.transform.position = bottom;

        // assign left
        bottomTransform.transform.localScale = new Vector3(width, 0.01f, 1);
        leftTransform.transform.position = left;

        var size = height > width ? height : width;
        background.localScale = size * 1.1f * Vector3.one;
    }
}
