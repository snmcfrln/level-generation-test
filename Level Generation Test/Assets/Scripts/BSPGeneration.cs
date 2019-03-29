using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BSPGeneration : MonoBehaviour
{
    [Header("Map Size")]
    public int rows, columns;
    [Header("Room Size")]
    public int minRoomSize, maxRoomSize;
    [Header("Number of zones per row/column")]
    public int amountOfZones = 1;
    [Header("Prefab Size")]

    private Zone[,] zones;

    private static List<Rect> sectionList = new List<Rect>();
    private static List<Rect> corridorList = new List<Rect>();
    private static List<Room> roomList = new List<Room>();
    private static List<Zone> zoneList = new List<Zone>();

    public GameObject[] dungeons;

    private Rect mapSize;


    private void Awake()
    {
        Section initialSection = new Section(new Rect(0, 0, rows, columns));
        Partition(initialSection);
        CreateRoom(initialSection);

        zones = new Zone[rows, columns];
        InitialiseZones(initialSection.rect);
        mapSize = initialSection.rect;

        foreach (Room room in roomList)
        {
            GetNeighbours(room);
        }

        //dungeons.OrderBy(dungeons => dungeons.GetComponent<Renderer>().bounds.size).ToArray();
        for (int i = 0; i < dungeons.Length; i++)
        {
            print(dungeons[i].GetComponent<Renderer>().bounds.size);
        }

        // DrawRooms(initialSection);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawCube(mapSize.center, mapSize.size);

        foreach (Rect rect in sectionList)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(rect.center, rect.size);
        }

        if (Application.isPlaying)
        {
            foreach (Zone zone in zoneList)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawWireCube(zone.rect.center, zone.rect.size);
            }
        }

        foreach (Room room in roomList)
        {
            if(!room.hasRandomised)
            {
                for (int i = 0; i < zoneList.Count; i++)
                {
                    if (zoneList[i].rect.Contains(room.rect.center))
                    {
                        room.color = zoneList[i].color;
                    }
                }
            }
            Gizmos.color = room.color;
            Gizmos.DrawCube(room.rect.center, room.rect.size);
        }

        foreach (Rect rect in corridorList)
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(rect.center, rect.size);
        }

        foreach (Room room in roomList)
        {
            for (int i = 0; i < room.neighbours.Count; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(room.rect.center, room.neighbours[i].rect.center);
            }
        }
    }

    public void GetNeighbours(Room room)
    {
        foreach (Room troom in roomList)
        {
            float dist = Vector3.Distance(troom.rect.center, room.rect.center);
            if (dist < 25 && dist > 1)
            {
                room.neighbours.Add(troom);
            }
        }
    }

    public void DrawRooms(Section section)
    {

    }

    public Vector2 RoomWidth(Rect rect)
    {
        Vector2 roomsize;
        roomsize = new Vector2(1, 3);

        int sectionX = (int)rect.size.x;
        int sectionY = (int)rect.size.y;

        for (int i = 0; i < dungeons.Length; i++)
        {
            int dungeonX = (int)dungeons[i].GetComponent<Renderer>().bounds.size.x;
            int dungeonY = (int)dungeons[i].GetComponent<Renderer>().bounds.size.y;
            if (dungeonX < sectionX && dungeonY < sectionY)
            {
                roomsize = new Vector2(dungeonX, dungeonY);
                print("Success!");
                return roomsize;
                //print(dungeon.GetComponent<Renderer>().bounds.size);
            }
        }
        return roomsize;
    }

    public void CreateRoom(Section section)
    {
        if (section.left != null)
        {
            CreateRoom(section.left);
        }
        if (section.right != null)
        {
            CreateRoom(section.right);
        }
        if (section.IsLeaf())
        {
            Vector2 roomDimensions = RoomWidth(section.rect);
            int roomWidth = (int)roomDimensions.x;
            int roomHeight = (int)roomDimensions.y;
            //int roomWidth = (int)Random.Range(section.rect.width / 2, section.rect.width - 2);
            //int roomHeight = (int)Random.Range(section.rect.height / 2, section.rect.height - 2);
            int roomX = (int)Random.Range(1, section.rect.width - roomWidth - 1);
            int roomY = (int)Random.Range(1, section.rect.height - roomHeight - 1);

            section.room = new Room(new Rect(section.rect.x + roomX, section.rect.y + roomY, roomWidth, roomHeight));

            float corridorX = Random.Range(Mathf.Abs(section.room.rect.xMin) + 1, Mathf.Abs(section.room.rect.xMax) - 1);
            float corridorY;
            if (Random.Range(0.0f, 1.0f) > 0.5f)
            {
                corridorY = section.room.rect.yMax - 1;
            }
            else
            {
                corridorY = section.room.rect.yMin;
            }
            section.corridor = new Rect(corridorX, Mathf.Abs(corridorY), 1, 1);

            roomList.Add(section.room);
            corridorList.Add(section.corridor);
        }
    }

    public class Room
    {
        public Rect rect;
        public bool hasRandomised = false;
        public Color color;
        public GameObject dungeon;

        public List<Room> neighbours = new List<Room>();

        public Room(Rect trect, Color tcolor = default(Color))
        {
            rect = trect;
            color = tcolor;
        }
    }

    public class Zone
    {
        public int ID;
        public Rect rect;
        private static int idCounter;
        public Color color;

        public Zone(Rect trect)
        {
            rect = trect;
            ID = idCounter;
            idCounter++;
            color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        }
    }

    public class Section
    {
        public Section left, right;
        public Rect rect;
        public Room room = new Room(new Rect(-1, -1, 0, 0)); //null
        public Rect corridor = new Rect(-1, -1, 0, 0); //null

        public int sectionID;
        public static int sectionIDCounter = 0;

        public Section(Rect trect)
        {
            rect = trect;
            sectionID = sectionIDCounter;
            sectionIDCounter++;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            if (IsLeaf() == false)
            {
                return false;
            }

            bool splitHorizontally;

            if (rect.width / rect.height >= 1.25)
            {
                splitHorizontally = false;
            }
            else if (rect.height / rect.width >= 1.25)
            {
                splitHorizontally = true;
            }
            else
            {
                splitHorizontally = Random.Range(0.0f, 1.0f) > 0.5f;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
            {
                return false;
            }

            if (splitHorizontally == true)
            {
                int splitValue = Random.Range(minRoomSize, (int)rect.width - minRoomSize);

                left = new Section(new Rect(rect.x, rect.y, rect.width, splitValue));
                right = new Section(new Rect(rect.x, rect.y + splitValue, rect.width, rect.height - splitValue));
                sectionList.Add(left.rect);
                sectionList.Add(right.rect);
            }
            else
            {
                int splitValue = Random.Range(minRoomSize, (int)rect.height - minRoomSize);

                left = new Section(new Rect(rect.x, rect.y, splitValue, rect.height));
                right = new Section(new Rect(rect.x + splitValue, rect.y, rect.width - splitValue, rect.height));
                sectionList.Add(left.rect);
                sectionList.Add(right.rect);
            }
            return true;
        }
    }

    public void Partition(Section section)
    {
        if (section.IsLeaf() == true)
        {
            if (section.rect.width > maxRoomSize
            || section.rect.height > maxRoomSize
            || Random.Range(0.0f, 1.0f) > 0.25)
            {
                if (section.Split(minRoomSize, maxRoomSize))
                {
                    Partition(section.left);
                    Partition(section.right);
                }
            }
        }
    }

    void InitialiseZones(Rect section)
    {
        int zoneHeight = (columns) / amountOfZones;
        int zoneWidth = (rows) / amountOfZones;

        for (int i = (int)section.xMin; i < section.xMax / zoneWidth; i++)
        {
            for (int j = (int)section.yMin; j < section.yMax / zoneHeight; j++)
            {
                zones[i, j] = new Zone(new Rect(i * zoneWidth, j * zoneHeight, zoneWidth, zoneHeight));
                zoneList.Add(zones[i, j]);
                print("zi: " + i + "zj: " + j);
                print("Zone " + zones[i, j].ID + " created" + zones[i, j].rect.center);
            }
        }
    }
}