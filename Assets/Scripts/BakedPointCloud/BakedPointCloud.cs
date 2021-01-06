using System.Collections.Generic;
using UnityEngine;

public class BakedPointCloud : ScriptableObject
{
    #region Public Properties

    [field: SerializeField]
    public int PointCount { get; private set; }

    [field: SerializeField]
    public Texture2D PositionMap { get; private set; }

    [field: SerializeField]
    public Texture2D ColorMap { get; private set; }

    #endregion

	#region Public Members


	public void Initialize(List<Vector3> positions, List<Color> colors)
	{
		PointCount = positions.Count;
		int width = Mathf.CeilToInt(Mathf.Sqrt(PointCount));
		PositionMap = new Texture2D(width, width, TextureFormat.RGBAHalf, false)
		{
			name = "Position Map", filterMode = FilterMode.Point
		};

		ColorMap = new Texture2D(width, width, TextureFormat.RGBA32, false)
		{
			name = "Color Map", filterMode = FilterMode.Point
		};

		List<Vector3> checks = new List<Vector3>();

		int i1 = 0;
		uint i2 = 0U;

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int i = i1 < PointCount ? i1 : (int)(i2 % PointCount);

				Vector3 p = positions[i];
				checks.Add(p);

				PositionMap.SetPixel(x, y, new Color(p.x, p.y, p.z));
				ColorMap.SetPixel(x, y, colors[i]);

				i1++;
				i2 += 132049U; //prime
			}
		}

		Debug.Log(checks.Count);
		Debug.Log(positions.Count);


		PositionMap.Apply(false, true);
		ColorMap.Apply(false, true);
	}

	#endregion


}
