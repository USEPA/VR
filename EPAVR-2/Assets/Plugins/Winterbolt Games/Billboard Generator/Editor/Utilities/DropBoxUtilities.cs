using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WinterboltGames.BillboardGenerator.Editor.Utilities
{
	internal static class DropBoxUtilities
	{
		public static T DropBoxHandler<T>(T value, Rect rect, Action<T> onDrop = null) where T : Object
		{
			Event currentEvent = Event.current;

			if (!(currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)) return value;

			if (!rect.Contains(currentEvent.mousePosition)) return value;

			Object objectReference = DragAndDrop.objectReferences[0];

			if (objectReference is T)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			else
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

				return value;
			}

			if (currentEvent.type != EventType.DragPerform) return value;

			DragAndDrop.AcceptDrag();

			currentEvent.Use();

			onDrop?.Invoke((T)objectReference);

			return objectReference as T;
		}

		private static T[] DropBoxHandler<T>(Rect rect, Action<T[]> onDrop = null) where T : Object
		{
			Event currentEvent = Event.current;

			if (!(currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)) return Array.Empty<T>();

			if (!rect.Contains(currentEvent.mousePosition)) return Array.Empty<T>();

			System.Collections.Generic.List<T> objectReferences = DragAndDrop.objectReferences.OfType<T>().ToList();

			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if (currentEvent.type != EventType.DragPerform) return Array.Empty<T>();

			DragAndDrop.AcceptDrag();

			currentEvent.Use();

			onDrop?.Invoke(objectReferences.ToArray());

			return objectReferences.ToArray();
		}

		public static void GUILayoutDropBox<T>(string label, float height = 24.0f, Action<T[]> onDrop = null) where T : Object
		{
			GUILayout.Box(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(height));

			DropBoxHandler(GUILayoutUtility.GetLastRect(), onDrop);
		}
	}
}
