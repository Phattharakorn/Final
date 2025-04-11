using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPassCam : MonoBehaviour
{
    [SerializeField] private Transform previousRoom;
    [SerializeField] private Transform nextRoom;
    [SerializeField] private Cam cam;

    private List<Collider2D> playersInTrigger = new List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ตรวจสอบว่าเป็นวัตถุที่มีแท็ก "Player"
        if (collision.CompareTag("Player"))
        {
            // เพิ่มเข้า List ถ้ายังไม่มี
            if (!playersInTrigger.Contains(collision))
            {
                playersInTrigger.Add(collision);
                CheckPlayerCount();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ถ้าวัตถุออกจาก Trigger ให้ลบออกจาก List
        if (collision.CompareTag("Player"))
        {
            playersInTrigger.Remove(collision);
        }
    }

    private void CheckPlayerCount()
    {
        // ถ้ามี Player ใน Trigger ครบ 2 ตัว
        if (playersInTrigger.Count == 2)
        {
            // หาตำแหน่งเฉลี่ยของ Player ทั้งสอง
            Vector2 averagePosition = (playersInTrigger[0].transform.position + playersInTrigger[1].transform.position) / 2f;

            // ตัดสินใจย้ายห้องตามตำแหน่งเฉลี่ย
            if (averagePosition.x < transform.position.x)
                cam.MoveToNewRoom(nextRoom);
            else
                cam.MoveToNewRoom(previousRoom);
        }
    }
}
