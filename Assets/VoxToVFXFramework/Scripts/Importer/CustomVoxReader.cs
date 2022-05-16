using System;
using FileToVoxCore.Vox;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Importer
{
	public class CustomVoxReader : VoxReader
	{
		public override VoxModel LoadModel(string absolutePath, bool writeLog = false, bool debug = false, bool offsetPalette = true)
		{
			VoxModelCustom output = new VoxModelCustom();
			var name = Path.GetFileNameWithoutExtension(absolutePath);
			VoxelCountLastXyziChunk = 0;
			LogOutputFile = name + "-" + DateTime.Now.ToString("y-MM-d_HH.m.s") + ".txt";
			WriteLog = writeLog;
			OffsetPalette = offsetPalette;
			ChildCount = 0;
			ChunkCount = 0;
			using (BinaryReader reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(absolutePath))))
			{
				var head = new string(reader.ReadChars(4));
				if (!head.Equals(HEADER))
				{
					Console.WriteLine("Not a Magicavoxel file! " + output);
					return null;
				}
				int version = reader.ReadInt32();
				if (version != VERSION)
				{
					Console.WriteLine("Version number: " + version + " Was designed for version: " + VERSION);
				}
				ResetModel(output);
				while (reader.BaseStream.Position != reader.BaseStream.Length)
					ReadChunk(reader, output);
			}

			if (debug)
			{
				CheckDuplicateIds(output);
				CheckDuplicateChildGroupIds(output);
				CheckTransformIdNotInGroup(output);
				Console.ReadKey();
			}


			if (output.Palette == null)
				output.Palette = LoadDefaultPalette();
			return output;
		}

		protected override void ReadSIZENodeChunk(BinaryReader chunkReader, VoxModel output)
		{
			VoxModelCustom outputCasted = output as VoxModelCustom;

			int xSize = chunkReader.ReadInt32();
			int ySize = chunkReader.ReadInt32();
			int zSize = chunkReader.ReadInt32();
			if (ChildCount >= outputCasted.VoxelFramesCustom.Count)
				outputCasted.VoxelFramesCustom.Add(new VoxelDataCustom());

			//Swap XZ
			//Dirty but resize add +1 
			outputCasted.VoxelFramesCustom[ChildCount].Resize(xSize - 1, zSize - 1, ySize - 1);
			ChildCount++;
		}

		protected override void ReadXYZINodeChunk(BinaryReader chunkReader, VoxModel output)
		{
			VoxModelCustom outputCasted = output as VoxModelCustom;
			int voxelCountLastXyziChunk = chunkReader.ReadInt32();
			VoxelDataCustom frame = outputCasted.VoxelFramesCustom[ChildCount - 1];
			frame.VoxelNativeArray = new NativeArray<byte>((frame.VoxelsWide) * (frame.VoxelsTall) * (frame.VoxelsDeep), Allocator.Persistent);
			for (int i = 0; i < voxelCountLastXyziChunk; i++)
			{
				int x = frame.VoxelsWide - 1 - chunkReader.ReadByte(); //invert
				int z = frame.VoxelsDeep - 1 - chunkReader.ReadByte(); //swapYZ //invert
				int y = chunkReader.ReadByte();
				byte color = chunkReader.ReadByte();
				if (color > 0)
				{
					frame.VoxelNativeArray[frame.GetGridPos(x, y, z)] = color;
				}
			}




		}
	}
}
