using System;
using System.Linq;
using UnityEngine;

public class GridState : MonoBehaviour
{
    public int gridDimensions;
    public float cellSize;
    public bool showGrid;

    public Vector3 CurrentCell => _currentCell;
    public Vector3 HighlightCell => _highlightCell;

    private bool _dirtyGrid;
    private GridRenderer _gridRenderer;
    private Vector3 _currentCell;
    private Vector3 _highlightCell;

    private void Start()
    {
        _gridRenderer = GetComponent<GridRenderer>();
    }

    private void Update()
    {
        if (_dirtyGrid)
        {
            _dirtyGrid = false;
            _gridRenderer.dirtyGrid = true;
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 50f, LayerMask.GetMask("Building", "Terrain")))
        {
            var hitLayer = hitInfo.transform.gameObject.layer;

            if (hitLayer == LayerMask.NameToLayer("Terrain"))
            {
                var hitPoint = hitInfo.point;
                Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
                _currentCell = newCell;
                _highlightCell = _currentCell;
            }
            else if (hitLayer == LayerMask.NameToLayer("Building"))
            {
                var hitPoint = hitInfo.transform.position;
                Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
                _currentCell = newCell;

                // Calculate most dominant normal axis
                var normal = hitInfo.normal;
                var values = new[] {Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z)};
                var maxIndex = Array.IndexOf(values, values.Max());
                var dir = Vector3.zero;
                dir[maxIndex] = normal[maxIndex];
                dir.Normalize();
                _highlightCell = _currentCell + dir;
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            _dirtyGrid = true;
        }
    }
}