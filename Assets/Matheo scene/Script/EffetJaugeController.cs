using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffetJaugeController : MonoBehaviour
{
    [SerializeField]
    private Volume globalVolume;

    [Range(0f, 1f)]
    public float valeurJauge = 1.0f;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    void Start()
    {
        if (globalVolume.profile.TryGet(out ChromaticAberration ca))
        {
            chromaticAberration = ca;
            chromaticAberration.intensity.value = 0.0f;
        }
        if (globalVolume.profile.TryGet(out Vignette vg))
        {
            vignette = vg;
            
            vignette.intensity.value = 1.0f - valeurJauge;
        }


        valeurJauge = 1.0f;
    }

    void Update()
    {
        if (chromaticAberration != null)
        {
            float intensiteVoulue = 1.0f - valeurJauge;
            chromaticAberration.intensity.value = intensiteVoulue;
        }
        if (vignette != null)
        {
            float intensiteVoulue = 1.0f - valeurJauge;
            vignette.intensity.value = intensiteVoulue;
        }
    }

    public void SetValeurJauge(float nouvelleValeur)
    {
        valeurJauge = Mathf.Clamp01(nouvelleValeur);
    }
}