using UnityEngine;

public class GridSettings : MonoBehaviour
{
    public int gridDimensions;
    public float cellSize;
    public bool showGrid;

    private bool _dirtyGrid;
    private GridRenderer _gridRenderer;
    
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
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            _dirtyGrid = true;
        }
    }
}
