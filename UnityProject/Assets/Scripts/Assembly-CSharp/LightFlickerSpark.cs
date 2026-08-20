using UnityEngine;

public class LightFlickerSpark : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem m_particleSystem;

	[SerializeField]
	private Light m_light;

	[SerializeField]
	private Material m_emissiveMaterial;

	[SerializeField]
	private float m_normalFliclerTime = 0.5f;

	[SerializeField]
	private bool m_isFlickering;

	[SerializeField]
	private float m_flickerDuration;

	private AudioSource m_audioSource;

	private Color m_originalEmmisionColor;

	private float currentTimer;

	private float durationTimer;

	private Color m_offColor = new Color(0f, 0f, 0f, 0f);

	private bool m_isLightOn = true;

	private void Start()
	{
		currentTimer = m_normalFliclerTime;
		m_originalEmmisionColor = m_emissiveMaterial.GetColor("_EmissionColor");
		m_audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (m_isFlickering)
		{
			if (durationTimer > 0f)
			{
				durationTimer -= Time.deltaTime;
			}
			else
			{
				m_emissiveMaterial.SetColor("_EmissionColor", m_originalEmmisionColor);
				m_light.intensity = 1.25f;
				m_isLightOn = true;
				m_isFlickering = false;
				currentTimer = Random.Range(4f, 7f);
				m_particleSystem.gameObject.SetActive(value: false);
			}
		}
		if (currentTimer > 0f)
		{
			currentTimer -= Time.deltaTime;
		}
		else if (!m_isFlickering)
		{
			m_isFlickering = true;
			currentTimer = Random.Range(0.005f, 0.12f);
			durationTimer = m_flickerDuration;
			m_particleSystem.gameObject.SetActive(value: true);
			m_audioSource.Play();
		}
		else if (m_isLightOn)
		{
			m_emissiveMaterial.SetColor("_EmissionColor", m_offColor);
			currentTimer = Random.Range(0.005f, 0.12f);
			m_light.intensity = 0f;
			m_isLightOn = false;
		}
		else
		{
			m_emissiveMaterial.SetColor("_EmissionColor", m_originalEmmisionColor);
			currentTimer = Random.Range(0.005f, 0.12f);
			m_light.intensity = 1.25f;
			m_isLightOn = true;
		}
	}

	private void OnApplicationQuit()
	{
		m_emissiveMaterial.SetColor("_EmissionColor", m_originalEmmisionColor);
	}
}
