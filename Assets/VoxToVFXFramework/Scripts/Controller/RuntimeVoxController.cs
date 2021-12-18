using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.VFX;
using UnityEngine;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

[RequireComponent(typeof(VisualEffect))]
public class RuntimeVoxController : MonoBehaviour
{
    #region Fields

    private VisualEffect mVisualEffect;
    private GraphicsBuffer mVfxBuffer;
    private GraphicsBuffer mPaletteBuffer;
    #endregion

    #region UnityMethods

    private void Start()
    {
        mVisualEffect = GetComponent<VisualEffect>();
        mVisualEffect.enabled = false;
        VoxImporter voxImporter = new VoxImporter();
        StartCoroutine(voxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "Sydney.vox"), OnLoadProgress, OnLoadFinished));
    }

    private void OnDestroy()
    {
        mVfxBuffer?.Release();
        mVisualEffect.enabled = false;
    }

    #endregion

    #region PrivateMethods

    private void OnLoadProgress(float progress)
    {
        Debug.Log("[RuntimeVoxController] Load progress: " + progress);
    }

    private void OnLoadFinished(VoxelDataVFX voxelData)
    {
        Debug.Log("[RuntimeVoxController] OnLoadFinished: " + voxelData.Voxels.Count);
        mVfxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelData.Voxels.Count, Marshal.SizeOf(typeof(VoxelVFX)));
        mVfxBuffer.SetData(voxelData.Voxels);

        mVisualEffect.SetInt("InitialBurstCount", voxelData.Voxels.Count);
        mVisualEffect.SetGraphicsBuffer("Buffer", mVfxBuffer);

        mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelData.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
        mPaletteBuffer.SetData(voxelData.Materials);
        mVisualEffect.SetGraphicsBuffer("MaterialBuffer", mPaletteBuffer);

        mVisualEffect.enabled = true;
    }

    #endregion
}

