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
        // ��Ǩ�ͺ������ѵ�ط������ "Player"
        if (collision.CompareTag("Player"))
        {
            // ������� List ����ѧ�����
            if (!playersInTrigger.Contains(collision))
            {
                playersInTrigger.Add(collision);
                CheckPlayerCount();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ����ѵ���͡�ҡ Trigger ���ź�͡�ҡ List
        if (collision.CompareTag("Player"))
        {
            playersInTrigger.Remove(collision);
        }
    }

    private void CheckPlayerCount()
    {
        // ����� Player � Trigger �ú 2 ���
        if (playersInTrigger.Count == 2)
        {
            // �ҵ��˹�����¢ͧ Player ����ͧ
            Vector2 averagePosition = (playersInTrigger[0].transform.position + playersInTrigger[1].transform.position) / 2f;

            // �Ѵ�Թ�������ͧ������˹������
            if (averagePosition.x < transform.position.x)
                cam.MoveToNewRoom(nextRoom);
            else
                cam.MoveToNewRoom(previousRoom);
        }
    }
}
