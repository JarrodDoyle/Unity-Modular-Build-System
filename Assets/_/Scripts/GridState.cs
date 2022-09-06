using UnityEngine;

public class GridState : MonoBehaviour
{
    public int gridDimensions;
    public float cellSize;
    public bool showGrid;

    public Vector3 CurrentCell => _currentCell;

    private bool _dirtyGrid;
    private GridRenderer _gridRenderer;
    private Vector3 _currentCell;

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
        
        if (Input.GetKeyDown(KeyCode.Q)) _currentCell.y -= 1;
        if (Input.GetKeyDown(KeyCode.E)) _currentCell.y += 1;
        
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, -_currentCell.y * cellSize);
        
        if (plane.Raycast(ray, out var enter))
        {
            var hitPoint = ray.GetPoint(enter);
            Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
            newCell.y = _currentCell.y;
            _currentCell = newCell;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = cellSize * (_currentCell + Vector3.one / 2);
            go.transform.SetParent(transform);
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
