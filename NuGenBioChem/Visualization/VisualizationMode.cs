using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Types of visualizations
    /// </summary>
    public enum VisualizationMode
    {
        /// <summary>
        /// Only balls visualization
        /// </summary>
        Ball,

        /// <summary>
        /// Only sticks visualization
        /// </summary>
        Stick,

        /// <summary>
        /// In a ball-and-stick model, each atom is represented by a ball, 
        /// and chemical (usually covalent) bonds are represented by rods
        /// </summary>
        BallAndStick,

        /// <summary>
        /// Space-filling model, also known as calotte model, is a type of three-dimensional molecular model, 
        /// where the atoms are represented by spheres whose radii are proportional to the radii of the atoms, 
        /// and whose center-to-center distances are proportional to the distances between the atomic nuclei, 
        /// all in the same scale
        /// </summary>
        SpaceFilling,

        /// <summary>
        /// It is a surface that represents points of a constant 
        /// color within a volume of space
        /// </summary>
        Isosurface,

        /// <summary>
        /// "Quasi" realistic rendering
        /// </summary>
        QuasiRealistic
    }
}
