using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int room { get; set; }
    private bool slimeBoss = true;
    private bool crystalBoss = true;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
    public void ChangeRoom(int newRoom) // called when a player touches a door
    {
        if (newRoom != room)
        {
            room = newRoom;
            SceneManager.LoadScene(newRoom);

        }
    }
    public bool isOpen(int room) // is the room open
    {
        if (room == 1)
            return slimeBoss;
        if(room == 2)
            return crystalBoss;
        return true;
 

    }

}
