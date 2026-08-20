using UnityEngine;
using UnityEngine.Rendering;

namespace Autohand
{
	[DefaultExecutionOrder(-5)]
	public class HandStabilizer : MonoBehaviour
	{
		public HandBase hand;

		private void Start()
		{
			if (!GetComponent<Camera>().enabled || hand == null)
			{
				base.enabled = false;
			}
		}

		private void OnEnable()
		{
			if (GraphicsSettings.renderPipelineAsset != null)
			{
				RenderPipelineManager.beginCameraRendering += OnPreRender;
				RenderPipelineManager.endCameraRendering += OnPostRender;
			}
		}

		private void OnDisable()
		{
			if (GraphicsSettings.renderPipelineAsset != null)
			{
				RenderPipelineManager.beginCameraRendering -= OnPreRender;
				RenderPipelineManager.endCameraRendering -= OnPostRender;
			}
		}

		private void Update()
		{
			if (hand == null)
			{
				base.enabled = false;
			}
		}

		private void OnPreRender()
		{
			if (hand.gameObject.activeInHierarchy)
			{
				hand.OnPreRender();
			}
		}

		private void OnPostRender()
		{
			if (hand.gameObject.activeInHierarchy)
			{
				hand.OnPostRender();
			}
		}

		private void OnPreRender(ScriptableRenderContext src, Camera cam)
		{
			if (hand.gameObject.activeInHierarchy)
			{
				hand.OnPreRender();
			}
		}

		private void OnPostRender(ScriptableRenderContext src, Camera cam)
		{
			if (hand.gameObject.activeInHierarchy)
			{
				hand.OnPostRender();
			}
		}
	}
}
