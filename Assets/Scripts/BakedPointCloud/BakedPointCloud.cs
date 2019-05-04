using System.Collections.Generic;
using UnityEngine;
using VoxSlicer.Vox;

public class BakedPointCloud : ScriptableObject
{
    #region Serialized Members
    [SerializeField] private int _pointCloud;
    [SerializeField] private Texture2D _positionMap;
    [SerializeField] private Texture2D _colorMap;
    #endregion

    #region Public Properties

    public int PointCount { get { return _pointCloud; } }
    public Texture2D PositionMap { get { return _positionMap; } }
    public Texture2D ColorMap { get { return _colorMap; } }
    #endregion

    #region Public Members
      

    public void Initialize(List<Vector3> positions, List<Color> colors)
    {
        _pointCloud = positions.Count;
        int width = Mathf.CeilToInt(Mathf.Sqrt(_pointCloud));
        _positionMap = new Texture2D(width, width, TextureFormat.RGBAHalf, false);
        _positionMap.name = "Position Map";
        _positionMap.filterMode = FilterMode.Point;

        _colorMap = new Texture2D(width, width, TextureFormat.RGBA32, false);
        _colorMap.name = "Color Map";
        _colorMap.filterMode = FilterMode.Point;

        List<Vector3> checks = new List<Vector3>();

        int i1 = 0;
        uint i2 = 0U;

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = i1 < _pointCloud ? i1 : (int)(i2 % _pointCloud);

                Vector3 p = positions[i];
                checks.Add(p);

                _positionMap.SetPixel(x, y, new Color(p.x, p.y, p.z));
                _colorMap.SetPixel(x, y, colors[i]);

                i1++;
                i2 += 132049U; //prime
            }
        }

        Debug.Log(checks.Count);
        Debug.Log(positions.Count);


        _positionMap.Apply(false, true);
        _colorMap.Apply(false, true);
    }

    #endregion

}
