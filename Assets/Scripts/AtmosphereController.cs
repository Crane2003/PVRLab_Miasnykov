using UnityEngine;
using UnityEngine.UI;

public class AtmosphereController : MonoBehaviour
{
    public Light sunLight;
    public Material skyMaterial;
    public Gradient skyColors;
    public Gradient lightColors;
    public AnimationCurve lightIntensityCurve;
    public AnimationCurve fogDensityCurve;
    public Color dayFogColor;
    public Color nightFogColor;

    [Range(6, 18)]
    public float timeOfDay;
    public Slider timeSlider;

    void Start()
    {
        timeSlider.value = timeOfDay;

        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        timeOfDay = value;
    }

    void Update()
    {
        UpdateLighting();
        UpdateSky();
        UpdateFog();
    }

    private void UpdateLighting()
    {
        sunLight.color = lightColors.Evaluate(timeOfDay / 18f);

        sunLight.intensity = lightIntensityCurve.Evaluate(timeOfDay / 18f);

        sunLight.transform.rotation = Quaternion.Euler((timeOfDay / 24f) * 360f - 90f, 170f, 0);
    }

    private void UpdateSky()
    {
        skyMaterial.SetColor("_SkyTint", skyColors.Evaluate(timeOfDay / 18f));
    }

    private void UpdateFog()
    {
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, lightIntensityCurve.Evaluate(timeOfDay / 18f));

        RenderSettings.fogDensity = fogDensityCurve.Evaluate(timeOfDay / 18f);
    }
}
