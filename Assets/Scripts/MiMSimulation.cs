//Tommy Bui

using System.Collections;
using UnityEngine;

public class MiMSimulation : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject attackerServerPrefab;
    public GameObject greenPacketPrefab;
    public GameObject redPacketPrefab;

    [Header("Points")]
    public Transform attackerSpawnPoint;
    public Transform clientPacketSpawnPoint;
    public Transform attackerStashPoint;
    public Transform serverTargetPoint;

    [Header("Settings")]
    public float packetSpeed = 2.5f;
    public float delayBetweenSteps = 0.5f;

    private bool isRunning = false;
    private GameObject stashedPacket;
    private GameObject spawnedAttacker;
    private GameObject spawnedServer;

    // Starts simulation, via button interaction
    public void StartSimulation()
    {
        if (isRunning) return;

        ClearOldRun();

        isRunning = true;

        spawnedAttacker = Instantiate(
            attackerServerPrefab,
            attackerSpawnPoint.position + new Vector3(0, -2.3f, 0),
            attackerSpawnPoint.rotation
        );

        spawnedServer = Instantiate(
            attackerServerPrefab,
            serverTargetPoint.position + new Vector3(0, -1f, 0),
            serverTargetPoint.rotation
        );

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        // Client sends login request
        yield return SendPacketAndWait(
            greenPacketPrefab,
            clientPacketSpawnPoint.position,
            attackerSpawnPoint,
            "Login Anfrage"
        );

        yield return new WaitForSeconds(delayBetweenSteps);

        // Attacker copies credentials
        StartCoroutine(SendPacketAndWait(
            greenPacketPrefab,
            attackerSpawnPoint.position,
            attackerStashPoint,
            "gespeichertes Passwort",
            false
        ));

        // Attacker forwards original request
        StartCoroutine(SendPacketAndWait(
            greenPacketPrefab,
            attackerSpawnPoint.position,
            serverTargetPoint,
            "Login Anfrage"
        ));

        yield return new WaitForSeconds(2f);

        // Client sends transfer request
        yield return SendPacketAndWait(
            greenPacketPrefab,
            clientPacketSpawnPoint.position,
            attackerSpawnPoint,
            "Überweise 10€ an B"
        );

        yield return new WaitForSeconds(delayBetweenSteps);

        // Attacker modifies request
        yield return SendPacketAndWait(
            redPacketPrefab,
            attackerSpawnPoint.position,
            serverTargetPoint,
            "Überweise 1000€ an A"
        );

        yield return new WaitForSeconds(delayBetweenSteps);

        // Replay stored password
        if (stashedPacket != null)
        {
            PacketLabel.SetPacketLabel(
                stashedPacket,
                "Gespeichertes Passwort"
            );

            // Move from stash back to attacker
            yield return MovePacketAndWait(
                stashedPacket,
                attackerSpawnPoint,
                false
            );

            yield return new WaitForSeconds(delayBetweenSteps);

            // Move from attacker to server
            yield return MovePacketAndWait(
                stashedPacket,
                serverTargetPoint,
                true
            );

            stashedPacket = null;
        }

        // Leave result visible for a moment
        yield return new WaitForSeconds(1f);

        // Clean up attacker and server
        if (spawnedAttacker != null)
        {
            Destroy(spawnedAttacker);
            spawnedAttacker = null;
        }

        if (spawnedServer != null)
        {
            Destroy(spawnedServer);
            spawnedServer = null;
        }

        isRunning = false;
    }

    private IEnumerator SendPacketAndWait(
        GameObject prefab,
        Vector3 startPosition,
        Transform target,
        string labelText,
        bool destroyOnArrival = true
    )
    {
        GameObject packet = Instantiate(
            prefab,
            startPosition,
            Quaternion.identity
        );

        PacketLabel.SetPacketLabel(packet, labelText);


        yield return MovePacketAndWait(
            packet,
            target,
            destroyOnArrival
        );
    }

    private IEnumerator MovePacketAndWait(
        GameObject packet,
        Transform target,
        bool destroyOnArrival
    )
    {
        bool arrived = false;

        if (packet == null || target == null)
        {
            yield break;
        }

        PacketMover mover =
            packet.GetComponent<PacketMover>();

        if (mover == null)
        {
            yield break;
        }

        mover.target = target;
        mover.speed = packetSpeed;
        mover.destroyOnArrival = destroyOnArrival;

        mover.OnArrived = (arrivedPacket) =>
        {
            arrived = true;

            if (
                !destroyOnArrival &&
                target == attackerStashPoint
            )
            {
                stashedPacket = arrivedPacket;

                arrivedPacket.transform.position =
                    attackerStashPoint.position;
            }
        };

        yield return new WaitUntil(() => arrived);
    }

    private void ClearOldRun()
    {
        if (spawnedAttacker != null)
        {
            Destroy(spawnedAttacker);
            spawnedAttacker = null;
        }

        if (spawnedServer != null)
        {
            Destroy(spawnedServer);
            spawnedServer = null;
        }

        if (stashedPacket != null)
        {
            Destroy(stashedPacket);
            stashedPacket = null;
        }
    }
}