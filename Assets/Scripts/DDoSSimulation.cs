//Tommy Bui

using System.Collections;
using UnityEngine;

public class DDoSSimulation : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject serverPrefab;
    public GameObject greenPacketPrefab;
    public GameObject redPacketPrefab;

    [Header("Spawn Points")]
    public Transform serverSpawnPoint;
    public Transform legitPacketSpawnPoint;
    public Transform[] attackPacketSpawnPoints;

    [Header("Targets")]
    public Transform serverTargetPoint;
    public Transform internalTargetPoint;

    [Header("Queue Settings")]
    public Transform greenQueueStartPoint;
    public Transform redQueueStartPoint;
    public float queueSpacing = 0.18f;
    public int maxQueueLength = 15;

    [Header("Traffic Settings")]
    public float greenSpawnDelay = 1.2f;
    public float redInitialSpawnDelay = 0.8f;
    public float overloadDelayThreshold = 0.18f;
    public float redQueueSpawnDelay = 0.25f;

    private bool simulationRunning;
    private bool serverOverloaded;

    private int greenQueuePos;
    private int[] redQueuePos;
    private GameObject spawnedServer;

    public void StartSimulation()
    {
        if (simulationRunning) return;

        simulationRunning = true;
        serverOverloaded = false;

        greenQueuePos = 0;
        redQueuePos = new int[attackPacketSpawnPoints.Length];

        StartCoroutine(RunSimulation());
    }

    private IEnumerator RunSimulation()
    {
         spawnedServer = Instantiate(serverPrefab, serverSpawnPoint.position + new Vector3(0, -1f, 0), serverSpawnPoint.rotation);

        StartCoroutine(SpawnGreenTraffic());

        yield return new WaitForSeconds(4f);

        StartCoroutine(SpawnRedTraffic());
    }

    private IEnumerator SpawnGreenTraffic()
    {
        while (simulationRunning)
        {
            if (serverOverloaded)
            {
                TrySpawnQueuedPacket(
                    greenPacketPrefab,
                    legitPacketSpawnPoint,
                    greenQueueStartPoint,
                    legitPacketSpawnPoint,
                    ref greenQueuePos,
                    2.5f
                );
            }
            else
            {
                SpawnIncomingPacket(
                    greenPacketPrefab,
                    legitPacketSpawnPoint,
                    serverTargetPoint,
                    2.5f,
                    true
                );
            }

            StopIfQueuesFull();
            yield return new WaitForSeconds(greenSpawnDelay);
        }
    }

    private IEnumerator SpawnRedTraffic()
    {
        float spawnDelay = redInitialSpawnDelay;

        while (simulationRunning)
        {
            int spawnIndex = Random.Range(0, attackPacketSpawnPoints.Length);
            Transform attackSpawn = attackPacketSpawnPoints[spawnIndex];

            if (serverOverloaded)
            {
                TrySpawnQueuedPacket(
                    redPacketPrefab,
                    attackSpawn,
                    redQueueStartPoint,
                    attackSpawn,
                    ref redQueuePos[spawnIndex],
                    2.2f
                );

                StopIfQueuesFull();
                yield return new WaitForSeconds(redQueueSpawnDelay);
                continue;
            }

            SpawnIncomingPacket(
                redPacketPrefab,
                attackSpawn,
                serverTargetPoint,
                2.2f,
                false
            );

            spawnDelay = Mathf.Max(spawnDelay - 0.03f, overloadDelayThreshold);

            if (spawnDelay <= overloadDelayThreshold)
            {
                serverOverloaded = true;
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnIncomingPacket(
        GameObject prefab,
        Transform spawnPoint,
        Transform target,
        float speed,
        bool createsInternalPacket
    )
    {
        PacketMover mover = CreatePacket(prefab, spawnPoint.position, target, speed, true);

        if (mover == null) return;

        mover.OnArrived = _ =>
        {
            if (createsInternalPacket && !serverOverloaded)
            {
                SpawnInternalPacket();
            }
        };
    }

    private void TrySpawnQueuedPacket(
        GameObject prefab,
        Transform spawnPoint,
        Transform queueStartPoint,
        Transform directionPoint,
        ref int queueIndex,
        float speed
    )
    {
        if (queueIndex >= maxQueueLength) return;

        Vector3 queuePosition = GetQueuePosition(queueStartPoint, directionPoint, queueIndex);
        queueIndex++;

        GameObject queueTarget = new GameObject("QueueTarget");
        queueTarget.transform.position = queuePosition;

        PacketMover mover = CreatePacket(prefab, spawnPoint.position, queueTarget.transform, speed, false);

        mover.OnArrived = _ =>
        {
            mover.target = null;
            Destroy(queueTarget);
        };
    }

    private void SpawnInternalPacket()
    {
        CreatePacket(
            greenPacketPrefab,
            serverTargetPoint.position,
            internalTargetPoint,
            2.5f,
            true
        );
    }

    private PacketMover CreatePacket(
        GameObject prefab,
        Vector3 spawnPosition,
        Transform target,
        float speed,
        bool destroyOnArrival
    )
    {
        GameObject packet = Instantiate(prefab, spawnPosition, Quaternion.identity);

        PacketMover mover = packet.GetComponent<PacketMover>();

        mover.target = target;
        mover.speed = speed;
        mover.destroyOnArrival = destroyOnArrival;

        return mover;
    }

    private Vector3 GetQueuePosition(
        Transform queueStartPoint,
        Transform directionPoint,
        int index
    )
    {
        Vector3 direction = (directionPoint.position - queueStartPoint.position).normalized;
        return queueStartPoint.position + direction * queueSpacing * index;
    }

    private void StopIfQueuesFull()
    {
        if (!serverOverloaded) return;

        bool greenFull = greenQueuePos >= maxQueueLength;
        bool allRedFull = true;

        for (int i = 0; i < redQueuePos.Length; i++)
        {
            if (redQueuePos[i] < maxQueueLength)
            {
                allRedFull = false;
                break;
            }
        }

        if (greenFull && allRedFull)
        {
            simulationRunning = false;
            Invoke(nameof(ClearSimulation), 10f);
        }
    }
    private void ClearSimulation()
    {
        StopAllCoroutines();

        foreach (PacketMover packet in FindObjectsByType<PacketMover>(
            FindObjectsSortMode.None))
        {
            Destroy(packet.gameObject);
        }


        if (spawnedServer != null)
        {
            Destroy(spawnedServer);
            spawnedServer = null;
        }

        serverOverloaded = false;
        simulationRunning = false;
        greenQueuePos = 0;
        redQueuePos = null;
    }
}
