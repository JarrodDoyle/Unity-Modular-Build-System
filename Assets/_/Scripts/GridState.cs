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
        var uiHit = Physics.Raycast(ray, 10f, LayerMask.GetMask("UI"));
        var terrainHit = Physics.Raycast(ray, out var terrainHitInfo, 25f, LayerMask.GetMask("Terrain"));
        var buildingHit = Physics.Raycast(ray, out var buildingHitInfo, 25f, LayerMask.GetMask("Building"));

        if (!uiHit && terrainHit && !buildingHit)
        {
            var hitPoint = terrainHitInfo.point;
            Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
            _currentCell = newCell;
            _highlightCell = _currentCell;
        }
        else if (!uiHit && buildingHit)
        {
            var hitPoint = buildingHitInfo.transform.position;
            Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
            _currentCell = newCell;

            // Calculate most dominant normal axis
            var normal = buildingHitInfo.normal;
            var values = new[] {Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z)};
            var maxIndex = Array.IndexOf(values, values.Max());
            var dir = Vector3.zero;
            dir[maxIndex] = normal[maxIndex];
            dir.Normalize();
            _highlightCell = _currentCell + dir;
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