using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Sobel")]
public sealed class Sobel : CustomPostProcessVolumeComponent, IPostProcessComponent
{
	[Tooltip("Controls the intensity of the effect.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

	[Tooltip("Controls the colour of the outline.")]
	public ColorParameter outlineColour = new ColorParameter(Color.black);

	[Tooltip("Controls the thickness of the outline.")]
	public FloatParameter outlineThickness = new FloatParameter(1f);

	[Tooltip("Linearly scales the depth calculation.")]
	public FloatParameter depthMultiplier = new FloatParameter(1f);

	[Tooltip("Bias (ie. power) applied to the scaled depth value.")]
	public FloatParameter depthBias = new FloatParameter(1f);

	[Tooltip("Linearly scales the normal calculation.")]
	public FloatParameter normalMultiplier = new FloatParameter(1f);

	[Tooltip("Bias (ie. power) applied to the scaled normal value.")]
	public FloatParameter normalBias = new FloatParameter(1f);

	Material m_Material;

	public bool IsActive() => m_Material != null && intensity.value > 0f;

	// Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Settings).
	public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	const string kShaderName = "Hidden/Shader/Sobel";

	public override void Setup()
	{
		if (Shader.Find(kShaderName) != null)
			m_Material = new Material(Shader.Find(kShaderName));
		else
			Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Sobel is unable to load.");
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;

		m_Material.SetFloat("_Intensity", intensity.value);
		m_Material.SetColor("_Colour", outlineColour.value);
		m_Material.SetFloat("_Thickness", outlineThickness.value);
		m_Material.SetFloat("_DepthMultiplier", depthMultiplier.value);
		m_Material.SetFloat("_DepthBias", depthBias.value);
		m_Material.SetFloat("_NormalMultiplier", normalMultiplier.value);
		m_Material.SetFloat("_NormalBias", normalBias.value);
		cmd.Blit(source, destination, m_Material, 0);
	}

	public override void Cleanup()
	{
		CoreUtils.Destroy(m_Material);
	}
}