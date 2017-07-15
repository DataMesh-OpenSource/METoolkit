using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorDefinition : MonoBehaviour
{
    public enum LockType
    {
        None,
        Lock,
        FollowCoordinate
    }

    public string anchorName;

    public float xMin = -1;
    public float xMax = 1;
    public float yMin = -1;
    public float yMax = 1;
    public float zMin = -1;
    public float zMax = 1;

    public LockType lockX = LockType.None;
    public LockType lockY = LockType.None;
    public LockType lockZ = LockType.None;

    private Vector3 originEular;

    [HideInInspector]
    public bool startLock = false;

    void Awake()
    {
        originEular = transform.eulerAngles;
    }

    void Update()
    {
        if (startLock)
        {
            Vector3 eular = transform.eulerAngles;
            if (lockX == LockType.FollowCoordinate)
            {
                eular.x = 0;
            }
            else if (lockX == LockType.Lock)
            {
                eular.x = originEular.x;
            }

            if (lockY == LockType.FollowCoordinate)
            {
                eular.y = 0;
            }
            else if (lockY == LockType.Lock)
            {
                eular.y = originEular.y;
            }

            if (lockZ == LockType.FollowCoordinate)
            {
                eular.z = 0;
            }
            else if (lockZ == LockType.Lock)
            {
                eular.z = originEular.z;
            }

            transform.eulerAngles = eular;
        }
    }
 
}