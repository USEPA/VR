using System;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using WinterboltGames.BillboardGenerator.Runtime.Utilities;
using Object = UnityEngine.Object;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	/// <summary>
	/// Utility class that contains the required methods to generate billboards.
	/// </summary>
	public static class Generator
	{
		/// <summary>
		/// The <see cref="GameObject"/> of the camera used by the generator to render billboards.
		/// </summary>
		private static GameObject _cameraGameObject;

		/// <summary>
		/// The camera used by the generator to render billboards.
		/// </summary>
		private static Camera _camera;

		/// <summary>
		/// The render texture used by the generator to store camera capture results.
		/// </summary>
		private static RenderTexture _renderTexture;

		/// <summary>
		/// Prepares the generator.
		/// </summary>
		/// <param name="settings">The settings to use when preparing the generator.</param>
		private static void Prepare(in RawTransform transform, in GeneratorSettings settings)
		{
			if (settings.useMainCamera)
			{
				if (Camera.main == null)
				{
					throw new MissingReferenceException("No camera tagged 'MainCamera' is present in the current scene.");
				}
				else
				{
					Camera mainCameraClone = Object.Instantiate(Camera.main);

					_cameraGameObject = mainCameraClone.gameObject;

					_camera = mainCameraClone;
				}
			}
			else
			{
				_cameraGameObject = new GameObject("Billboard Camera");

				_camera = _cameraGameObject.AddComponent<Camera>();

				_camera.clearFlags = settings.cameraClearFlags;

				_camera.backgroundColor = settings.textureBackgroundColor;

				_camera.depth = -1;

				if (_camera.orthographic = settings.useOrthographicCamera)
				{
					_camera.orthographicSize = settings.cameraSizeOrFieldOfView;
				}
				else
				{
					_camera.fieldOfView = settings.cameraSizeOrFieldOfView;
				}

				Vector3 position = transform.TransformPoint(settings.cameraOffset);

				_camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(transform.Position - position, transform.Up));
			}

			_renderTexture = RenderTexture.GetTemporary(settings.textureWidth, settings.textureHeight);

			RenderTexture.active = _renderTexture;

			_camera.targetTexture = _renderTexture;
		}

		/// <summary>
		/// Generates billboard textures.
		/// </summary>
		/// <param name="targetGameObject">The gameObject to generate billboards for.</param>
		/// <param name="settings">The settings to use when generating billboards for <paramref name="targetGameObject"/>.</param>
		/// <returns>An array containing the generated billboard textures.</returns>
		public static Texture2D[] Generate(in RawTransform transform, in GeneratorSettings settings)
		{
			try
			{
				Prepare(transform, settings);

				float angle = 360.0f / settings.directions;

				Texture2D[] textures = new Texture2D[settings.directions];

				Rect source = new Rect(0.0f, 0.0f, settings.textureWidth, settings.textureHeight);

				for (int i = 0; i < settings.directions; i++)
				{
					_camera.Render();

					Texture2D texture = new Texture2D(settings.textureWidth, settings.textureHeight);

					texture.ReadPixels(source, 0, 0);

					texture.Apply();

					RawTransform result = transform.RotateTransform(new RawTransform(_camera.transform), transform.Up, angle);

					_camera.transform.SetPositionAndRotation(result.Position, result.Rotation);

					textures[i] = texture;
				}

				return textures;
			}
			catch
			{
				throw;
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Captures an image of a the <paramref name="targetGameObject"/> asynchronously
		/// without performing extra any operations and writes it to a PNG file.
		/// </summary>
		/// <param name="targetGameObject">The <see cref="GameObject"/> to capture an image of.</param>
		/// <param name="settings">The <see cref="GeneratorSettings"/> to use when capturing.</param>
		/// <param name="outputFolderPath">The path to the folder to store the resulting PNG image file in.</param>
		public static void CaptureSingleShotAsync(in RawTransform transform, in GeneratorSettings settings, string outputFolderPath)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(outputFolderPath)) throw new ArgumentException($"Empty '{nameof(outputFolderPath)}'.", nameof(outputFolderPath));

				Prepare(transform, settings);

				int width = settings.textureWidth;
				int height = settings.textureHeight;

				RenderTexture renderTexture = RenderTexture.GetTemporary(width, width);

				NativeArray<byte> buffer = new NativeArray<byte>(width * height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

				_camera.targetTexture = renderTexture;

				_camera.Render();

				_camera.targetTexture = null;

				_ = AsyncGPUReadback.RequestIntoNativeArray(ref buffer, renderTexture, 0, request =>
				{
					if (request.hasError) throw new Exception($"Error reading from the GPU.");

					File.WriteAllBytes(Path.Combine(outputFolderPath, $"{DateTime.Now.GetHashCode()}_Captured_Texture.png"), ImageConversion.EncodeNativeArrayToPNG(buffer, renderTexture.graphicsFormat, (uint)width, (uint)height).ToArray());
				});

				AsyncGPUReadback.WaitAllRequests();

				RenderTexture.ReleaseTemporary(renderTexture);

				buffer.Dispose();
			}
			catch
			{
				throw;
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Disposes <seealso cref="_renderTexture"/> then destroys <see cref="_cameraGameObject"/>.
		/// </summary>
		private static void CleanUp()
		{
			try
			{
				RenderTexture.active = null;

				if (_camera != null)
				{
					_camera.targetTexture = null;

#if UNITY_EDITOR

					if (Application.isPlaying)
					{
						Object.Destroy(_cameraGameObject);
					}
					else
					{
						Object.DestroyImmediate(_cameraGameObject);
					}

#else

					Object.Destroy(_cameraGameObject);

#endif

				}

				if (_renderTexture != null) RenderTexture.ReleaseTemporary(_renderTexture);
			}
			catch
			{
				throw;
			}
		}
	}
}
