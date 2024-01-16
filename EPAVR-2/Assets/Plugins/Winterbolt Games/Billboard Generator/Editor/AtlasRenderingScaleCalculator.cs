using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal sealed class AtlasRenderingScaleCalculator : EditorWindow
	{
		private SerializedObject _serializedObject;

		[SerializeField]
		private BillboardAtlasAsset atlasAsset;

		[SerializeField]
		private Texture2D atlasTexture;

		[MenuItem("Tools/Billboard Generator/Rendering Scale Calculator")]
		private static void OpenWindow()
		{
			GetWindow<AtlasRenderingScaleCalculator>().titleContent = new GUIContent("Rendering Scale Calculator");
		}

		private void OnEnable()
		{
			_serializedObject = new SerializedObject(this);

			ObjectField atlasAssetField = new ObjectField("Atlas")
			{
				objectType = typeof(BillboardAtlasAsset),
			};

			atlasAssetField.BindProperty(_serializedObject.FindProperty(nameof(atlasAsset)));

			rootVisualElement.Add(atlasAssetField);

			ObjectField atlasTextureField = new ObjectField("Texture")
			{
				objectType = typeof(Texture2D),
			};

			atlasTextureField.BindProperty(_serializedObject.FindProperty(nameof(atlasTexture)));

			rootVisualElement.Add(atlasTextureField);

			Button calculateButton = new Button(CalculateRenderingScale)
			{
				text = "Calculate",
			};

			rootVisualElement.Add(calculateButton);
		}

		private void CalculateRenderingScale()
		{
			if (atlasAsset == null)
			{
				Debug.LogError($"Unable to calculate atlas rendering scale. {nameof(atlasAsset)} is null.");

				return;
			}

			if (atlasTexture == null)
			{
				Debug.LogError($"Unable to calculate atlas rendering scale. {nameof(atlasTexture)} is null.");

				return;
			}

			int atlasWidth = atlasTexture.width;
			int atlasHeight = atlasTexture.height;

			float averageWidthToHeightRatio = atlasAsset.uvCoordinates.Average(uv => uv.UScale * atlasWidth / (uv.VScale * atlasHeight));
			float averageHeightToWidthRatio = atlasAsset.uvCoordinates.Average(uv => uv.VScale * atlasHeight / (uv.UScale * atlasWidth));

			atlasAsset.rendererScale = new Vector3(averageWidthToHeightRatio, averageHeightToWidthRatio, 1.0f);
		}
	}
}
