using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VoxSlicer.Vox;

[ScriptedImporter(1, "vox")]
public class VoxImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        BakedPointCloud data = ImportAsBakedPointCloud(ctx.assetPath);
        if (data != null)
        {
            ctx.AddObjectToAsset("container", data);
            ctx.AddObjectToAsset("position", data.PositionMap);
            ctx.AddObjectToAsset("color", data.ColorMap);
            ctx.SetMainObject(data);
        }
    }

    BakedPointCloud ImportAsBakedPointCloud(string path)
    {
        VoxReader voxReader = new VoxReader();
        VoxModel model = voxReader.LoadModel(path);
        if (model == null) return null;

        List<Vector3> positions = new List<Vector3>();
        List<Color> colors = new List<Color>();
        System.Drawing.Color[] colorsPalette = model.palette;

        Debug.Log("Frames: " + model.voxelFrames.Count);
        Debug.Log("Transform Nodes: " + model.transformNodeChunks.Count);

        for (int i = 0; i < model.voxelFrames.Count; i++)
        {
            VoxelData data = model.voxelFrames[i];
            Vector3 worldPositionFrame = model.transformNodeChunks[i + 1].TranslationAt();
            Debug.Log(worldPositionFrame);

            if (worldPositionFrame == Vector3.zero)
                continue;


            for (int y = 0; y < data.VoxelsTall; y++)
            {
                for (int z = 0; z < data.VoxelsDeep; z++)
                {
                    for (int x = 0; x < data.VoxelsWide; x++)
                    {
                        int indexColor = data.Get(x, y, z);
                        System.Drawing.Color color = colorsPalette[indexColor];
                        if (!color.IsEmpty)
                        {
                            positions.Add(new Vector3(z + worldPositionFrame.x, y + worldPositionFrame.z, x + worldPositionFrame.y));
                            colors.Add(new Color32(color.R, color.G, color.B, color.A));
                        }
                    }
                }
            }

        }

        BakedPointCloud bakedPointCloud = ScriptableObject.CreateInstance<BakedPointCloud>();
        bakedPointCloud.Initialize(positions, colors);
        bakedPointCloud.name = Path.GetFileNameWithoutExtension(path);
        return bakedPointCloud;
    }
}
