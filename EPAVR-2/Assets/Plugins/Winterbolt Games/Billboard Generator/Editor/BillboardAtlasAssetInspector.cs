using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	[CustomEditor(typeof(BillboardAtlasAsset))]
	[CanEditMultipleObjects]
	internal sealed class BillboardAtlasAssetInspector : UnityEditor.Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			VisualElement rootVisualElement = new VisualElement();

			PropertyField rendererScaleField = new PropertyField(serializedObject.FindProperty(nameof(BillboardAtlasAsset.rendererScale)));

			rootVisualElement.Add(rendererScaleField);

			PropertyField uvCoordinatesField = new PropertyField(serializedObject.FindProperty(nameof(BillboardAtlasAsset.uvCoordinates)));

			uvCoordinatesField.SetEnabled(false);

			rootVisualElement.Add(uvCoordinatesField);

			return rootVisualElement;
		}
	}
}
