using System.Collections;
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

    private bool zoned = false;
    private Zone[,] zones;

    private static List<Rect> sectionList = new List<Rect>();
    private static List<Room> roomList = new List<Room>();
    private static List<Rect> corridorList = new List<Rect>();
    private static List<Zone> zoneList = new List<Zone>();

    public GameObject floorTile;
    private GameObject[,] floorPositions;

    private Rect mapSize;


    private void Start()
    {
        Section initialSection = new Section(new Rect(0, 0, rows, columns));
        Partition(initialSection);
        initialSection.CreateRoom();

        floorPositions = new GameObject[rows, columns];
        zones = new Zone[rows, columns];
        InitialiseZones(initialSection.rect);
        mapSize = initialSection.rect;


        // DrawRooms(initialSection);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 1, 0.2f);
        Gizmos.DrawCube(mapSize.center, mapSize.size);

        foreach (Rect rect in sectionList)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(rect.center, rect.size);
        }

        if (zoned)
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
    }

    public void DrawRooms(Section section)
    {
        if (section == null)
        {
            return;
        }
        if (section.IsLeaf())
        {
            for (int i = (int)section.room.rect.x; i < section.room.rect.xMax; i++)
            {
                for (int j = (int)section.room.rect.y; j < section.room.rect.yMax; j++)
                {
                    GameObject instance = Instantiate(floorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                }
            }
        }
        else
        {
            DrawRooms(section.left);
            DrawRooms(section.right);
        }
    }

    public class Room
    {
        public Rect rect;
        public bool hasRandomised = false;
        public Color color;

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
        public Room room = new Room(new Rect(-1, -1, 0, 0)); // i.e null
        public Rect corridor = new Rect(-1, -1, 0, 0); // i.e null

        public int sectionID;
        public static int sectionIDCounter = 0;

        public void CreateRoom()
        {
            if (left != null)
            {
                left.CreateRoom();
            }
            if (right != null)
            {
                right.CreateRoom();
            }
            if (IsLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                room = new Room(new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight));

                float corridorX = Random.Range(rect.xMin + 1, room.rect.xMax - 1);
                float corridorY;
                if (Random.Range(0.0f, 1.0f) > 0.5f)
                {
                    corridorY = room.rect.yMax - 1;
                }
                else
                {
                    corridorY = room.rect.yMin;
                }
                corridor = new Rect(corridorX, corridorY, 1, 1);

                roomList.Add(room);
                corridorList.Add(corridor);
            }
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public Section(Rect trect)
        {
            rect = trect;
            sectionID = sectionIDCounter;
            sectionIDCounter++;
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

        for (int i = (int)section.x; i < section.xMax / zoneWidth; i++)
        {
            for (int j = (int)section.y; j < section.yMax / zoneHeight; j++)
            {
                zones[i, j] = new Zone(new Rect(i * zoneWidth, j * zoneHeight, zoneWidth, zoneHeight));
                zoneList.Add(zones[i, j]);
                print("zi: " + i + "zj: " + j);
                print("Zone " + zones[i, j].ID + " created" + zones[i, j].rect.center);
            }
        }
        zoned = true;
    }
}