using UnityEditor;
using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime.Renderers;

namespace WinterboltGames.BillboardGenerator.Editor
{
	[CustomEditor(typeof(DirectionalBillboardRenderer))]
	[CanEditMultipleObjects]
	internal sealed class DirectionalBillboardRendererInspector : UnityEditor.Editor
	{
		private DirectionalBillboardRenderer _preview;

		private void OnEnable()
		{
			if (EditorApplication.isPlaying || !(target is DirectionalBillboardRenderer billboard) || !billboard.IsValid()) return;

			_preview = Instantiate(billboard);

			_preview.gameObject.hideFlags = HideFlags.HideAndDontSave;

			_preview.PrepareBillboard();

			EditorApplication.update += OnEditorApplicationUpdate;

			if (!_preview.useAlternativeWorkflow) SceneView.duringSceneGui += OnDuringSceneGui;
		}

		private void OnDisable()
		{
			if (_preview != null) DestroyImmediate(_preview.gameObject);

			EditorApplication.update -= OnEditorApplicationUpdate;

			SceneView.duringSceneGui -= OnDuringSceneGui;
		}

		private void OnEditorApplicationUpdate()
		{
			if (_preview == null) return;

			_preview.transform.position = ((DirectionalBillboardRenderer)target).transform.position;

			_preview.UpdateBillboard();

			EditorApplication.QueuePlayerLoopUpdate();
		}

		private void OnDuringSceneGui(SceneView sceneView)
		{
			if (_preview == null) return;

			_preview.RenderBillboard();

			sceneView.Repaint();
		}

		public override void OnInspectorGUI()
		{
			SerializedProperty useBillboardTargetProperty = serializedObject.FindProperty("useBillboardTarget");

			if (useBillboardTargetProperty.boolValue)
			{
				EditorGUILayout.HelpBox("Billboard will automatically target the transform assigned to [BillboardTarget.Transform].", MessageType.Info, true);
			}
			else
			{
				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
			}

			_ = EditorGUILayout.PropertyField(useBillboardTargetProperty);

			SerializedProperty useAlternativeWorkflowProperty = serializedObject.FindProperty("useAlternativeWorkflow");

			_ = EditorGUILayout.PropertyField(useAlternativeWorkflowProperty);

			if (useAlternativeWorkflowProperty.boolValue)
			{
				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("meshFilter"));

				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("meshRenderer"));

				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("localScale"));
			}
			else
			{
				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
			}

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("atlas"));

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BillboardRendererBase.update)));

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("render"));

			_ = serializedObject.ApplyModifiedProperties();
		}
	}
}
