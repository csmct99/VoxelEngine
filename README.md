# Generic Voxel Engine
I wanted to try my hand at a optimized voxel engine in Unity.
Both because I want to use it for my own projects and also as a good learning project.

I consider this project a standalone package though currently its not perfectly segmented like this.

If you want, feel free to make any pull requests though as of right now this is a solo project.

![Example](https://github.com/csmct99/VoxelEngine/blob/main/PromotionalMaterial/Example.png?raw=true)

# TODO
- [X] Naive implementation
- [X] Face culling
- [X] Greedy meshing
- [ ] Multi-threaded greedy meshing
- [X] Proper chunking of data
- [ ] Option for Automatic LODing
- [ ] Binary greedy meshing
- [ ] Lighten the mesh data weight
- [ ] Record performance metrics
- [ ] First pass of terrain shader

# Technical Details
Unity: Unity 6 (6000.0.X)  
Render Pipeline: Built-in Render Pipeline  
Backend: IL2CPP  
Target: All major desktops should work \[Windows, Mac(Intel, Silicon), Linux\]  

I am using Odin Inspector for the editor tools UI and Hot Reload for faster iteration times.  
Both of these are paid assets in Unity, I have not included them in the repo for this reason.  
You will need to either cut out the odin code yourself (its not that bad) or buy the asset and install it on your side.  
Maybe one day Ill add preprocessor directives to do this automagically but that makes the code pretty icky.

