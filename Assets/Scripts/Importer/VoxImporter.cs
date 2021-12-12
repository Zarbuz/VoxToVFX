//using FileToVoxCore.Vox;
//using System.Collections.Generic;
//using System.IO;
//using ColorConversion;
//using UnityEngine;
//using UnityEngine.VFX;

//[UnityEditor.AssetImporters.ScriptedImporter(1, "vox")]
//public class VoxImporter : UnityEditor.AssetImporters.ScriptedImporter
//{
//	public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
//	{
//		BakedPointCloud data = ImportAsBakedPointCloud(ctx.assetPath);
//		if (data != null)
//		{
//			ctx.AddObjectToAsset("container", data);
//			ctx.AddObjectToAsset("position", data.PositionMap);
//			ctx.AddObjectToAsset("color", data.ColorMap);
//			ctx.SetMainObject(data);
//		}
//	}

//	BakedPointCloud ImportAsBakedPointCloud(string path)
//    {
//		VoxReader voxReader = new VoxReader();
//		VoxModel model = voxReader.LoadModel(path);
//		if (model == null) return null;

//		List<Vector3> positions = new List<Vector3>();
//		List<Color> colors = new List<Color>();
//		var colorsPalette = model.Palette;

//		for (int i = 0; i < model.VoxelFrames.Count; i++)
//		{
//			VoxelData data = model.VoxelFrames[i];
//			FileToVoxCore.Schematics.Tools.Vector3 worldPositionFrame = model.TransformNodeChunks[i + 1].TranslationAt();

//			if (worldPositionFrame == FileToVoxCore.Schematics.Tools.Vector3.zero)
//				continue;

//			for (int y = 0; y < data.VoxelsTall; y++)
//			{
//				for (int z = 0; z < data.VoxelsDeep; z++)
//				{
//					for (int x = 0; x < data.VoxelsWide; x++)
//					{
//						int indexColor = data.Get(x, y, z);
//						var color = colorsPalette[indexColor];
//						if (color != FileToVoxCore.Drawing.Color.Empty)
//						{
//							positions.Add(new Vector3(z + worldPositionFrame.X, y + worldPositionFrame.Z, x + worldPositionFrame.Y));
//							colors.Add(color.ToUnityColor());
//						}
//					}
//				}
//			}

//		}

//		BakedPointCloud bakedPointCloud = ScriptableObject.CreateInstance<BakedPointCloud>();
//		bakedPointCloud.Initialize(positions, colors);
//		bakedPointCloud.name = Path.GetFileNameWithoutExtension(path);
//		return bakedPointCloud;
//	}
//}
