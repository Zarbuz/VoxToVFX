using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FileToVoxCore.Utils;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

[RequireComponent(typeof(VisualEffect))]
public class RuntimeVoxController : MonoBehaviour
{
    #region SerializeFields

    [SerializeField] private Transform MainCamera;

    #endregion

    #region ConstStatic

    private const string MAIN_VFX_BUFFER = "Buffer";
    private const string MATERIAL_VFX_BUFFER = "MaterialBuffer";

    #endregion

    #region Fields

    private VisualEffect mVisualEffect;
    private GraphicsBuffer mVfxBuffer;
    private GraphicsBuffer mPaletteBuffer;
    private CustomSchematic mCustomSchematic;
    private BoxCollider[] mBoxColliders;

    private Vector3 mPreviousPosition;
    private long mPreviousChunkIndex;

    #endregion


    #region UnityMethods

    private void Start()
    {
        InitBoxColliders();
        mVisualEffect = GetComponent<VisualEffect>();
        mVisualEffect.enabled = false;
        VoxImporter voxImporter = new VoxImporter();
        StartCoroutine(voxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "Sydney.vox"), OnLoadProgress, OnLoadFinished));
    }

    private void OnDestroy()
    {
        mVfxBuffer?.Release();
        mPaletteBuffer?.Release();
        Destroy(mVisualEffect);
        mVfxBuffer = null;
        mPaletteBuffer = null;
    }

    private void Update()
    {
        if (mCustomSchematic == null)
        {
            return;
        }

        if (mPreviousPosition != MainCamera.position)
        {
            mPreviousPosition = MainCamera.position;
            FastMath.FloorToInt(mPreviousPosition.x / CustomSchematic.CHUNK_SIZE, mPreviousPosition.y / CustomSchematic.CHUNK_SIZE, mPreviousPosition.z / CustomSchematic.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
            long chunkIndex = CustomSchematic.GetVoxelIndex(chunkX, chunkY, chunkZ);
            if (mPreviousChunkIndex != chunkIndex)
            {
                mPreviousChunkIndex = chunkIndex;
                CreateBoxColliderForCurrentChunk(chunkIndex);
            }
        }
    }

    #endregion

    #region PrivateMethods

    private void InitBoxColliders()
    {
        mBoxColliders = new BoxCollider[1000];
        GameObject boxColliderParent = new GameObject("BoxColliders");
        for (int i = 0; i < 1000; i++)
        {
            GameObject go = new GameObject("BoxCollider " + i);
            go.transform.SetParent(boxColliderParent.transform);
            BoxCollider boxCollider = go.AddComponent<BoxCollider>();
            mBoxColliders[i] = boxCollider;
        }
    }

    private void OnLoadProgress(float progress)
    {
        Debug.Log("[RuntimeVoxController] Load progress: " + progress);
    }

    private void OnLoadFinished(VoxelDataVFX voxelData)
    {
        List<VoxelVFX> voxels = voxelData.CustomSchematic.GetAllVoxels();
        int targetPositionX = voxelData.CustomSchematic.Width / 2;
        int targetPositionY = voxelData.CustomSchematic.Height / 2;
        int targetPositionZ = voxelData.CustomSchematic.Length / 2;
        MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);

        Debug.Log("[RuntimeVoxController] OnLoadFinished: " + voxels.Count);
        mVfxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxels.Count, Marshal.SizeOf(typeof(VoxelVFX)));
        mVfxBuffer.SetData(voxels);

        mVisualEffect.SetInt("InitialBurstCount", voxels.Count);
        mVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER, mVfxBuffer);

        mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelData.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
        mPaletteBuffer.SetData(voxelData.Materials);
        mVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER, mPaletteBuffer);

        mVisualEffect.enabled = true;
        mCustomSchematic = voxelData.CustomSchematic;
    }

    private void CreateBoxColliderForCurrentChunk(long chunkIndex)
    {
        int i = 0;
        foreach (VoxelVFX voxel in mCustomSchematic.RegionDict[chunkIndex].BlockDict.Values)
        {
            if (i < mBoxColliders.Length)
            {
                BoxCollider boxCollider = mBoxColliders[i];
                boxCollider.transform.position = voxel.position;
                i++;
            }
            else
            {
                Debug.Log("Capacity of box colliders is too small");
                break;
            }
        }
    }

    #endregion
}

