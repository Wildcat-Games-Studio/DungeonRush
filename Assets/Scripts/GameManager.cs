using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int room { get; set; }
    private bool[] openRooms;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public void ChangeRoom(int newRoom) // called when a player touches a door
    {
        if (openRooms[newRoom] && newRoom != room)
        {
            room = newRoom;
            SceneManager.LoadScene(newRoom);

        }
    }
    public void ChangeRoom(int newRoom, bool oldRoomIsOpen) // called when a player touches a door, change oldRoomIsOpen to false to close a room
    {
        if (openRooms[newRoom] && newRoom != room)
        {
            openRooms[newRoom] = oldRoomIsOpen;
            room = newRoom;
            SceneManager.LoadScene(newRoom);

        }
    }
}
