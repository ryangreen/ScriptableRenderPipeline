using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public partial class DecalProjectorComponentEditor
    {
        const string kEditShapePreservingUVTooltip = "Modify Decal volume preserving UV.\nIn addition to customizable shortcut, you can press shift to quickly swap between too modes.";
        const string kEditShapeWithoutPreservingUVTooltip = "Modify Decal volume without preserving UV.\nIn addition to customizable shortcut, you can press shift to quickly swap between too modes.";
        
        static readonly GUIContent kSizeContent = EditorGUIUtility.TrTextContent("Size", "Sets the size of the projector.");
        static readonly GUIContent kMaterialContent = EditorGUIUtility.TrTextContent("Material", "Specifies the Material this component projects as a decal.");
        static readonly GUIContent kDistanceContent = EditorGUIUtility.TrTextContent("Draw Distance", "Sets the distance from the Camera at which HDRP stop rendering the decal.");
        static readonly GUIContent kFadeScaleContent = EditorGUIUtility.TrTextContent("Start Fade", "Controls the distance from the Camera at which this component begins to fade the decal out. Expressed as a percentage of Fade Distance.");
        static readonly GUIContent kUVScaleContent = EditorGUIUtility.TrTextContent("Tilling", "Sets the scale for the decal Material. Scales the decal along its UV axes.");
        static readonly GUIContent kUVBiasContent = EditorGUIUtility.TrTextContent("Offset", "Sets the offset for the decal Material. Moves the decal along its UV axes.");
        static readonly GUIContent kFadeFactorContent = EditorGUIUtility.TrTextContent("Fade Factor", "In Percent");
        static readonly GUIContent kAffectTransparentContent = EditorGUIUtility.TrTextContent("Affects Transparent", "When enabled, HDRP draws this projector's decal on top of transparent surfaces.");
    }
}
