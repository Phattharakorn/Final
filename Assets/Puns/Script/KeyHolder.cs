using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHolder : MonoBehaviour
{
    private List<KeyScript.KeyType> keyList;
    //public AudioSource playerSource;
    //public AudioClip keySound;

    private void Awake()
    {
        keyList = new List<KeyScript.KeyType>();
    }

    public void AddKey(KeyScript.KeyType keyType)
    {
        //playerSource.PlayOneShot(keySound);
        Debug.Log("Added Key:" + keyType);
        keyList.Add(keyType);
    }

    public void RemoveKey(KeyScript.KeyType keyType)
    {
        keyList.Remove(keyType);
    }

    public bool ConstainKey(KeyScript.KeyType keyType)
    {
        return keyList.Contains(keyType);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        KeyScript key = collider.GetComponent<KeyScript>();
        if (key != null)
        {
            AddKey(key.GetKeyType());
            Destroy(key.gameObject);

        }


        KeyDoor keyDoor = collider.GetComponent<KeyDoor>();
        if (keyDoor != null)
        {
            if (ConstainKey(keyDoor.GetKeyType()))
            {
                RemoveKey(keyDoor.GetKeyType());
                keyDoor.OpenDoor();
            }
        }
    }
}
