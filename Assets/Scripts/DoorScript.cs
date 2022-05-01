using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public int room;
    private bool open;

    private void Start()
    {
        open = GameManager.Instance.isOpen(room);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (open && collision.gameObject.name == "Player")
        {
            GameManager.Instance.ChangeRoom(room);
        }
    }
}