using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPGeneration : MonoBehaviour {
    public int rows, columns;
    public int minRoomSize, maxRoomSize;

    public static List<Rect> sectionList = new List<Rect>();
    public static List<Rect> corridorList = new List<Rect>();



    public GameObject floorTile;
    public GameObject[,] floorPositions;



    private void Start()
    {
        Section initialSection = new Section(new Rect(0, 0, rows, columns));
        Partition(initialSection);
        initialSection.CreateRoom();

        floorPositions = new GameObject[rows, columns];

       // DrawRooms(initialSection);

    }

    private void OnDrawGizmos()
    {
        foreach (Rect rect in sectionList)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f); 
            Gizmos.DrawCube(rect.center, rect.size);
        }
        foreach (Rect rect in corridorList)
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(rect.center, rect.size);
        }
    }

    public void DrawRooms(Section section)
    {
        if(section == null)
        {
            return;
        }
        if(section.IsLeaf())
        {
            for(int i = (int)section.room.x; i < section.room.xMax; i++)
            {
                for (int j = (int)section.room.y; j < section.room.yMax; j++)
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

    public class Section
    {
        public Section left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0); // i.e null
        public Rect corridor = new Rect(-1, -1, 0, 0); // i.e null

        public int sectionID;
        public static int sectionIDCounter = 0;

        public void CreateRoom()
        {
            if(left != null)
            {
                left.CreateRoom();
            }
            if(right != null)
            {
                right.CreateRoom();
            }
            if (IsLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                room = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);

                print("xMin: " + rect.xMin + "  xMax: " + rect.xMax);


                float corridorX = Random.Range(room.xMin + 1, room.xMax - 1);
                float corridorY;
                if (Random.Range(0.0f, 1.0f) > 0.5f)
                {
                    corridorY = room.yMax - 1;
                }
                else
                {
                    corridorY = room.yMin;
                }
                corridor = new Rect(corridorX, corridorY, 1, 1);

                sectionList.Add(room);
                corridorList.Add(corridor);


                Debug.Log("Created room " + room + " in section " + sectionID + " " + rect);
            }
        }

        public void CreateCorridor()
        {
            
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public Section(Rect mrect)
        {
            rect = mrect;
            sectionID = sectionIDCounter;
            sectionIDCounter++;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            //If this has already split
            if (IsLeaf() == false)
            {
                //Stop splitting
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
                Debug.Log("Section " + sectionID + " will be a Leaf");
                return false;
            }

            if (splitHorizontally == true)
            {
                int splitValue = Random.Range(minRoomSize, (int)rect.width - minRoomSize);

                left = new Section(new Rect(rect.x, rect.y, rect.width, splitValue));
                right = new Section(new Rect(rect.x, rect.y + splitValue, rect.width, rect.height - splitValue));
            }
            else
            {
                int splitValue = Random.Range(minRoomSize, (int)rect.height - minRoomSize);

                left = new Section(new Rect(rect.x, rect.y, splitValue, rect.height));
                right = new Section(new Rect(rect.x + splitValue, rect.y, rect.width - splitValue, rect.height));
                //sectionList.Add(left.rect);
                //sectionList.Add(right.rect);
            }
            return true;
        }
    }

    public void Partition(Section section)
    {
        Debug.Log("Splitting section " + section.sectionID + ": " + section.rect);
        if (section.IsLeaf() == true)
        //If the section is too large
        {
            if (section.rect.width > maxRoomSize
                || section.rect.height > maxRoomSize
                || Random.Range(0.0f, 1.0f) > 0.25)
            {

                if (section.Split(minRoomSize, maxRoomSize))
                {
                    Debug.Log("Split section " + section.sectionID + " in "
                              + section.left.sectionID + ": " + section.left.rect + ", "
                              + section.right.sectionID + ": " + section.right.rect);

                    Partition(section.left);
                    Partition(section.right);
                }
            }
        }
    }
}
