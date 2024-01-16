using System;
using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Utilities
{
	/// <summary>
	/// Represents a Position, a Rotation, and a Scale.
	/// </summary>
	public readonly struct RawTransform
	{
		/// <summary>
		/// The <see cref="RawTransform"/>'s position.
		/// </summary>
		public Vector3 Position { get; }

		/// <summary>
		/// The <see cref="RawTransform"/>'s rotation.
		/// </summary>
		public Quaternion Rotation { get; }

		/// <summary>
		/// The <see cref="RawTransform"/>'s scale.
		/// </summary>
		public Vector3 Scale { get; }

		/// <summary>
		/// The <see cref="RawTransform"/>'s right vector.
		/// </summary>
		public Vector3 Right { get; }

		/// <summary>
		/// The <see cref="RawTransform"/>'s up vector.
		/// </summary>
		public Vector3 Up { get; }

		/// <summary>
		/// The <see cref="RawTransform"/>'s forward vector.
		/// </summary>
		public Vector3 Forward { get; }

		public RawTransform(in Vector3? position = null, in Quaternion? rotation = null, in Vector3? scale = null)
		{
			Position = position ?? Vector3.zero;

			Rotation = rotation ?? Quaternion.identity;

			Scale = scale ?? Vector3.one;

			// TODO: Check how accurate the following are.

			Right = Rotation * Vector3.right;
			Up = Rotation * Vector3.up;
			Forward = Rotation * Vector3.forward;
		}

		public RawTransform(Transform transform, bool useLocalPosition = false, bool useLocalRotation = false, bool useLocalScale = true)
		{
			if (transform == null) throw new ArgumentNullException($"{nameof(transform)} must not be null.", nameof(transform));

			Position = useLocalPosition ? transform.localPosition : transform.position;

			Rotation = useLocalRotation ? transform.localRotation : transform.localRotation;

			Scale = useLocalScale ? transform.localScale : transform.lossyScale;

			Right = transform.right;
			Up = transform.up;
			Forward = transform.forward;
		}

		public void Deconstruct(out Vector3 position, out Quaternion rotation, out Vector3 scale)
		{
			position = Position;

			rotation = Rotation;

			scale = Scale;
		}

		/// <summary>
		/// Transforms <paramref name="point"/> from local space to world space.
		/// </summary>
		/// <param name="point">The point to transform from local space to world space.</param>
		/// <returns><paramref name="point"/> transformed from local space to world space.</returns>
		public Vector3 TransformPoint(in Vector3 point)
		{
			return Rotation * Vector3.Scale(point, Scale) + Position;
		}

		/// <summary>
		/// Transforms <paramref name="point"/> from world space to local space.
		/// </summary>
		/// <param name="point">The point to transform from world space to local space.</param>
		/// <returns><paramref name="point"/> transformed from world space to local space.</returns>
		public Vector3 InverseTransformPoint(in Vector3 point)
		{
			Vector3 difference = point - Position;

			return Quaternion.Inverse(Rotation) * new Vector3(difference.x / Scale.x, difference.y / Scale.y, difference.z / Scale.z);
		}

		/// <summary>
		/// Rotates <paramref name="transform"/> around <see cref="Position"/> on the specified <paramref name="axis"/> by <paramref name="angle"/> degrees.
		/// </summary>
		/// <param name="transform">The <see cref="RawTransform"/> to rotate.</param>
		/// <param name="axis">The axis to rotate on.</param>
		/// <param name="angle">The angle degrees to rotate by.</param>
		/// <returns><paramref name="transform"/> rotated around <see cref="Position"/> on the specified <paramref name="axis"/> by <paramref name="angle"/> degrees.</returns>
		public RawTransform RotateTransform(in RawTransform transform, in Vector3 axis, float angle)
		{
			Quaternion angleAxis = Quaternion.AngleAxis(angle, axis);

			Vector3 direction = angleAxis * (transform.Position - Position);

			Vector3 newPosition = Position + direction;

			Quaternion newRotation = transform.Rotation * (Quaternion.Inverse(transform.Rotation) * angleAxis * transform.Rotation);

			return new RawTransform(newPosition, newRotation);
		}
	}
}
