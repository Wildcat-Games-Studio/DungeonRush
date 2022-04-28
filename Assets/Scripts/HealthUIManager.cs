using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUIManager : MonoBehaviour
{

    public EntityStats PlayerStats;
    public const int numHearts = 5;
    public HeartBreaker[] hearts = new HeartBreaker[numHearts];
    public GameObject heartPrefab;
    public Vector2 heartSpacing = new Vector2(20,0);  // space between the hearts
    public Vector2 startingHeartLocation = new Vector2(-35, 20);

    private int currentHeart;
    private GameObject[] heartTargets;
    // Start is called before the first frame update
    void Start()
    {
        GenerateHeartItems();
        PlayerStats.onDamage += OnDamageReceived;
        currentHeart = hearts.Length;
    }


    void GenerateHeartItems()
    {
        Vector2 heartLocation = startingHeartLocation;
        for (int i = 0; i < numHearts; i++)
        {
            GameObject heart = Instantiate(heartPrefab);
            heart.transform.parent = this.transform;
            heart.transform.position = heartLocation;
            heartLocation += heartSpacing;
            hearts[i] = heart.GetComponent<HeartBreaker>();
        }
    }

    void OnDamageReceived(int CurrentHealth)
    {
        if (currentHeart == 0) return;
        hearts[--currentHeart].Break();
    }
}
