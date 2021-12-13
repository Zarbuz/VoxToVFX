using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ColorConversion;
using FileToVoxCore.Vox;
using FileToVoxCore.Vox.Chunks;
using UnityEngine;
using UnityEngine.VFX;
using Color = FileToVoxCore.Drawing.Color;

[RequireComponent(typeof(VisualEffect))]
public class RuntimeVoxImporter : MonoBehaviour
{
    #region Fields

    private VisualEffect mVisualEffect;
    private GraphicsBuffer mGraphicsBuffer;
    private List<VoxelVFX> mVoxels = new List<VoxelVFX>();
    private VoxModel mVoxModel;
    #endregion

    #region ConstStatic

    private const int MAX_RANGE = 256;

    #endregion

    #region UnityMethods

    private void Start()
    {
        mVisualEffect = GetComponent<VisualEffect>();

        VoxReader voxReader = new VoxReader();
        mVoxModel = voxReader.LoadModel(Path.Combine(Application.streamingAssetsPath, "test.vox"));
        if (mVoxModel == null)
        {
            return;
        }

        for (int i = 0; i < mVoxModel.TransformNodeChunks.Count; i++)
        {
            TransformNodeChunk transformNodeChunk = mVoxModel.TransformNodeChunks[i];
            Debug.Log(transformNodeChunk.FrameAttributes.Length);
            int childId = transformNodeChunk.ChildId;

            GroupNodeChunk groupNodeChunk = mVoxModel.GroupNodeChunks.FirstOrDefault(grp => grp.Id == childId);
            if (groupNodeChunk != null)
            {
            }
            else
            {
                ShapeNodeChunk shapeNodeChunk = mVoxModel.ShapeNodeChunks.FirstOrDefault(shp => shp.Id == childId);
                if (shapeNodeChunk == null)
                {
                    Debug.LogError("Failed to find chunk with ID: " + childId);
                }
                else
                {
                    foreach (ShapeModel shapeModel in shapeNodeChunk.Models)
                    {
                        int modelId = shapeModel.ModelId;
                        VoxelData voxelData = mVoxModel.VoxelFrames[modelId];
                        WriteVoxelFrameData(voxelData, transformNodeChunk);
                    }
                }
            }
        }

        Debug.Log("Voxel count: " + mVoxels.Count);
        mGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mVoxels.Count, Marshal.SizeOf(typeof(VoxelVFX)));
        mGraphicsBuffer.SetData(mVoxels);

        mVisualEffect.SetGraphicsBuffer("Buffer", mGraphicsBuffer);
	}


    private void WriteVoxelFrameData(VoxelData data, TransformNodeChunk transformNodeChunk)
    {
        FileToVoxCore.Schematics.Tools.Vector3 worldPositionFrame = transformNodeChunk.TranslationAt();
        Debug.Log(transformNodeChunk.RotationAt());
        Rotation transformRotation = transformNodeChunk.RotationAt();
        worldPositionFrame = new FileToVoxCore.Schematics.Tools.Vector3(worldPositionFrame.X - (data.VoxelsDeep / 2), worldPositionFrame.Y - (data.VoxelsWide / 2), worldPositionFrame.Z - (data.VoxelsTall / 2));
        for (int y = 0; y < data.VoxelsTall; y++)
        {
            for (int z = 0; z < data.VoxelsDeep; z++)
            {
                for (int x = 0; x < data.VoxelsWide; x++)
                {
                    int indexColor = data.Get(x, y, z);
                    var color = mVoxModel.Palette[indexColor];
                    if (indexColor != 0)
                    {
                        mVoxels.Add(new VoxelVFX()
                        {
                            color = new Vector3(color.R / (float)255, color.G / (float)255, color.B / (float)255),
                            position = new Vector3(x + worldPositionFrame.Y, y + worldPositionFrame.Z, z + worldPositionFrame.X)
                        });
                    }
                }
            }
        }
    }

    public static Quaternion GetTransform(Rotation r, out Vector3 scale)
    {
        byte b = (byte)r;
        Matrix4x4 m = Matrix4x4.zero;
        int x = b & 3;
        int y = (b >> 2) & 3;
        scale.x = ((b >> 4) & 1) != 0 ? -1 : 1;
        scale.y = ((b >> 5) & 1) != 0 ? -1 : 1;
        scale.z = ((b >> 6) & 1) != 0 ? -1 : 1;
        m[x, 0] = scale.x;
        m[y, 1] = scale.y;
        m[Mathf.Clamp(3 - x - y, 0, 2), 2] = scale.z;
        m[3, 3] = 1;
        //Debug.Log($"{r} ({b})↓\r\n{m}");
        //Debug.Log($"lossyScale={m.lossyScale}");
        //Debug.Log($"scale={scale}");
        scale = m.lossyScale;
        return m.rotation;
    }

    private void OnDestroy()
    {
        mGraphicsBuffer?.Dispose();
    }

    #endregion
}

[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
public struct VoxelVFX
{
    public Vector3 position;
    public Vector3 color;
}