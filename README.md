# 580RTXProject
Beyond Real-time Ray Tracing - Simulating Global illumination in Complex Scenes with Photon Maps

Team: 
Shicheng Chu, Lance Newby, ShengKai Tang, Linda Wong



Problem: 
There are some global illumination effects that are not handled by general ray tracer and shading algorithms, such as interreflections, caustics, color bleeding, participating media,
subsurface scattering, and motion blur in 3D scenes with complex geometry. We would like to improve the result of general ray tracing rendering in complex scenes.

Solution: 
We will create a custom real-time ray tracing and shading algorithm extended with photon maps in Unity to demonstrate how simple data structures can efficiently improve rendering of complex with high-fidelity global illumination effects.

Goal:
We will compare the results of rendering a complex scene through real-time ray tracing with and without photon mapping extension. The general ray tracing and shading algorithm should produce scenes rendered with poor global illumination effects. In contrast, the enhanced algorithm with photon mapping extension will greatly improve rendered by handling many effects, such as interreflections, caustics, color bleeding, participating media, subsurface scattering, and motion blur to provide a far more accurate simulation of real-world lighting.

Citations:  
Jensen, Henrik. Christensen, Per. “High Quality Rendering using Ray Tracing and Photon Mapping.” Siggraph 2007 Course 8, University of California, San Diego and Pixar Animation Studios. 5 August 2007. Lecture. 

Jensen, Henrik. Christensen, Niels. “A Practical Guide to Global Illumination using Photon Maps.” Siggraph 2000 Course 8, Stanford University and Technical University of Denmark. Monday, 23 July 2000. Lecture.

"Photon mapping" Wikipedia: The Free Encyclopedia. Wikipedia, The Free Encyclopedia, 2 August 2018. Web. 31 October 2018, en.wikipedia.org/wiki/Photon_mapping

"Ray tracing (graphics)" Wikipedia: The Free Encyclopedia. Wikipedia, The Free Encyclopedia, 29 October 2018. Web. 31 October 2018, en.wikipedia.org/wiki/Ray_tracing_(graphics)
