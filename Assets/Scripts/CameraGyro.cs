using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraGyro : MonoBehaviour
{
    Quaternion offset;

    void Start()
    {
        offset = transform.rotation * Quaternion.Inverse(GyroToUnity(Input.gyro.attitude));
    }

    void Update()
    {
        transform.rotation = offset * GyroToUnity(Input.gyro.attitude);
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
