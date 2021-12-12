using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ColorConversion;
using FileToVoxCore.Vox;
using UnityEngine;
using UnityEngine.VFX;
using Color = FileToVoxCore.Drawing.Color;

[RequireComponent(typeof(VisualEffect))]
public class RuntimeVoxImporter : MonoBehaviour
{
    #region Fields

    private VisualEffect mVisualEffect;
    private GraphicsBuffer mGraphicsBuffer;
    #endregion

    
    #region UnityMethods

    private void Start()
    {
        mVisualEffect = GetComponent<VisualEffect>();

        VoxReader voxReader = new VoxReader();
        VoxModel model = voxReader.LoadModel(Path.Combine(Application.streamingAssetsPath, "test.vox"));
        if (model == null)
        {
            return;
        }

        Color[] colorsPalette = model.Palette;

        List<VoxelVFX> voxels = new List<VoxelVFX>();
        for (int i = 0; i < model.VoxelFrames.Count; i++)
        {
            VoxelData data = model.VoxelFrames[i];
            FileToVoxCore.Schematics.Tools.Vector3 worldPositionFrame = model.TransformNodeChunks[i + 1].TranslationAt();

            if (worldPositionFrame == FileToVoxCore.Schematics.Tools.Vector3.zero)
                continue;

            for (int y = 0; y < data.VoxelsTall; y++)
            {
                for (int z = 0; z < data.VoxelsDeep; z++)
                {
                    for (int x = 0; x < data.VoxelsWide; x++)
                    {
                        int indexColor = data.Get(x, y, z);
                        var color = colorsPalette[indexColor];
                        if (indexColor != 0)
                        {
                            voxels.Add(new VoxelVFX()
                            {
                                color = new Vector3(color.R / (float)255, color.G / (float)255, color.B / (float)255),
                                position = new Vector3(z + worldPositionFrame.X, y + worldPositionFrame.Z, x + worldPositionFrame.Y)
                            });
                        }
                    }
                }
            }

        }

        Debug.Log("Voxel count: " + voxels.Count);
        mGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxels.Count, Marshal.SizeOf(typeof(VoxelVFX)));
        mGraphicsBuffer.SetData(voxels);

        mVisualEffect.SetGraphicsBuffer("Buffer", mGraphicsBuffer);
	}


    #endregion
}

[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
public struct VoxelVFX
{
    public Vector3 position;
    public Vector3 color;
}