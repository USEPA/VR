using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParticleSystemExtension  {

	public static void EnableEmission(this ParticleSystem particleSystem, bool enabled)
	{
		var emission = particleSystem.emission;
		emission.enabled = enabled;
	}

	public static float GetEmissionRate(this ParticleSystem particleSystem)
	{
		//return particleSystem.emission.rate.constantMax;
		return particleSystem.emission.rateOverTime.constantMax;
	}

	public static float GetParticleSpeed(this ParticleSystem particleSystem)
	{
		return particleSystem.main.startSpeed.constantMax;
	}

	public static float GetParticleLifetime(this ParticleSystem particleSystem)
	{
		return particleSystem.main.startLifetime.constantMax;
	}

	public static float GetParticleSize(this ParticleSystem particleSystem)
	{
		return particleSystem.main.startSize.constantMax;
	}

	public static void SetEmissionRate(this ParticleSystem particleSystem, float emissionRate)
	{
		var emission = particleSystem.emission;
		var rate = emission.rateOverTime;

		rate.constantMax = emissionRate;
		emission.rateOverTime = rate;
	}

	public static void SetParticleLifetime(this ParticleSystem particleSystem, float lifetime)
	{
		var main = particleSystem.main;
		var ltime = main.startLifetime;
		ltime.constantMax = lifetime;
		main.startLifetime = ltime;
	}


	public static void SetParticleSpeed(this ParticleSystem particleSystem, float speed)
	{
		var main = particleSystem.main;
		var spd = main.startSpeed;
		spd.constantMax = speed;
		main.startSpeed = spd;
	}

	public static void SetParticleSize(this ParticleSystem particleSystem, float size)
	{
		var main = particleSystem.main;
		var sz = main.startSize;
		sz.constantMax = size;
		main.startSize = sz;
	}
}
