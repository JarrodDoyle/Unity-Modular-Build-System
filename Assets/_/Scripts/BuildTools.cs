
using UnityEngine;

public enum ToolType
{
    Place,
    Edit
}

public class BuildTools : MonoBehaviour
{
    private GridState _gridState;
    private GameObject _gridObjects;

    private void Start()
    {
        _gridState = GetComponent<GridState>();
        _gridObjects = new GameObject("Building Elements");
        _gridObjects.transform.SetParent(transform);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = _gridState.cellSize * (_gridState.CurrentCell + Vector3.one / 2);
            go.transform.SetParent(_gridObjects.transform);
        }
    }
}
