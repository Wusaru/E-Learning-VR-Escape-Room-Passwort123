//Tommy Bui

using System.Collections;
using UnityEngine;

public class PredictRNGSimulation : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject algorithmPrefab;
    public GameObject normalPacketPrefab;
    public GameObject guessPacketPrefab;

    [Header("Algorithm")]
    public Transform algorithmSpawnPoint;

    [Header("Points")]
    public Transform timeSpawnPoint;
    public Transform pidSpawnPoint;
    public Transform ppidSpawnPoint;
    public Transform algorithmTargetPoint;

    [Header("Display Points")]
    public Transform originalTokenDisplayPoint;
    public Transform predictedTokenDisplayPoint;

    [Header("Label Settings")]
    public Vector3 labelRotationEuler = new Vector3(0f, 90f, 0f);

    [Header("Settings")]
    public float packetSpeed = 2.5f;
    public float delayBetweenSteps = 0.5f;

    [Header("Attack Simulation")]
    public int guessAttemptCount = 20;
    public float guessPacketDelay = 0.5f;

    private bool running = false;

    private GameObject spawnedAlgorithm;
    private GameObject shownOriginalToken;
    private GameObject shownPredictedToken;

    private readonly string originalToken = "A7F3-91C2";
    private string predictedToken = "";

    public void StartSimulation()
    {
        if (running) return;

        CleanupOldRun();

        running = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        spawnedAlgorithm = Instantiate(
            algorithmPrefab,
            algorithmSpawnPoint.position + new Vector3(0, -2f, 0),
            algorithmSpawnPoint.rotation
        );

        yield return new WaitForSeconds(0.5f);

        yield return SendSeedSetAndWait(
            normalPacketPrefab,
            "ZEIT\n14:32:15",
            "PID\n1203",
            "PPID\n1170"
        );

        yield return new WaitForSeconds(delayBetweenSteps);

        yield return SendPacketAndWait(
            normalPacketPrefab,
            algorithmSpawnPoint.position,
            algorithmTargetPoint,
            "TOKEN\n" + originalToken
        );

        shownOriginalToken = SpawnDisplayPacket(
            normalPacketPrefab,
            originalTokenDisplayPoint.position,
            "ORIGINAL\n" + originalToken
        );

        yield return new WaitForSeconds(1f);


        int secondsOffset = 30;

        for (int attempt = 0; attempt < guessAttemptCount; attempt++)
        {
            string guessedTime =
                "14:32:" + secondsOffset.ToString("00");

            yield return SendSeedSetAndWait(
                guessPacketPrefab,
                "TESTE ZEIT\n" + guessedTime,
                "PID\n1203",
                "PPID\n1170"
            );

            yield return new WaitForSeconds(guessPacketDelay);

            
            secondsOffset++;

            if (secondsOffset > 59)
            {
                secondsOffset = 0;
            }
        }
        // Visualization: Attacker eventually finds token (hardcoded)
        predictedToken = originalToken;

        yield return SendSeedSetAndWait(
            guessPacketPrefab,
            "PASSENDE ZEIT\n14:32:15",
            "PID\n1203",
            "PPID\n1170"
        );
        yield return new WaitForSeconds(delayBetweenSteps);

        
        yield return SendPacketAndWait(
            guessPacketPrefab,
            algorithmSpawnPoint.position,
            algorithmTargetPoint,
            "GEFUNDENES TOKEN\n" + predictedToken
        );

        shownPredictedToken = SpawnDisplayPacket(
            guessPacketPrefab,
            predictedTokenDisplayPoint.position,
            "GEFUNDEN\n" + predictedToken
        );
        

        running = false;
    }

    private IEnumerator SendSeedSetAndWait(
        GameObject prefab,
        string timeLabel,
        string pidLabel,
        string ppidLabel
    )
    {
        Coroutine timePacket = StartCoroutine(
            SendPacketAndWait(
                prefab,
                timeSpawnPoint.position,
                algorithmSpawnPoint,
                timeLabel
            )
        );

        Coroutine pidPacket = StartCoroutine(
            SendPacketAndWait(
                prefab,
                pidSpawnPoint.position,
                algorithmSpawnPoint,
                pidLabel
            )
        );

        Coroutine ppidPacket = StartCoroutine(
            SendPacketAndWait(
                prefab,
                ppidSpawnPoint.position,
                algorithmSpawnPoint,
                ppidLabel
            )
        );

        yield return timePacket;
        yield return pidPacket;
        yield return ppidPacket;
    }

    private IEnumerator SendPacketAndWait(
        GameObject prefab,
        Vector3 startPosition,
        Transform target,
        string labelText
    )
    {
        bool arrived = false;

        GameObject packet = Instantiate(
            prefab,
            startPosition,
            Quaternion.identity
        );

        SetPacketLabelWithRotation(packet, labelText);

        PacketMover mover = packet.GetComponent<PacketMover>();

        mover.target = target;
        mover.speed = packetSpeed;
        mover.destroyOnArrival = true;

        mover.OnArrived += (_) =>
        {
            arrived = true;
        };

        yield return new WaitUntil(() => arrived);
    }

    private GameObject SpawnDisplayPacket(
        GameObject prefab,
        Vector3 position,
        string labelText
    )
    {
        GameObject packet = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        SetPacketLabelWithRotation(packet, labelText);

        PacketMover mover = packet.GetComponent<PacketMover>();

        if (mover != null)
        {
            mover.target = null;
        }

        return packet;
    }

    // Wrapper for SetPacketLabel, to rotate 
    private void SetPacketLabelWithRotation(
        GameObject packet,
        string labelText
    )
    {
        PacketLabel label = PacketLabel.SetPacketLabel(packet, labelText);

        if (label != null && label.textLabel != null)
        {
            label.textLabel.transform.localEulerAngles =
                labelRotationEuler;
        }
    }

    private void CleanupOldRun()
    {
        if (spawnedAlgorithm != null)
        {
            Destroy(spawnedAlgorithm);
            spawnedAlgorithm = null;
        }

        if (shownOriginalToken != null)
        {
            Destroy(shownOriginalToken);
            shownOriginalToken = null;
        }

        if (shownPredictedToken != null)
        {
            Destroy(shownPredictedToken);
            shownPredictedToken = null;
        }

        predictedToken = "";
    }
}