using JuegoCooperativo.Cuerda;
using UnityEngine;
using UnityEngine.UI;

namespace JuegoCooperativo.UI
{
    public class IndicadorTensionCuerda : MonoBehaviour
    {
        [SerializeField] private CuerdaCooperativa cuerda;
        [SerializeField] private Image relleno;
        [SerializeField] private Text texto;

        private void Update()
        {
            if (cuerda == null || relleno == null) return;

            float tension = cuerda.Tension01;
            relleno.fillAmount = tension;
            relleno.color = Color.Lerp(new Color(0.3f, 0.9f, 0.55f), new Color(1f, 0.22f, 0.12f), Mathf.InverseLerp(0.7f, 1f, tension));

            if (texto != null)
            {
                texto.text = tension > 0.88f ? "CUERDA TENSA" : "Tension de cuerda";
            }
        }
    }
}
