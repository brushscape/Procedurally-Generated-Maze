using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mazeGeneration : MonoBehaviour {
    private Vector3 verticalEuler = new Vector3(0, 90, 0);
    private const int size = 8; // greater than 1
    private static readonly System.Random rnd = new System.Random();
    private HashSet<string> el = new HashSet<string>(); //holds reachable edges
    private Dictionary<string, GameObject[]> spanningTree = new Dictionary<string, GameObject[]>(); //holds edges of spanning tree and walls associated 
    private HashSet<int> openNodes = new HashSet<int>(); //hold unvisited nodes
    int currNode; //leaf node of current edge being looked at 
    bool borderAdded = false;
    int offset = 0; //controls the pace of the animation 

    // Use this for initialization
    void Start()
    {
        //add all nodes to unvisted node set 
        for (int i = 0; i < size * size; i++)
        {
            openNodes.Add(i);
        }

        //pick a random starting node and evaluate 
        currNode = rnd.Next(0, size * size);
        openNodes.Remove(currNode);
        addEdges(currNode);
    }

    // Update is called once per frame
    void Update()
    {
        //as long as there are reachable, unevaluated edges 
        if (el.Count > 0 && offset % 3 == 0)
        {
            //pick a random edge 
            string edge = getRandom();
            int space = edge.IndexOf(' ');
            int first = int.Parse(edge.Substring(0, space));
            int last = int.Parse(edge.Substring(space + 1));

            //add to spanning tree if not between 2 visited nodes
            if (openNodes.Contains(first) || openNodes.Contains(last))
            {
                int connectingNode = 0; //node attached to spanning tree 
                if (openNodes.Contains(first))
                {
                    currNode = first;
                    connectingNode = last;
                    openNodes.Remove(first);
                }
                else if (openNodes.Contains(last))
                {
                    currNode = last;
                    connectingNode = first;
                    openNodes.Remove(last);
                }
                else
                {
                    Debug.Log("Error");
                    //should be unreachable 
                }

                //find position of spanning tree edge 
                float x = first % size;
                float z = first / size;

                //create spanning tree wall and maze walls 
                GameObject spanWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spanWall.GetComponent<Renderer>().material.color = Color.black;
                spanWall.transform.localScale = new Vector3(1f, 0.5f, 0.1f);
                if (last - first > 1) // vertical 
                {
                    spanWall.transform.eulerAngles = verticalEuler;
                    spanWall.transform.position = new Vector3(x, 0, (z + 0.5f));

                    GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leftWall.transform.localScale = new Vector3(1f, 0.5f, 0.1f);
                    leftWall.transform.eulerAngles = verticalEuler;
                    leftWall.transform.position = new Vector3(x - 0.5f, 0, (z + 0.5f));
                    GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rightWall.transform.localScale = new Vector3(1f, 0.5f, 0.1f);
                    rightWall.transform.eulerAngles = verticalEuler;
                    rightWall.transform.position = new Vector3(x + 0.5f, 0, (z + 0.5f));

                    spanningTree.Add(edge, new GameObject[] { leftWall, rightWall, spanWall });

                    //offset maze walls correctly (this method just makes it look cool in real time)
                    addVerticalEdgeWalls(connectingNode, first, last, leftWall, rightWall);

                }
                else // horizontal 
                {
                    spanWall.transform.position = new Vector3(x + 0.5f, 0, z);

                    GameObject upWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    upWall.transform.localScale = new Vector3(1f, 0.5f, 0.1f);
                    upWall.transform.position = new Vector3(x + 0.5f, 0, z + 0.5f);
                    GameObject downWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    downWall.transform.localScale = new Vector3(1f, 0.5f, 0.1f);
                    downWall.transform.position = new Vector3(x + 0.5f, 0, z - 0.5f);

                    spanningTree.Add(edge, new GameObject[] { upWall, downWall, spanWall });

                    //offset maze walls correctly (this method just makes it look cool in real time)
                    addHorizontalEdgeWalls(connectingNode, first, last, upWall, downWall);
                }

                //update reachable edges 
                addEdges(currNode);
            }
        }
        //delete spanning tree walls and add border walls if algorithm complete 
        else if (!borderAdded && offset % 3 == 0)
        {
            destroyRealtimeWalls();
            addBorderWalls();
            borderAdded = true;
        }
        offset++;
    }

    //destroys all wall GameObjects stored in spanningTree
    private void destroyRealtimeWalls()
    {
        foreach (KeyValuePair<string, GameObject[]> walls in spanningTree)
        {
            if (walls.Value[0] != null)
                Destroy(walls.Value[0]);
            if (walls.Value[1] != null)
                Destroy(walls.Value[1]);
        }
    }

    //replace all realtime walls with nice, pretty, clean walls while destroying spanning tree walls 
    private void addBorderWalls()
    {
        for (int i = 0; i < size; i++)
        {
            //horizontal walls
            for (int j = 0; j < size; j++)
            {
                //is vertical edge blocking it? 
                string edge = ((j * size) + i - size) + " " + ((j * size) + i);
                if (!spanningTree.ContainsKey(edge))
                {
                    // if no, make a wall
                    GameObject horiz = horizWall(1f);
                    horiz.transform.position = new Vector3(i, 0, j - 0.5f);
                }
                else
                {
                    // if yes, destroy edge blocking it 
                    Destroy(spanningTree[edge][2]);
                }
            }

            //vertical walls
            for (int j = 0; j < size; j++)
            {
                //is horizontal edge blocking it? 
                string edge = ((j * size) + i - 1) + " " + ((j * size) + i);
                if (!spanningTree.ContainsKey(edge))
                {
                    // if no, make wall
                    GameObject vert = vertWall(1f);
                    vert.transform.position = new Vector3(i - 0.5f, 0, j);
                }
                else
                {
                    // if yes, destroy edge blocking it 
                    Destroy(spanningTree[edge][2]);
                }
            }

            GameObject right = vertWall(1f);
            right.transform.position = new Vector3(size - 0.5f, 0, i);

            GameObject top = horizWall(1f);
            top.transform.position = new Vector3(i, 0, size - 0.5f);
        }

    }

    //create a vertical wall
    private GameObject vertWall(float scale)
    {
        GameObject vert = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vert.transform.localScale = new Vector3(scale, 0.5f, 0.1f);
        vert.transform.eulerAngles = verticalEuler;
        return vert;
    }

    //create a horizontal wall 
    private GameObject horizWall(float scale)
    {
        GameObject horiz = GameObject.CreatePrimitive(PrimitiveType.Cube);
        horiz.transform.localScale = new Vector3(scale, 0.5f, 0.1f);
        return horiz;
    }

    //offset flanking walls of horizontal edge, adjusting if corner or deadend is created 
    private void addHorizontalEdgeWalls(int node, int first, int last, GameObject upWall, GameObject downWall)
    {
        string neighborEdge = node + " " + (node + size);
        if (spanningTree.ContainsKey(neighborEdge)) //bottom corner
        {
            string relEdge = (first + size) + " " + (last + size);
            if (node == first) // left corner 
            {
                handleCorner(upWall, new Vector3(0.5f, 0, 0), relEdge, spanningTree[neighborEdge][1], new Vector3(0, 0, 0.5f));
            }
            else if (node == last) // right corner 
            {
                handleCorner(upWall, new Vector3(-0.5f, 0, 0), relEdge, spanningTree[neighborEdge][0], new Vector3(0, 0, 0.5f));
            }
            else
            {
                //should be unreachable 
                Debug.Log("Error");
            }
        }

        neighborEdge = (node - size) + " " + node;
        if (spanningTree.ContainsKey(neighborEdge)) // top corner
        {
            string relEdge = (first - size) + " " + (last - size);
            if (node == first) //left corner 
            {
                handleCorner(downWall, new Vector3(0.5f, 0, 0), relEdge, spanningTree[neighborEdge][1], new Vector3(0, 0, -0.5f));
            }
            else if (node == last) // right corner 
            {
                handleCorner(downWall, new Vector3(-0.5f, 0, 0), relEdge, spanningTree[neighborEdge][0], new Vector3(0, 0, -0.5f));
            }
            else
            {
                //should be unreachable 
                Debug.Log("Error");
            }
        }
    }

    //offset flanking walls of horizontal edge, adjusting if corner or deadend is created 
    private void addVerticalEdgeWalls(int node, int first, int last, GameObject leftWall, GameObject rightWall)
    {
        string neighborEdge = node + " " + (node + 1);
        if (spanningTree.ContainsKey(neighborEdge)) //right corner
        {
            string relEdge = (first + 1) + " " + (last + 1);
            if (node == first) // bottom corner 
            {
                handleCorner(rightWall, new Vector3(0, 0, 0.5f), relEdge, spanningTree[neighborEdge][0], new Vector3(0.5f, 0, 0));
            }
            else if (node == last) // top corner 
            {
                handleCorner(rightWall, new Vector3(0, 0, -0.5f), relEdge, spanningTree[neighborEdge][1], new Vector3(0.5f, 0, 0));
            }
            else
            {
                //should be unreachable 
                Debug.Log("Error");
            }
        }

        neighborEdge = (node - 1) + " " + node;
        if (spanningTree.ContainsKey(neighborEdge)) // left corner
        {
            string relEdge = (first - 1) + " " + (last - 1);
            if (node == first) //bottom corner 
            {
                handleCorner(leftWall, new Vector3(0, 0, 0.5f), relEdge, spanningTree[neighborEdge][0], new Vector3(-0.5f, 0, 0));
            }
            else if (node == last) // top corner 
            {
                handleCorner(leftWall, new Vector3(0, 0, -0.5f), relEdge, spanningTree[neighborEdge][1], new Vector3(-0.5f, 0, 0));
            }
            else
            {
                //should be unreachable 
                Debug.Log("Error");
            }
        }
    }

    //offset or destory walls as needed
    private void handleCorner(GameObject newWall, Vector3 newWallPos, string relevantEdge, GameObject cornerWall, Vector3 cornWallPos)
    {
        newWall.transform.position += newWallPos;

        if (spanningTree.ContainsKey(relevantEdge))
        {
            //dead end, so destroy tbone wall 
            Destroy(cornerWall);
        }
        else
        {
            if (cornerWall != null)
            {
                cornerWall.transform.position += cornWallPos;
            }
        }
    }

    //return a random edge 
    private string getRandom()
    {
        string[] arr = new string[el.Count];
        el.CopyTo(arr);
        int index = rnd.Next(0, el.Count);
        string edge = arr[index];
        el.Remove(edge);
        return edge;
    }

    //add all adjacent edges of node to el 
    private void addEdges(int node)
    {
        if (node == 0) //bottom left
        {
            add("0 1");
            add("0 " + size);
        }
        else if (node == size * size - 1) //top right
        {
            add((size * size - 2) + " " + (size * size - 1));
            add((size * size - 1 - size) + " " + (size * size - 1));
        }
        else if (node == size * size - size) //top left 
        {
            add((size * size - size) + " " + (size * size - size + 1));
            add((size * size - size * 2) + " " + (size * size - size));
        }
        else if (node == size - 1) // bottom right 
        {
            add((size - 2) + " " + (size - 1));
            add((size - 1) + " " + (size * 2 - 1));
        }
        else
        {
            if (node < size) // bottom row
            {
                add(node + " " + (node + 1));
                add(node + " " + (node + size));
                add((node - 1) + " " + node);
            }
            else if (node >= size * size - size) // top row
            {
                add(node + " " + (node + 1));
                add((node - size) + " " + node);
                add((node - 1) + " " + node);
            }
            else
            {
                int pos = node % size;
                if (pos == 0) // left col 
                {
                    add(node + " " + (node + 1));
                    add(node + " " + (node + size));
                    add((node - size) + " " + node);
                }
                else if (pos == size - 1) // right col
                {
                    add((node - 1) + " " + node);
                    add(node + " " + (node + size));
                    add((node - size) + " " + node);
                }
                else // general case 
                {
                    add(node + " " + (node + 1));
                    add(node + " " + (node + size));
                    add((node - 1) + " " + node);
                    add((node - size) + " " + node);
                }
            }

        }
    }

    //skip add if edge already visited 
    private void add(string s)
    {
        if (!spanningTree.ContainsKey(s) && !el.Contains(s))
            el.Add(s);
    }
}
