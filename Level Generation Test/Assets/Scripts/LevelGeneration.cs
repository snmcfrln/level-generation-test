using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    public int rows, columns;
    public int minRoomSize, maxRoomSize;

    public GameObject floorTile;
    private GameObject[,] floorPositions;

    // Use this for initialization
    void Start()
    {
        Section initialSection = new Section(new Rect(0, 0, rows, columns));
        CreateBSP(initialSection);
        initialSection.CreateRoom();

        floorPositions = new GameObject[rows, columns];
        DrawRooms(initialSection);
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

    public class Section
    {
        public Section left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0);
        public int debugID;

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
            if(IAmLeaf())
            {
                int roomWidth = (int)(Random.Range(rect.width / 2, rect.width - 2));
                int roomHeight = (int)(Random.Range(rect.height / 2, rect.height - 2));
                int roomX = (int)(Random.Range(1, rect.width - roomWidth - 1));
                int roomY = (int)(Random.Range(1, rect.height - roomHeight - 1));

                room = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);
                Debug.Log("Created room " + room + " in section " + debugID + " " + rect);
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
