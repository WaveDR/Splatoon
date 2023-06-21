using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Node(GameObject _node, int _x, int _y) { node = _node; x = _x; y = _y; } 
    public GameObject node;
    public int x;
    public int y;

}

[System.Serializable]
public class Floor_Creater : MonoBehaviour
{
    public Vector2Int top_Right, bottom_Left;
    private GameObject[] node_Obj;
    [SerializeField] private GameObject node_Prefab;
    [SerializeField] private List<Node> node = new List<Node>();
    [SerializeField] private int node_poolCount;

    public Node[,] node_Array;
    private Vector3 _poolVec;

    int sizeX = 2;
    int sizeY = 2;
    private int nodePool_Num;
    // Update is called once per frame

    private void Awake()
    {
        _poolVec = new Vector3(500, 500, 500);
        node_Obj = new GameObject[node_poolCount];

        for (int i = 0; i < node_Obj.Length; i++)
        {
            node_Obj[i] = Instantiate(node_Prefab, _poolVec, Quaternion.identity, transform);
            node_Obj[i].SetActive(false);
        }
        Node_ArraySet();
    }

    void Node_ArraySet()
    {
        sizeX = top_Right.x - bottom_Left.x + 1;
        sizeY = top_Right.y - bottom_Left.y + 1;
        node_Array = new Node[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                node_Obj[nodePool_Num].SetActive(true);
                node_Array[i, j] = new Node(node_Obj[nodePool_Num], i, j);
                node_Obj[nodePool_Num].transform.position = new Vector3( i * 3, 0, j * 3);
                nodePool_Num++;
            }
        }
    }
    void Update()
    {
        
    }
}
