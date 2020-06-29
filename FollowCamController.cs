using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamController : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset;
    public float distance = 5f;

    private Vector3 targetOffsetPosition;
    private Transform pivot;
    
    // Start is called before the first frame update
    void Start()
    {
        pivot = new GameObject("Pivot").transform;
        transform.parent = pivot;
        transform.position = new Vector3(0f, 0f, -distance);
    }

    // Update is called once per frame
    void Update()
    {
        targetOffsetPosition = target.position + targetOffset;
        pivot.position = targetOffsetPosition;
        pivot.eulerAngles = new Vector3(0f, target.eulerAngles.y, 0f);
    }
}
