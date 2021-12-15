using FileToVoxCore.Vox;
using FileToVoxCore.Vox.Chunks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using Color = FileToVoxCore.Drawing.Color;
using IntVector3 = Assets.VoxToVFXFramework.Scripts.Common.IntVector3;
using VoxelData = FileToVoxCore.Vox.VoxelData;

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

        bool result = LoadVoxModel(Path.Combine(Application.streamingAssetsPath, "test2.vox"));

        if (!result)
        {
            return;
        }

        InitComputeShader();
        mVisualEffect.SetInt("InitialBurstCount", mVoxels.Count);
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
        mVoxModel = voxReader.LoadModel(path, false, false, true);
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
        Matrix4x4 matrix4X4 = ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());

        IntVector3 originSize = new IntVector3(data.VoxelsWide, data.VoxelsTall, data.VoxelsDeep);
        originSize.y = data.VoxelsDeep;
        originSize.z = data.VoxelsTall;

        var pivot = new Vector3(originSize.x / 2, originSize.y / 2, originSize.z / 2);
        var fpivot = new Vector3(originSize.x / 2f, originSize.y / 2f, originSize.z / 2f);

        //worldPositionFrame = new FileToVoxCore.Schematics.Tools.Vector3(worldPositionFrame.X - (data.VoxelsDeep / 2), worldPositionFrame.Y - (data.VoxelsWide / 2), worldPositionFrame.Z - (data.VoxelsTall / 2));
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
                        IntVector3 tmpVoxel = new IntVector3(x, y, z);
                        IntVector3 origPos;
                        origPos.x = data.VoxelsWide - 1 - tmpVoxel.x; //invert
                        origPos.y = data.VoxelsDeep - 1 - tmpVoxel.z; //swapYZ //invert
                        origPos.z = tmpVoxel.y;

                        Vector3 pos = new Vector3(origPos.x + 0.5f, origPos.y + 0.5f, origPos.z + 0.5f);
                        pos -= pivot;
                        pos = matrix4X4.MultiplyPoint(pos);
                        pos += pivot;

                        pos.x += fpivot.x;
                        pos.y += fpivot.y;
                        pos.z -= fpivot.z;

                        origPos.x = Mathf.FloorToInt(pos.x);
                        origPos.y = Mathf.FloorToInt(pos.y);
                        origPos.z = Mathf.FloorToInt(pos.z);

                        tmpVoxel.x = data.VoxelsWide - 1 - origPos.x; //invert
                        tmpVoxel.z = data.VoxelsDeep - 1 - origPos.y; //swapYZ  //invert
                        tmpVoxel.y = origPos.z;


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
                                position = new Vector3(tmpVoxel.x, tmpVoxel.y, tmpVoxel.z) 
                            });
                        }
                    }
                }
            }
        }
    }

    private static Matrix4x4 ReadMatrix4X4FromRotation(Rotation rotation, FileToVoxCore.Schematics.Tools.Vector3 transform)
    {
        Matrix4x4 result = Matrix4x4.identity;
        {
            byte r = Convert.ToByte(rotation);
            int indexRow0 = (r & 3);
            int indexRow1 = (r & 12) >> 2;
            bool signRow0 = (r & 16) == 0;
            bool signRow1 = (r & 32) == 0;
            bool signRow2 = (r & 64) == 0;

            result.SetRow(0, Vector4.zero);
            switch (indexRow0)
            {
                case 0: result[0, 0] = signRow0 ? 1f : -1f; break;
                case 1: result[0, 1] = signRow0 ? 1f : -1f; break;
                case 2: result[0, 2] = signRow0 ? 1f : -1f; break;
            }
            result.SetRow(1, Vector4.zero);
            switch (indexRow1)
            {
                case 0: result[1, 0] = signRow1 ? 1f : -1f; break;
                case 1: result[1, 1] = signRow1 ? 1f : -1f; break;
                case 2: result[1, 2] = signRow1 ? 1f : -1f; break;
            }
            result.SetRow(2, Vector4.zero);
            switch (indexRow0 + indexRow1)
            {
                case 1: result[2, 2] = signRow2 ? 1f : -1f; break;
                case 2: result[2, 1] = signRow2 ? 1f : -1f; break;
                case 3: result[2, 0] = signRow2 ? 1f : -1f; break;
            }

            result.SetColumn(3, new Vector4(transform.X, transform.Y, transform.Z, 1f));
        }
        return result;
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