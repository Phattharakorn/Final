using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPressure : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private float openHeight = 2f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float closeDelay = 1f;

    // เพิ่มตัวแปรสำหรับระบบเสียง
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;
    [SerializeField] private AudioSource audioSource;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private int objectsOnPlate = 0;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        closedPosition = doorTransform.position;
        openPosition = closedPosition + new Vector3(0f, openHeight, 0f);

        // ตรวจสอบ AudioSource ถ้าไม่มีให้เพิ่มอัตโนมัติ
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.attachedRigidbody != null)
        {
            objectsOnPlate++;

            if (objectsOnPlate == 1)
            {
                if (moveCoroutine != null)
                    StopCoroutine(moveCoroutine);

                // เล่นเสียงเปิดประตู
                PlayDoorSound(doorOpenSound);
                moveCoroutine = StartCoroutine(MoveDoor(openPosition));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.attachedRigidbody != null)
        {
            objectsOnPlate--;

            if (objectsOnPlate <= 0)
            {
                objectsOnPlate = 0;
                if (moveCoroutine != null)
                    StopCoroutine(moveCoroutine);

                // เล่นเสียงปิดประตู
                PlayDoorSound(doorCloseSound);
                StartCoroutine(CloseDoorWithDelay());
            }
        }
    }

    private IEnumerator MoveDoor(Vector3 targetPosition)
    {
        while (Vector3.Distance(doorTransform.position, targetPosition) > 0.01f)
        {
            doorTransform.position = Vector3.MoveTowards(
                doorTransform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        moveCoroutine = StartCoroutine(MoveDoor(closedPosition));
    }

    // เมธอดสำหรับเล่นเสียง
    private void PlayDoorSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
