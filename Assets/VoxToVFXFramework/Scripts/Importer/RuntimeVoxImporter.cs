using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Assets.VoxToVFXFramework.Scripts.Importer;
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
    private GraphicsBuffer mVfxBuffer;
    private readonly List<VoxelVFX> mVoxels = new List<VoxelVFX>();
    private VoxModel mVoxModel;
 

    #endregion


    #region UnityMethods

    private void Start()
    {
        mVisualEffect = GetComponent<VisualEffect>();
        mVisualEffect.enabled = false;

        bool result = LoadVoxModel(Path.Combine(Application.streamingAssetsPath, "test.vox"));

        if (!result)
        {
            return;
        }

        InitComputeShader();
        mVisualEffect.SetGraphicsBuffer("Buffer", mVfxBuffer);
        mVisualEffect.enabled = true;
    }

    private void OnDestroy()
    {
        mVfxBuffer?.Release();
        mVisualEffect.enabled = false;
    }

    #endregion

    #region PrivateMethods

    private bool LoadVoxModel(string path)
    {
        VoxReader voxReader = new VoxReader();
        mVoxModel = voxReader.LoadModel(path);
        if (mVoxModel == null)
        {
            return false;
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

        return true;
    }

    private void WriteVoxelFrameData(VoxelData data, TransformNodeChunk transformNodeChunk)
    {
        FileToVoxCore.Schematics.Tools.Vector3 worldPositionFrame = transformNodeChunk.TranslationAt();
        Rotation transformRotation = transformNodeChunk.RotationAt();
        Matrix4x4 matrix4X4 = ReadMatrix4X4FromRotation(transformRotation);
        Debug.Log(transformNodeChunk.RotationAt() + " " + matrix4X4);
        worldPositionFrame = new FileToVoxCore.Schematics.Tools.Vector3(worldPositionFrame.X - (data.VoxelsDeep / 2), worldPositionFrame.Y - (data.VoxelsWide / 2), worldPositionFrame.Z - (data.VoxelsTall / 2));
        for (int y = 0; y < data.VoxelsTall; y++)
        {
            for (int z = 0; z < data.VoxelsDeep; z++)
            {
                for (int x = 0; x < data.VoxelsWide; x++)
                {
                    int paletteIndex = data.Get(x, y, z);
                    Color color = paletteIndex == 0 ? Color.Empty : mVoxModel.Palette[paletteIndex - 1];

                    if (paletteIndex != 0)
                    {
                        Vector3 worldPosition = new Vector3(x + worldPositionFrame.Y, y + worldPositionFrame.Z, z + worldPositionFrame.X);
                        //worldPosition = transformRotation == Rotation._PZ_PX_P ? worldPosition : Quaternion.Euler(0, -90, 0) * worldPosition;

                        bool canAdd = false;
                        Vector3 finalColor = new Vector3(color.R / (float)255, color.G / (float)255, color.B / (float)255);
                        if (x - 1 >= 0 && x + 1 < data.VoxelsWide && y - 1 >= 0 && y + 1 < data.VoxelsTall && z - 1 >= 0 && z + 1 < data.VoxelsDeep)
                        {
                            int left = data.Get(x - 1, y, z);
                            int right = data.Get(x + 1, y, z);

                            int top = data.Get(x, y + 1, z);
                            int bottom = data.Get(x, y - 1, z);

                            int front = data.Get(x, y, z + 1);
                            int back = data.Get(x, y, z - 1);

                            if (left == 0 || right == 0 || top == 0 || bottom == 0 || front == 0 || back == 0)
                            {
                                canAdd = true;
                            }
                        }
                        else
                        {
                            canAdd = true;
                        }

                        if (canAdd)
                        {
                            mVoxels.Add(new VoxelVFX()
                            {
                                color = finalColor,
                                position = worldPosition
                            });
                        }
                    }
                }
            }
        }
    }

    private static Matrix4x4 ReadMatrix4X4FromRotation(Rotation param)
    {
        Vector3 scale;
        byte b = (byte)param;
        Matrix4x4 matrix4X4 = Matrix4x4.zero;
        int x = b & 3;
        int y = (b >> 2) & 3;
        scale.x = ((b >> 4) & 1) != 0 ? -1 : 1;
        scale.y = ((b >> 5) & 1) != 0 ? -1 : 1;
        scale.z = ((b >> 6) & 1) != 0 ? -1 : 1;
        matrix4X4[x, 0] = scale.x;
        matrix4X4[y, 1] = scale.y;
        matrix4X4[Mathf.Clamp(3 - x - y, 0, 2), 2] = scale.z;
        matrix4X4[3, 3] = 1;
        return matrix4X4;
    }


    private void InitComputeShader()
    {
        Debug.Log(mVoxels.Count);
        mVfxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mVoxels.Count, Marshal.SizeOf(typeof(VoxelVFX)));
        mVfxBuffer.SetData(mVoxels);

    }

    #endregion
}

[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
public struct VoxelVFX
{
    public Vector3 position;
    public Vector3 color;
}