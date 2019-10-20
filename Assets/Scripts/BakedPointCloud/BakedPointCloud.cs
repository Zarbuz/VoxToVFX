using System;
using System.Collections.Generic;
using UnityEngine;
using Vox;

public class BakedPointCloud : ScriptableObject
{
    #region Serialized Members
    [SerializeField] private List<int> _pointCloud;
    [SerializeField] private List<Texture2D> _positionMap;
    [SerializeField] private List<Texture2D> _colorMap;
    #endregion

    #region Public Properties

    public List<int> PointCount => _pointCloud;
    public List<Texture2D> PositionMap => _positionMap;
    public List<Texture2D> ColorMap => _colorMap;

    #endregion

    #region Public Members
      

    //public void Initialize(List<Vector3> positions, List<Color> colors)
    //{
    //    _pointCloud = positions.Count;
    //    int width = Mathf.CeilToInt(Mathf.Sqrt(_pointCloud));
    //    _positionMap = new Texture2D(width, width, TextureFormat.RGBAHalf, false);
    //    _positionMap.name = "Position Map";
    //    _positionMap.filterMode = FilterMode.Point;

    //    _colorMap = new Texture2D(width, width, TextureFormat.RGBA32, false);
    //    _colorMap.name = "Color Map";
    //    _colorMap.filterMode = FilterMode.Point;

    //    List<Vector3> checks = new List<Vector3>();

    //    int i1 = 0;
    //    uint i2 = 0U;

    //    for (int y = 0; y < width; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            int i = i1 < _pointCloud ? i1 : (int)(i2 % _pointCloud);

    //            Vector3 p = positions[i];
    //            checks.Add(p);

    //            _positionMap.SetPixel(x, y, new Color(p.x, p.y, p.z));
    //            _colorMap.SetPixel(x, y, colors[i]);

    //            i1++;
    //            i2 += 132049U; //prime
    //        }
    //    }

    //    Debug.Log(checks.Count);
    //    Debug.Log(positions.Count);


    //    _positionMap.Apply(false, true);
    //    _colorMap.Apply(false, true);
    //}

    #endregion

    public void Initialize(Dictionary<MaterialType, Tuple<List<Vector3>, List<Color>>> voxels)
    {
        _pointCloud = new List<int>(voxels.Count);
        _positionMap = new List<Texture2D>();
        _colorMap = new List<Texture2D>();

        foreach (MaterialType materialType in voxels.Keys)
        {
            int pointCloud = voxels[materialType].Item1.Count;
            _pointCloud.Add(pointCloud);

            int width = Mathf.CeilToInt(Mathf.Sqrt(voxels[materialType].Item1.Count));
            Texture2D positionTexture = new Texture2D(width, width, TextureFormat.RGBAHalf, false);
            positionTexture.name = "Position Map " + materialType;
            positionTexture.filterMode = FilterMode.Point;
            _positionMap.Add(positionTexture);

            Texture2D colorTexture = new Texture2D(width, width, TextureFormat.RGBA32, false);
            colorTexture.name = "Color Map " + materialType;
            colorTexture.filterMode = FilterMode.Point;
            _colorMap.Add(colorTexture);

            List<Vector3> checks = new List<Vector3>();

            int i1 = 0;
            uint i2 = 0U;

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = i1 < pointCloud ? i1 : (int)(i2 % pointCloud);

                    Vector3 p = voxels[materialType].Item1[i];
                    checks.Add(p);

                    positionTexture.SetPixel(x, y, new Color(p.x, p.y, p.z));
                    colorTexture.SetPixel(x, y, voxels[materialType].Item2[i]);

                    i1++;
                    i2 += 132049U; //prime
                }
            }

            positionTexture.Apply(false, true);
            colorTexture.Apply(false, true);
        }

        
    }
}
