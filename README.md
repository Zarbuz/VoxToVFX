# VoxToVFX

## Requirements

- Unity 2019.1
- VFX Graph
- HDRP

## What is VoxToVFX ? 

VoxToVFX allows you to import a MagicaVoxel project into Unity using the new VFX Graph.
No mesh is created, so the import process for huge world is very quick ! All voxels are particles rendered on the GPU.
It support also world regions of MagicaVoxel so you can import world bigger than 126^3.

## How to use it ? 

You just need to put your .vox file into your project. The import process will start automatically. 
It will create two textures (Position Map and Color Map). Then you have to put a Visual Effect into your scene with the asset template "Cubes".

You might need to modify this asset template to adjust the capacity of the Initialize block.

![](img/img0.png)

![](img/img1.png)
