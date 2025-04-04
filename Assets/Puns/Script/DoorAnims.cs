using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnims : MonoBehaviour, IDoor
{
    private Animator doorAnimator;
    private bool isOpen = false;

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        isOpen = true;
        doorAnimator.SetBool("Open", true);
    }

    public void CloseDoor()
    {
        isOpen = false;
        doorAnimator.SetBool("Open", false);
    }

    public void PlayOpenFailAnim()
    {
        doorAnimator.SetTrigger("OpenFail");
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }
}