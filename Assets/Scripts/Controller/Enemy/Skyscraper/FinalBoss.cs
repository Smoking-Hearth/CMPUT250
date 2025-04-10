using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class FinalBoss : MonoBehaviour
{

    [System.Serializable]
    public struct BossFloor
    {
        public StaircaseSection rightStaircase;
        public ConnectorFloor connector;
        public StaircaseSection leftStaircase;

        public BossFloor(StaircaseSection right, ConnectorFloor connect, StaircaseSection left)
        {
            rightStaircase = right;
            connector = connect;
            leftStaircase = left;
        }
    }

    [SerializeField] Animator cameraAnimator;
    [SerializeField] Transform cameraTarget;
    [SerializeField] private float baseAltitude;
    [SerializeField] private float floorHeight;
    private float buildingHeight;
    [Min(1)]
    [SerializeField] private int floorCount;
    private BossFloor[] floors;
    private int currentFloor;
    private int highestClimbedFloor;
    public float CurrentFloorLevel
    {
        get
        {
            return transform.position.y + baseAltitude + currentFloor * floorHeight;
        }
    }

    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private AudioSource rainAudio;
    [SerializeField] private DialogueHolder windowFireDialogue;

    public BossFloor CurrentFloor
    {
        get
        {
            return floors[currentFloor];
        }
    }

    public float Completion
    {
        get
        {
            if (highestClimbedFloor == floorCount - 1)
            {
                return 1;
            }
            return (float)highestClimbedFloor / (floorCount - 1);
        }
    }

    [SerializeField] private FloorConnectorSorter sorter;
    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] private BossFloor groundFloor;
    [SerializeField] private BossFloor topFloor;

    [Header("Stairs")]
    [SerializeField] private StaircaseSection[] rightStaircasePrefabs;
    [SerializeField] private StaircaseSection[] leftStaircasePrefabs;
    [SerializeField] private StaircaseSection[] rightUnclimbable;
    [SerializeField] private StaircaseSection[] leftUnclimbable;

    [Header("Connectors")]
    [SerializeField] private ConnectorFloor[] connectorPrefabs;
    [SerializeField] private List<int> fullPassageFloor = new List<int>();    //Floors that will need to have a full connecter through them
    [SerializeField] private BossFloor[] bossFloor;
    [SerializeField] private int bossStartLevel;

    [SerializeField] private Slider completionSlider;
    private bool rightSide = true;

    [SerializeField] private FadeScreen winScreen;
    public delegate void OnWin();
    public static OnWin onWin;

    private void OnEnable()
    {
        onWin += Win;
    }

    private void OnDisable()
    {
        onWin -= Win;
    }
    void Start()
    {
        Generate();
    }

    public void Win()
    {
        winScreen.FadeIn(3);
    }

    public void ShowCompletionSlider(bool set)
    {
        completionSlider.gameObject.SetActive(set);
    }

    public void SetCameraPosition(Vector2 position)
    {
        cameraTarget.position = position;
        cameraAnimator.SetBool("Cultist", true);
    }

    public void ResetCamera()
    {
        cameraAnimator.SetBool("Cultist", false);
    }

    private void FixedUpdate()
    {
        CheckCurrentFloor();
        if (currentFloor == 1 && !CurrentFloor.rightStaircase.GlassBroken)
        {
            windowFireDialogue.PlayDialogue();
            CurrentFloor.rightStaircase.ActivateArm();
        }

        if (gameObject.MyLevelManager().levelState == LevelState.Playing)
        {
            highestClimbedFloor = currentFloor;
        }
        else if (gameObject.MyLevelManager().levelState == LevelState.Defeat && currentFloor <= highestClimbedFloor)
        {
            floors[currentFloor].connector.OpenDoors();
            floors[currentFloor].rightStaircase.OpenDoor();
            floors[currentFloor].leftStaircase.OpenDoor();
        }
        completionSlider.value = (float)currentFloor / floorCount;

        if (Completion >= 1 && !rainParticles.isPlaying)
        {
            rainParticles.Play();
            rainAudio.Play();
        }
    }

    public void Generate()
    {
        sorter.Sort(connectorPrefabs);
        floors = new BossFloor[floorCount];

        //Create first floor
        StaircaseSection rightInitial = Instantiate(groundFloor.rightStaircase, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        ConnectorFloor connectInitial = Instantiate(groundFloor.connector, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        StaircaseSection leftInitial = Instantiate(groundFloor.leftStaircase, transform.position + new Vector3(-60, baseAltitude), Quaternion.identity, transform);

        //Create top floor
        StaircaseSection rightEnd = Instantiate(topFloor.rightStaircase, transform.position + new Vector3(0, baseAltitude + (floorCount - 1) * floorHeight), Quaternion.identity, transform);
        ConnectorFloor connectEnd = Instantiate(topFloor.connector, transform.position + new Vector3(0, baseAltitude + (floorCount - 1) * floorHeight), Quaternion.identity, transform);
        StaircaseSection leftEnd = Instantiate(topFloor.leftStaircase, transform.position + new Vector3(-60, baseAltitude + (floorCount - 1) * floorHeight), Quaternion.identity, transform);

        floors[0] = new BossFloor(rightInitial, connectInitial, leftInitial);
        floors[floorCount - 1] = new BossFloor(rightEnd, connectEnd, leftEnd);

        for (int i = 1; i < floorCount - 1; i++)
        {
            floors[i] = GenerateFloor(i);
        }
        buildingHeight = floorHeight * floors.Length;
    }

    private BossFloor GenerateFloor(int i)
    {
        int rightIndex = Random.Range(0, rightStaircasePrefabs.Length);
        int leftIndex = Random.Range(0, leftStaircasePrefabs.Length);
        ConnectorFloor.Connections connections = ConnectorFloor.Connections.RIGHT;
        ConnectorFloor.Connections exclude = new ConnectorFloor.Connections();

        StaircaseSection right = rightStaircasePrefabs[rightIndex];
        StaircaseSection left = leftStaircasePrefabs[leftIndex];


        //Checking if the previous floor connects to this one
        if ((floors[i - 1].connector.floorConnections & ConnectorFloor.Connections.UP) > 0)
        {
            connections |= ConnectorFloor.Connections.DOWN;
        }


        if (fullPassageFloor.Contains(i + 1))
        {
            exclude |= ConnectorFloor.Connections.UP;
        }
        if (fullPassageFloor.Contains(i))
        {
            connections |= ConnectorFloor.Connections.LEFT | ConnectorFloor.Connections.RIGHT;
            exclude |= ConnectorFloor.Connections.DOWN;
            if (rightSide)
            {
                right = rightUnclimbable[0];
            }
            else
            {
                left = leftUnclimbable[0];
            }
        }
        if (fullPassageFloor.Contains(i - 1))
        {
            exclude |= ConnectorFloor.Connections.UP;
            if (rightSide)
            {
                right = rightUnclimbable[1];
            }
            else
            {
                left = leftUnclimbable[1];
            }
            rightSide = !rightSide;
        }

        ConnectorFloor connector = sorter.GetFittingConnector(connections, exclude);
        if (i == bossStartLevel)
        {
            right = bossFloor[0].rightStaircase;
            left = bossFloor[0].leftStaircase;
            connector = bossFloor[0].connector;
        }
        else if (i - 1 == bossStartLevel)
        {
            right = bossFloor[1].rightStaircase;
            left = bossFloor[1].leftStaircase;
            connector = bossFloor[1].connector;
        }

        right = Instantiate(right, transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
        left = Instantiate(left, transform.position + new Vector3(-60, baseAltitude + i * floorHeight), Quaternion.identity, transform);
        connector = Instantiate(connector, transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
        return new BossFloor(right, connector, left);
    }

    private void CheckCurrentFloor()
    {
        float playerY = gameObject.MyLevelManager().Player.Position.y - baseAltitude;
        currentFloor = Mathf.Clamp(Mathf.CeilToInt(playerY / buildingHeight * floorCount), 0, floorCount - 1);
    }
}
