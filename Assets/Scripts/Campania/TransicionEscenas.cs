using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JuegoCooperativo.Campania
{
    public class TransicionEscenas : MonoBehaviour
    {
        [SerializeField] private float duracionFundido = 0.45f;

        private CanvasGroup fundido;
        private bool cargando;

        public bool EstaCargando => cargando;

        private void Awake()
        {
            CrearFundido();
            StartCoroutine(Aparecer());
        }

        public void Cargar(string escena)
        {
            if (cargando || string.IsNullOrWhiteSpace(escena)) return;
            StartCoroutine(CargarRutina(escena));
        }

        private IEnumerator Aparecer()
        {
            fundido.alpha = 1f;
            yield return CambiarAlpha(1f, 0f);
            fundido.blocksRaycasts = false;
        }

        private IEnumerator CargarRutina(string escena)
        {
            cargando = true;
            fundido.blocksRaycasts = true;
            yield return CambiarAlpha(fundido.alpha, 1f);

            Time.timeScale = 1f;
            AsyncOperation carga = SceneManager.LoadSceneAsync(escena);
            while (carga != null && !carga.isDone) yield return null;
        }

        private IEnumerator CambiarAlpha(float desde, float hasta)
        {
            float reloj = 0f;
            float duracion = Mathf.Max(0.05f, duracionFundido);
            while (reloj < duracion)
            {
                reloj += Time.unscaledDeltaTime;
                fundido.alpha = Mathf.Lerp(desde, hasta, reloj / duracion);
                yield return null;
            }
            fundido.alpha = hasta;
        }

        private void CrearFundido()
        {
            GameObject canvasObjeto = new GameObject("Transicion Fundido", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObjeto.transform.SetParent(transform, false);
            Canvas canvas = canvasObjeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            GameObject imagenObjeto = new GameObject("Negro", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            imagenObjeto.transform.SetParent(canvasObjeto.transform, false);
            RectTransform rect = imagenObjeto.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            imagenObjeto.GetComponent<Image>().color = Color.black;
            fundido = imagenObjeto.GetComponent<CanvasGroup>();
            fundido.alpha = 1f;
            fundido.blocksRaycasts = true;
        }
    }
}
