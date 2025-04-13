using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDoor : MonoBehaviour
{
    [SerializeField] private KeyScript.KeyType KeyType;

    public AudioSource playerSource;
    public AudioClip doorSound;
    public KeyScript.KeyType GetKeyType()
    {
        return KeyType;
    }

    public void OpenDoor()
    {
        playerSource.PlayOneShot(doorSound);
        gameObject.SetActive(false);

    }
}
