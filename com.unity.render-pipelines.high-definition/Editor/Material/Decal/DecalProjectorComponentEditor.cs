using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ShortcutManagement;
using static UnityEditorInternal.EditMode;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    [CustomEditor(typeof(DecalProjectorComponent))]
    [CanEditMultipleObjects]
    public partial class DecalProjectorComponentEditor : Editor
    {
        private MaterialEditor m_MaterialEditor = null;
        private DecalProjectorComponent m_DecalProjectorComponent = null;
        private SerializedProperty m_MaterialProperty;
        private SerializedProperty m_DrawDistanceProperty;
        private SerializedProperty m_FadeScaleProperty;
        private SerializedProperty m_UVScaleProperty;
        private SerializedProperty m_UVBiasProperty;
        private SerializedProperty m_AffectsTransparencyProperty;
        private SerializedProperty m_Size;
        private SerializedProperty m_FadeFactor;

        private DecalProjectorComponentHandle m_Handle = new DecalProjectorComponentHandle();

        private int m_LayerMask;

        const SceneViewEditMode kEditShapePreservingUV = (SceneViewEditMode)90;
        const SceneViewEditMode kEditShapeWithoutPreservingUV = (SceneViewEditMode)91;
        static readonly SceneViewEditMode[] k_EditModes = new SceneViewEditMode[]
        {
            kEditShapePreservingUV,
            kEditShapeWithoutPreservingUV
        };
        static SceneViewEditMode currentEditMode;
        static bool modeSwitched;

        static GUIContent[] k_EditLabels = null;
        static GUIContent[] editLabels => k_EditLabels ?? (k_EditLabels = new GUIContent[]
        {
            EditorGUIUtility.TrIconContent("PreMatCube", kEditShapePreservingUVTooltip),
            EditorGUIUtility.TrIconContent("SceneViewOrtho", kEditShapeWithoutPreservingUVTooltip)
        });

        static Editor owner;

        private void OnEnable()
        {
            // Create an instance of the MaterialEditor
            m_DecalProjectorComponent = (DecalProjectorComponent)target;
            m_LayerMask = m_DecalProjectorComponent.gameObject.layer;
            m_MaterialEditor = (MaterialEditor)CreateEditor(m_DecalProjectorComponent.Mat);
            m_DecalProjectorComponent.OnMaterialChange += OnMaterialChange;
            m_MaterialProperty = serializedObject.FindProperty("m_Material");
            m_DrawDistanceProperty = serializedObject.FindProperty("m_DrawDistance");
            m_FadeScaleProperty = serializedObject.FindProperty("m_FadeScale");
            m_UVScaleProperty = serializedObject.FindProperty("m_UVScale");
            m_UVBiasProperty = serializedObject.FindProperty("m_UVBias");
            m_AffectsTransparencyProperty = serializedObject.FindProperty("m_AffectsTransparency");
            m_Size = serializedObject.FindProperty("m_Size");
            m_FadeFactor = serializedObject.FindProperty("m_FadeFactor");

            owner = this;
            onEditModeStartDelegate += NotifyEnterMode;
            onEditModeEndDelegate += NotifyExitMode;
        }
        
        private void OnDisable()
        {
            m_DecalProjectorComponent.OnMaterialChange -= OnMaterialChange;

            owner = null;
            onEditModeStartDelegate -= NotifyEnterMode;
            onEditModeEndDelegate -= NotifyExitMode;
        }
        
        void NotifyEnterMode(Editor editor, SceneViewEditMode mode)
        {
            if (editor is DecalProjectorComponentEditor && !modeSwitched)
                currentEditMode = mode;
        }

        void NotifyExitMode(Editor editor)
        {
            if (editor is DecalProjectorComponentEditor && !modeSwitched)
                currentEditMode = SceneViewEditMode.None;
        }

        private void OnDestroy() =>
            DestroyImmediate(m_MaterialEditor);

        public void OnMaterialChange() =>
            // Update material editor with the new material
            m_MaterialEditor = (MaterialEditor)CreateEditor(m_DecalProjectorComponent.Mat);

        void OnSceneGUI()
        {
            DrawHandles();
            AdditionalShortcut();
        }

        void AdditionalShortcut()
        {
            var evt = Event.current;

            if(evt.shift && currentEditMode == editMode && (currentEditMode == kEditShapePreservingUV || currentEditMode == kEditShapeWithoutPreservingUV))
            {
                SceneViewEditMode targetMode;
                switch (editMode)
                {
                    case kEditShapePreservingUV:
                        targetMode = kEditShapeWithoutPreservingUV;
                        break;
                    case kEditShapeWithoutPreservingUV:
                        targetMode = kEditShapePreservingUV;
                        break;
                    default:
                        throw new System.ArgumentException("Unknown Decal edition mode");
                }
                modeSwitched = true;
                EditorApplication.delayCall += () => ChangeEditMode(targetMode, HDEditorUtils.GetBoundsGetter(owner)(), owner);
            }
            else if(!evt.shift && currentEditMode != editMode)
            {
                EditorApplication.delayCall += () => ChangeEditMode(currentEditMode, HDEditorUtils.GetBoundsGetter(owner)(), owner);
                modeSwitched = false;
            }
        }

        void DrawHandles()
        {
            var mat = Handles.matrix;
            var col = Handles.color;

            Handles.color = Color.white;
            Handles.matrix = m_DecalProjectorComponent.transform.localToWorldMatrix;
            m_Handle.center = m_DecalProjectorComponent.m_Offset;
            m_Handle.size = m_DecalProjectorComponent.m_Size;

            Vector3 boundsSizePreviousOS = m_Handle.size;
            Vector3 boundsMinPreviousOS = m_Handle.size * -0.5f + m_Handle.center;

            EditorGUI.BeginChangeCheck();
            m_Handle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // Adjust decal transform if handle changed.
                Undo.RecordObject(m_DecalProjectorComponent, "Decal Projector Change");

                m_DecalProjectorComponent.m_Size = m_Handle.size;
                m_DecalProjectorComponent.m_Offset = m_Handle.center;

                Vector3 boundsSizeCurrentOS = m_Handle.size;
                Vector3 boundsMinCurrentOS = m_Handle.size * -0.5f + m_Handle.center;

                if (editMode == kEditShapePreservingUV)
                {
                    // Treat decal projector bounds as a crop tool, rather than a scale tool.
                    // Compute a new uv scale and bias terms to pin decal projection pixels in world space, irrespective of projector bounds.
                    m_DecalProjectorComponent.m_UVScale.x *= Mathf.Max(1e-5f, boundsSizeCurrentOS.x) / Mathf.Max(1e-5f, boundsSizePreviousOS.x);
                    m_DecalProjectorComponent.m_UVScale.y *= Mathf.Max(1e-5f, boundsSizeCurrentOS.z) / Mathf.Max(1e-5f, boundsSizePreviousOS.z);

                    m_DecalProjectorComponent.m_UVBias.x += (boundsMinCurrentOS.x - boundsMinPreviousOS.x) / Mathf.Max(1e-5f, boundsSizeCurrentOS.x) * m_DecalProjectorComponent.m_UVScale.x;
                    m_DecalProjectorComponent.m_UVBias.y += (boundsMinCurrentOS.z - boundsMinPreviousOS.z) / Mathf.Max(1e-5f, boundsSizeCurrentOS.z) * m_DecalProjectorComponent.m_UVScale.y;
                }
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(m_DecalProjectorComponent))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(m_DecalProjectorComponent);
                }
            }

            // Automatically recenter our transform component if necessary.
            // In order to correctly handle world-space snapping, we only perform this recentering when the user is no longer interacting with the gizmo.
            if ((GUIUtility.hotControl == 0) && (m_DecalProjectorComponent.m_Offset != Vector3.zero))
            {
                // Both the DecalProjectorComponent, and the transform will be modified.
                // The undo system will automatically group all RecordObject() calls here into a single action.
                Undo.RecordObject(m_DecalProjectorComponent, "Decal Projector Change");

                // Re-center the transform to the center of the decal projector bounds,
                // while maintaining the world-space coordinates of the decal projector boundings vertices.
                m_DecalProjectorComponent.transform.Translate(
                    Vector3.Scale(m_DecalProjectorComponent.m_Offset, m_DecalProjectorComponent.transform.localScale),
                    Space.Self
                );

                m_DecalProjectorComponent.m_Offset = Vector3.zero;
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(m_DecalProjectorComponent))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(m_DecalProjectorComponent);
                }
            }

            Handles.matrix = mat;
            Handles.color = col;
        }

        Bounds GetBoundsGetter()
        {
            var bounds = new Bounds();
            var decalTransform = ((Component)target).transform;
            bounds.Encapsulate(decalTransform.position);
            return bounds;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DoInspectorToolbar(k_EditModes, editLabels, GetBoundsGetter, this);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_Size, kSizeContent);
            EditorGUILayout.PropertyField(m_MaterialProperty, kMaterialContent);
            EditorGUILayout.PropertyField(m_DrawDistanceProperty, kDistanceContent);

            EditorGUI.BeginChangeCheck();
            float fadeDistancePercent = m_FadeScaleProperty.floatValue * 100f;
            fadeDistancePercent = EditorGUILayout.Slider(kFadeScaleContent, fadeDistancePercent, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
                m_FadeScaleProperty.floatValue = fadeDistancePercent * 0.01f;

            EditorGUILayout.PropertyField(m_UVScaleProperty, kUVScaleContent);
            EditorGUILayout.PropertyField(m_UVBiasProperty, kUVBiasContent);

            EditorGUI.BeginChangeCheck();
            float fadePercent = m_FadeFactor.floatValue * 100f;
            fadePercent = EditorGUILayout.Slider(kFadeFactorContent, fadePercent, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
                m_FadeFactor.floatValue = fadePercent * 0.01f;

            // only display the affects transparent property if material is HDRP/decal
            if (DecalSystem.IsHDRenderPipelineDecal(m_DecalProjectorComponent.Mat.shader.name))
                EditorGUILayout.PropertyField(m_AffectsTransparencyProperty, kAffectTransparentContent);

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if(m_LayerMask != m_DecalProjectorComponent.gameObject.layer)
                m_DecalProjectorComponent.OnValidate();

            if (m_MaterialEditor != null)
            {
                // Draw the material's foldout and the material shader field
                // Required to call m_MaterialEditor.OnInspectorGUI ();
                m_MaterialEditor.DrawHeader();

                // We need to prevent the user to edit default decal materials
                bool isDefaultMaterial = false;
                var hdrp = GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset;
                if (hdrp != null)
                {
                    isDefaultMaterial = m_DecalProjectorComponent.Mat == hdrp.GetDefaultDecalMaterial();
                }
                using (new EditorGUI.DisabledGroupScope(isDefaultMaterial))
                {
                    // Draw the material properties
                    // Works only if the foldout of m_MaterialEditor.DrawHeader () is open
                    m_MaterialEditor.OnInspectorGUI();
                }
            }
        }

        [Shortcut("HDRP/Decal: Move Size Preserving UV", typeof(SceneView), KeyCode.Keypad1, ShortcutModifiers.Action)]
        static void EnterEditModePreservingUV(ShortcutArguments args)
        {
            var bounds = (owner as DecalProjectorComponentEditor).GetBoundsGetter();
            ChangeEditMode(currentEditMode = kEditShapePreservingUV, bounds, owner);
        }

        [Shortcut("HDRP/Decal: Move Size Without Preserving UV", typeof(SceneView), KeyCode.Keypad2, ShortcutModifiers.Action)]
        static void EnterEditModeWithoutPreservingUV(ShortcutArguments args)
        {
            var bounds = (owner as DecalProjectorComponentEditor).GetBoundsGetter();
            ChangeEditMode(currentEditMode = kEditShapeWithoutPreservingUV, bounds, owner);
        }
        
        [Shortcut("HDRP/Decal: Stop Editing", typeof(SceneView), KeyCode.Keypad0, ShortcutModifiers.Action)]
        static void ExitEditMode(ShortcutArguments args)
        {
            currentEditMode = SceneViewEditMode.None;
            QuitEditMode();
        }
    }
}
