using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    [SerializeField] private float speed;
    private float currentPosX;
    private float currentPosY;
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, currentPosY, transform.position.z), ref velocity, speed);

    }

    public void MoveToNewRoom(Transform _newRoom)
    {
        currentPosX = _newRoom.position.x;
        currentPosY = _newRoom.position.y;
    }
}
