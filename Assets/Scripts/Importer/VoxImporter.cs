using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using Vox;

namespace Importer
{
    [ScriptedImporter(1, "vox")]
    public class VoxImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            BakedPointCloud data = ImportAsBakedPointCloud(ctx.assetPath);
            if (data != null)
            {
                ctx.AddObjectToAsset("container", data);
                foreach (Texture2D colorMap in data.ColorMap)
                {
                    ctx.AddObjectToAsset("color" + colorMap.name, colorMap);
                }

                foreach (Texture2D positionMap in data.PositionMap)
                {
                    ctx.AddObjectToAsset("position" + positionMap.name, positionMap);
                }
                ctx.SetMainObject(data);
            }
        }

        BakedPointCloud ImportAsBakedPointCloud(string path)
        {
            VoxReader voxReader = new VoxReader();
            VoxModel model = voxReader.LoadModel(path);
            if (model == null) return null;

            Dictionary<MaterialType, Tuple<List<Vector3>, List<Color>>> voxels = new Dictionary<MaterialType, Tuple<List<Vector3>, List<Color>>>();
            voxels.Add(MaterialType._diffuse,
                new Tuple<List<Vector3>, List<Color>>(new List<Vector3>(), new List<Color>()));
            Color[] colorsPalette = model.palette;

            Debug.Log("Frames: " + model.voxelFrames.Count);
            Debug.Log("Transform Nodes: " + model.transformNodeChunks.Count);
            //Parallel.For(0, model.voxelFrames.Count, i =>
            //{
            //    VoxelData data = model.voxelFrames[i];
            //    Vector3 worldPositionFrame = model.transformNodeChunks[i + 1].TranslationAt();

            //    if (worldPositionFrame == Vector3.zero)
            //        return;

            //    for (int y = 0; y < data.VoxelsTall; y++)
            //    {
            //        for (int z = 0; z < data.VoxelsDeep; z++)
            //        {
            //            for (int x = 0; x < data.VoxelsWide; x++)
            //            {
            //                int indexColor = data.Get(x, y, z);

            //                System.Drawing.Color color = colorsPalette[indexColor];
            //                if (!color.IsEmpty)
            //                {
            //                    //MaterialType materialType = model.materialChunks.First(t => t.id == indexColor).Type;

            //                    //if (!voxels.ContainsKey(materialType))
            //                    //{
            //                    //    voxels.TryAdd(materialType,
            //                    //        new Tuple<List<Vector3>, List<Color>>(new List<Vector3>(), new List<Color>()));
            //                    //}



            //                    voxels[MaterialType._diffuse].Item1.Add(new Vector3(z + worldPositionFrame.x,
            //                        y + worldPositionFrame.z, x + worldPositionFrame.y));
            //                    voxels[MaterialType._diffuse].Item2.Add(new Color32(color.R, color.G, color.B, color.A));
            //                }
            //            }
            //        }
            //    }
            //});
            for (int i = 0; i < model.voxelFrames.Count; i++)
            {
                VoxelData data = model.voxelFrames[i];
                Vector3 worldPositionFrame = model.transformNodeChunks[i + 1].TranslationAt();

                if (worldPositionFrame == Vector3.zero)
                    continue;

                for (int y = 0; y < data.VoxelsTall; y++)
                {
                    for (int z = 0; z < data.VoxelsDeep; z++)
                    {
                        for (int x = 0; x < data.VoxelsWide; x++)
                        {
                            int indexColor = data.Get(x, y, z);

                            Color32 color = colorsPalette[indexColor];
                            if (color != Color.clear)
                            {
                                MaterialType materialType = model.materialChunks.First(t => t.id == indexColor).Type;

                                if (!voxels.ContainsKey(materialType))
                                {
                                    voxels.Add(materialType,
                                        new Tuple<List<Vector3>, List<Color>>(new List<Vector3>(), new List<Color>()));
                                }
                                voxels[materialType].Item1.Add(new Vector3(z + worldPositionFrame.x,
                                y + worldPositionFrame.z, x + worldPositionFrame.y));
                                voxels[materialType].Item2.Add(new Color32(color.r, color.g, color.b, color.a));
                            }
                        }
                    }
                }

            }

            BakedPointCloud bakedPointCloud = ScriptableObject.CreateInstance<BakedPointCloud>();
            bakedPointCloud.Initialize(voxels);
            bakedPointCloud.name = Path.GetFileNameWithoutExtension(path);
            return bakedPointCloud;
        }
    }
}
