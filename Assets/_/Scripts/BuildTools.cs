using System.Collections.Generic;
using UnityEngine;

public enum ToolType
{
    Place,
    Edit
}

public class BuildTools : MonoBehaviour
{
    private GridState _gridState;
    private GameObject _gridObjectsManager;
    private Dictionary<Vector3, GameObject> _gridObjectsMap;

    private void Start()
    {
        _gridState = GetComponent<GridState>();
        _gridObjectsManager = new GameObject("Built Objects");
        _gridObjectsManager.transform.SetParent(transform);
        _gridObjectsMap = new Dictionary<Vector3, GameObject>();
    }

    private void Update()
    {
        var cell = _gridState.CurrentCell;
        var cellBlocked = _gridObjectsMap.ContainsKey(cell);

        if (Input.GetMouseButtonDown(0) && !cellBlocked)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = _gridState.cellSize * (cell + Vector3.one / 2);
            go.transform.SetParent(_gridObjectsManager.transform);
            _gridObjectsMap.Add(cell, go);
        }

        if (Input.GetMouseButtonDown(1) && cellBlocked)
        {
            var go = _gridObjectsMap[cell];
            Destroy(go);
            _gridObjectsMap.Remove(cell);
        }
    }
}