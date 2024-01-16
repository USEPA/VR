using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WinterboltGames.BillboardGenerator.Runtime.Utilities;
using WinterboltGames.BillboardGenerator.Editor.Utilities;
using WinterboltGames.BillboardGenerator.Runtime;
using System.Collections.Generic;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal sealed class BillboardAtlasPacker : EditorWindow
	{
		private SerializedObject _serializedObject;

		[SerializeField]
		private Texture2D[] textures;

		[SerializeField]
		private int initialBinWidth;

		[SerializeField]
		private int initialBinHeight;

		[SerializeField]
		private MaxRectsBin.FreeRectChoiceHeuristic choiceHeuristic;

		[MenuItem("Tools/Billboard Generator/Billboard Atlas Packer")]
		private static void ShowWindow()
		{
			GetWindow<BillboardAtlasPacker>().titleContent = new GUIContent("Billboard Atlas Packer");
		}

		private void OnEnable()
		{
			_serializedObject = new SerializedObject(this);
		}

		private void CreateGUI()
		{
			PropertyField textureListPropertyField = new PropertyField();

			textureListPropertyField.BindProperty(_serializedObject.FindProperty(nameof(textures)));

			PropertyField initialBinWidthPropertyField = new PropertyField();

			initialBinWidthPropertyField.BindProperty(_serializedObject.FindProperty(nameof(initialBinWidth)));

			PropertyField initialBinHeightPropertyField = new PropertyField();

			initialBinHeightPropertyField.BindProperty(_serializedObject.FindProperty(nameof(initialBinHeight)));

			PropertyField choiceHeuristicPropertyField = new PropertyField();

			choiceHeuristicPropertyField.BindProperty(_serializedObject.FindProperty(nameof(choiceHeuristic)));

			Button packButton = new Button(Pack)
			{
				text = "Pack",
			};

			rootVisualElement.Add(textureListPropertyField);

			rootVisualElement.Add(initialBinWidthPropertyField);

			rootVisualElement.Add(initialBinHeightPropertyField);

			rootVisualElement.Add(choiceHeuristicPropertyField);

			rootVisualElement.Add(packButton);
		}

		private void Pack()
		{
			(Texture2D texture, List<UVCoordinates> uvs) = PackingUtilities.PackTight(textures, choiceHeuristic, initialBinWidth, initialBinHeight);

			AtlasUtilities.CreateAndSave(texture, uvs);
		}
	}
}
