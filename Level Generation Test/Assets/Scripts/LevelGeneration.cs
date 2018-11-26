using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    public int rows, columns;
    public int minRoomSize, maxRoomSize;

    public static List<Rect> sectionList = new List<Rect>();


    public GameObject floorTile;
    public GameObject corridorTile;

    private GameObject[,] floorPositions;

    // Use this for initialization
    void Start()
    {
        Section initialSection = new Section(new Rect(0, 0, rows, columns));
        CreateBSP(initialSection);
        initialSection.CreateRoom();

        floorPositions = new GameObject[rows, columns];
        //DrawCorridors(initialSection);
        //DrawRooms(initialSection);
    }

    private void OnDrawGizmos()
    {
        foreach (Rect rect in sectionList)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(rect.center, rect.size);
            //GUI.Box(new Rect(rect.x - 100, rect.y - 100, rect.width, rect.height), "Section ID: ");
        }
    }

    public void DrawRooms(Section section)
    {
        if(section == null)
        {
            return;
        }
        if(section.IAmLeaf() == true)
        {
            for (int i = (int)section.room.x; i < section.room.xMax; i++) {
                for (int j = (int)section.room.y; j < section.room.yMax; j++) {
                    GameObject instance = Instantiate(floorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                    floorPositions[i, j] = instance;
                }
            }
        }
        else
        {
            DrawRooms(section.left);
            DrawRooms(section.right);
        }
    }

    void DrawCorridors(Section section)
    {
        if (section == null)
        {
            return;
        }
        foreach(Rect corridor in section.corridors)
        {
            for(int i = (int)corridor.x; i < corridor.xMax; i++)
            {
                for(int j = (int)corridor.y; j < corridor.yMax; j++)
                {
                    if(floorPositions[i,j] == null)
                    {
                        GameObject instance = Instantiate (corridorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(transform);
                        floorPositions[i, j] = instance;
                    }
                }
            }
        }
    }

    public class Section
    {
        public Section left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0);
        public int debugID;

        public List<Rect> corridors = new List<Rect>();

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
            if(left != null && right != null)
            {
                CreateCorridor(left, right);
            }
            if (IAmLeaf())
            {
                int roomWidth = (int)(Random.Range(rect.width / 2, rect.width - 2));
                int roomHeight = (int)(Random.Range(rect.height / 2, rect.height - 2));
                int roomX = (int)(Random.Range(1, rect.width - roomWidth - 1));
                int roomY = (int)(Random.Range(1, rect.height - roomHeight - 1));

                room = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);

                sectionList.Add(room);

                Debug.Log("Created room " + room + " in section " + debugID + " " + rect);
            }
        }

        public Rect GetRoom()
        {
            if (IAmLeaf())
            {
                return room;
            }
            if (left != null)
            {
                Rect lroom = left.GetRoom();
                if (lroom.x != -1)
                {
                    return lroom;
                }
            }
            if (right != null)
            {
                Rect rroom = right.GetRoom();
                if (rroom.x != -1)
                {
                    return rroom;
                }
            }

            return new Rect(-1, -1, 0, 0);
        }

        public void CreateCorridor(Section left, Section right)
        {
            Rect lroom = left.GetRoom();
            Rect rroom = right.GetRoom();

            Debug.Log("Creating corridor between " + left.debugID + "(" + lroom + ") and " + right.debugID + " (" + rroom + ") ");

            // attach the corridor to a random point in each room
            Vector2 lpoint = new Vector2((int)Random.Range(lroom.x + 1, lroom.xMax - 1), (int)Random.Range(lroom.y + 1, lroom.yMax - 1));
            Vector2 rpoint = new Vector2((int)Random.Range(rroom.x + 1, rroom.xMax - 1), (int)Random.Range(rroom.y + 1, rroom.yMax - 1));

            //Always be sure that left point is on the left to simplify code
            if(lpoint.x > rpoint.x)
            {
                Vector2 temp = lpoint;
                lpoint = rpoint;
                rpoint = temp;
            }

            int w = (int)(lpoint.x - rpoint.x);
            int h = (int)(lpoint.y - rpoint.y);

            Debug.Log("lpoint: " + lpoint + ", rpoint: " + rpoint + ", w: " + w + ", h: " + h);

            //If the points are not aligned horizontally
            if(w != 0)
            {
                //Choose at random to go horizontal then vertical or vice versa 
                if(Random.Range(0, 1) > 2)
                {
                    //Add a corridor to the right
                    corridors.Add(new Rect(lpoint.x, lpoint.y, Mathf.Abs(w) + 1, 1));
                    //if left point is below point go up
                    //otherwise go down
                    if(h < 0)
                    {
                        corridors.Add(new Rect(rpoint.x, lpoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(lpoint.x, rpoint.y, 1, Mathf.Abs(h)));
                    }
                    //then go right
                    corridors.Add(new Rect(lpoint.x, rpoint.y, Mathf.Abs(w) + 1, 1));
                }
                else
                {
                    //if the points are aligned horizontally
                    //go up or down depending on the positions
                    if(h < 0)
                    {
                        corridors.Add(new Rect((int)lpoint.x, (int)lpoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect((int)lpoint.x, (int)rpoint.y, 1, Mathf.Abs(h)));
                    }
                }
                Debug.Log("corridors: ");
                foreach(Rect corridor in corridors)
                {
                    Debug.Log("corridor: " + corridor);
                }
            }
        }

        public static int debugCounter = 0;

        public Section(Rect mrect)
        {
            rect = mrect;
            debugID = debugCounter;
            debugCounter++;
        }

        public bool IAmLeaf()
        {
            return left == null && right == null;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            if (IAmLeaf() == false)
            {
                return false;
            }

            bool splitH;
            if (rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else if (rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }
            else
            {
                splitH = Random.Range(0.0f, 1.0f) > 0.5;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
            {
                Debug.Log("Section " + debugID + " will be a leaf");
                return false;
            }

            if (splitH)
            {
                int split = Random.Range(minRoomSize, (int)(rect.width - minRoomSize));

                left = new Section(new Rect(rect.x, rect.y, rect.width, split));
                right = new Section(new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            else
            {
                int split = Random.Range(minRoomSize, (int)(rect.height - minRoomSize));

                left = new Section(new Rect(rect.x, rect.y, split, rect.height));
                right = new Section(new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }
            return true;
        }
    }

    public void CreateBSP(Section section)
    {
        Debug.Log("Splitting section " + section.debugID + ": " + section.rect);
        if (section.IAmLeaf() == true)
        //If the section is too large
        {
            if (section.rect.width > maxRoomSize
                || section.rect.height > maxRoomSize
                || Random.Range(0.0f, 1.0f) > 0.25)
            {

                if (section.Split(minRoomSize, maxRoomSize))
                {
                    Debug.Log("Split section " + section.debugID + " in "
                              + section.left.debugID + ": " + section.left.rect + ", "
                              + section.right.debugID + ": " + section.right.rect);

                    CreateBSP(section.left);
                    CreateBSP(section.right);
                }
            }
        }
    }
}
