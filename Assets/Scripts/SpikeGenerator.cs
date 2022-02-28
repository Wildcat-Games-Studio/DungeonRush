using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeGenerator : MonoBehaviour
{
    public GameObject spike;

    public int xDivisions, yDivisions;
    public float roomWidth, roomHeight;

    public delegate void AttackFinishedFunc();
    public AttackFinishedFunc attackFinished;

    private List<Transform> m_spikes = new List<Transform>();

    private void Start()
    {
        float xSize = roomWidth / xDivisions;
        float ySize = roomHeight / yDivisions;
        float halfWidth = roomWidth / 2.0f;
        float halfHeight = roomHeight / 2.0f;

        float halfxSize = xSize / 2.0f;
        float halfySize = ySize / 2.0f;

        for (uint y = 0; y < yDivisions; y++)
        {
            float ypoint = -halfHeight + y * ySize + halfySize;

            for (uint x = 0; x < xDivisions; x++)
            {
                float xpoint = -halfWidth + x * xSize + halfxSize;
                Transform t = GameObject.Instantiate(spike, new Vector2(xpoint, ypoint), new Quaternion()).transform;
                t.SetParent(transform);
                m_spikes.Add(t);
                t.gameObject.SetActive(false);
            }
        }
    }

    public void StartRandomAttack(float waitTime)
    {
        switch(Random.Range(0, 2))
        {
            case 0: StartCoroutine(DoHorizontalLineWE(waitTime)); break;
            default: StartCoroutine(DoHorizontalLineEW(waitTime)); break;
        }
    }

    public IEnumerator DoHorizontalLineWE(float waitTime)
    {
        for (int x = 0; x < xDivisions + 1; x++)
        {
            int xrem = x - 1;
            for (int y = 0; y < yDivisions; y++)
            {
                if (xrem > -1)
                {
                    m_spikes[xrem + y * xDivisions].gameObject.SetActive(false);
                }
                if (x < xDivisions)
                {
                    m_spikes[x + y * xDivisions].gameObject.SetActive(true);
                }
            }

            yield return new WaitForSeconds(waitTime);
        }

        attackFinished?.Invoke();
        yield return null;
    }

    public IEnumerator DoHorizontalLineEW(float waitTime)
    {
        for (int x = xDivisions - 1; x > -2; x--)
        {
            int xrem = x + 1;
            for (int y = 0; y < yDivisions; y++)
            {
                if (xrem < xDivisions)
                {
                    m_spikes[xrem + y * xDivisions].gameObject.SetActive(false);
                }
                if (x > -1)
                {
                    m_spikes[x + y * xDivisions].gameObject.SetActive(true);
                }
            }

            yield return new WaitForSeconds(waitTime);
        }

        attackFinished?.Invoke();
        yield return null;
    }

    // TODO
    public IEnumerator DoDiagonalW(float waitTime)
    {
        for (int x = 0; x < xDivisions; x++)
        {
            for (int y = 0; y < yDivisions; y++)
            {
                
            }

            yield return new WaitForSeconds(waitTime);
        }

        attackFinished?.Invoke();
        yield return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float xSize = roomWidth / xDivisions;
        float ySize = roomHeight / yDivisions;
        float halfWidth = roomWidth / 2.0f;
        float halfHeight = roomHeight / 2.0f;

        Vector2 start = new Vector2();
        Vector2 end = new Vector2();

        start.x = -halfWidth;
        start.y = -halfHeight;
        end.x = start.x;
        end.y = halfHeight;
        Gizmos.DrawLine(start, end);
        end.x = halfWidth;
        end.y = start.y;
        Gizmos.DrawLine(start, end);
        start.x = halfWidth;
        start.y = halfHeight;
        Gizmos.DrawLine(start, end);
        end.x = -halfWidth;
        end.y = start.y;
        Gizmos.DrawLine(start, end);

        for (uint x = 0; x < xDivisions; x++)
        {
            start.x = -halfWidth + x * xSize;
            end.x = start.x;

            start.y = -halfHeight;
            end.y = halfHeight;

            Gizmos.DrawLine(start, end);
        }

        for (uint y = 0; y < yDivisions; y++)
        {
            start.y = -halfHeight + y * ySize;
            end.y = start.y;

            start.x = -halfWidth;
            end.x = halfWidth;

            Gizmos.DrawLine(start, end);
        }
    }
}
