using JuegoCooperativo.Audio;
using JuegoCooperativo.Cuerda;
using JuegoCooperativo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace JuegoCooperativo.Editor
{
    public static class ConstructorJuegoCompleto
    {
        private const string RutaEscenaPrincipal = "Assets/Escenas/EscenaCooperativa.unity";
        private const string RutaEscenaSample = "Assets/Scenes/SampleScene.unity";
        private const string NombreLayerSuelo = "Suelo";

        [MenuItem("Herramientas/BreadFred/Construir juego completo v1")]
        public static void Construir()
        {
            ConstructorAnimalesSierra.AgregarAnimalesALaEscena();
            ActualizarEscena(RutaEscenaPrincipal);
            ActualizarEscena(RutaEscenaSample);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Juego completo v1 construido.");
        }

        private static void ActualizarEscena(string ruta)
        {
            var escena = EditorSceneManager.OpenScene(ruta);
            int layerSuelo = LayerMask.NameToLayer(NombreLayerSuelo);

            CrearAudio();
            CrearDecoracionAndina(layerSuelo);
            CrearNivelExtendido(layerSuelo);
            CrearUiJuego();
            ConectarIndicadorCuerda();
            ConectarFlujoUi();
            AjustarCamara();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, ruta);
        }

        private static void CrearAudio()
        {
            GameObject audio = Buscar("Sonidos del Juego") ?? new GameObject("Sonidos del Juego");
            if (audio.GetComponent<AudioSource>() == null) audio.AddComponent<AudioSource>();
            if (audio.GetComponent<SonidosJuego>() == null) audio.AddComponent<SonidosJuego>();
        }

        private static void CrearDecoracionAndina(int layerSuelo)
        {
            CrearFondo("Montana Fondo 1", new Vector2(4f, 6f), new Vector2(18f, 7f), new Color(0.32f, 0.45f, 0.46f), -20);
            CrearFondo("Montana Fondo 2", new Vector2(20f, 8f), new Vector2(22f, 9f), new Color(0.24f, 0.36f, 0.40f), -21);
            CrearFondo("Montana Nevada", new Vector2(34f, 13f), new Vector2(24f, 12f), new Color(0.38f, 0.50f, 0.52f), -22);
            CrearFondo("Cielo Claro", new Vector2(18f, 12f), new Vector2(70f, 36f), new Color(0.45f, 0.75f, 0.95f), -30);

            for (int i = 0; i < 14; i++)
            {
                CrearBloque($"Ichu decorativo {i}", new Vector2(-2f + i * 3.2f, 0.1f + (i % 4) * 1.7f), new Vector2(0.15f, 0.85f), new Color(0.72f, 0.64f, 0.30f), layerSuelo, false, 2);
            }
        }

        private static void CrearNivelExtendido(int layerSuelo)
        {
            CrearBloque("Tutorial Movimiento", new Vector2(-4.5f, -0.6f), new Vector2(4f, 0.7f), new Color(0.20f, 0.28f, 0.30f), layerSuelo, true, 0);
            CrearBloque("Tutorial Salto", new Vector2(-7.5f, 1.0f), new Vector2(1.6f, 0.45f), new Color(0.25f, 0.35f, 0.33f), layerSuelo, true, 0);
            CrearBloque("Tutorial Cuerda", new Vector2(-10.5f, 2.6f), new Vector2(1.4f, 0.4f), new Color(0.25f, 0.35f, 0.33f), layerSuelo, true, 0);

            CrearBloque("Repisa Sierra 1", new Vector2(23.5f, 11.2f), new Vector2(2.8f, 0.45f), new Color(0.22f, 0.36f, 0.32f), layerSuelo, true, 0);
            CrearBloque("Repisa Sierra 2", new Vector2(27.2f, 13.4f), new Vector2(2.4f, 0.45f), new Color(0.22f, 0.36f, 0.32f), layerSuelo, true, 0);
            CrearBloque("Pared Cuerda Final", new Vector2(30.2f, 15.2f), new Vector2(0.35f, 4.3f), new Color(0.18f, 0.27f, 0.31f), layerSuelo, true, 0);
            CrearBloque("Cima Final", new Vector2(36f, 18f), new Vector2(8f, 0.6f), new Color(0.26f, 0.42f, 0.34f), layerSuelo, true, 0);

            GameObject meta = Buscar("Meta");
            if (meta != null) meta.transform.position = new Vector2(38.5f, 18.95f);
        }

        private static void CrearUiJuego()
        {
            GameObject canvas = Buscar("Interfaz Juego Completo") ?? new GameObject("Interfaz Juego Completo");
            Canvas c = canvas.GetComponent<Canvas>(); if (c == null) c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>(); if (scaler == null) scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.AddComponent<GraphicRaycaster>();

            CrearPanelTexto(canvas.transform, "Panel Menu Principal", "ANIMALES DE LA CUMBRE\nCooperativo Andino\n\nEnter/Espacio: empezar\nEsc: pausa durante el juego", new Vector2(0f, 0f), new Vector2(760f, 390f), 34);
            CrearPanelTexto(canvas.transform, "Panel Pausa", "PAUSA\n\nEsc: continuar\nR: reiniciar", new Vector2(0f, 0f), new Vector2(520f, 260f), 32).SetActive(false);
            CrearPanelTexto(canvas.transform, "Panel Victoria", "META ALCANZADA", new Vector2(0f, 0f), new Vector2(600f, 300f), 32).SetActive(false);

            CrearBarraTension(canvas.transform);
        }

        private static void CrearBarraTension(Transform canvas)
        {
            GameObject raiz = CrearUi(canvas, "Indicador Tension");
            ConfigRect(raiz.GetComponent<RectTransform>(), new Vector2(0f, -316f), new Vector2(430f, 44f));
            Image fondo = raiz.GetComponent<Image>(); if (fondo == null) fondo = raiz.AddComponent<Image>();
            fondo.color = new Color(0.04f, 0.06f, 0.08f, 0.82f);

            GameObject fill = CrearUi(raiz.transform, "Relleno Tension");
            Image img = fill.GetComponent<Image>(); if (img == null) img = fill.AddComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 0f;
            img.color = new Color(0.3f, 0.9f, 0.55f);
            ConfigRect(fill.GetComponent<RectTransform>(), Vector2.zero, new Vector2(400f, 18f));

            Text texto = CrearTexto(raiz.transform, "Texto Tension", "Tension de cuerda", 18, TextAnchor.UpperCenter);
            ConfigRect(texto.GetComponent<RectTransform>(), new Vector2(0f, 14f), new Vector2(400f, 24f));

            IndicadorTensionCuerda indicador = raiz.GetComponent<IndicadorTensionCuerda>(); if (indicador == null) indicador = raiz.AddComponent<IndicadorTensionCuerda>();
            SerializedObject so = new SerializedObject(indicador);
            so.FindProperty("relleno").objectReferenceValue = img;
            so.FindProperty("texto").objectReferenceValue = texto;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConectarIndicadorCuerda()
        {
            IndicadorTensionCuerda indicador = Buscar("Indicador Tension")?.GetComponent<IndicadorTensionCuerda>();
            CuerdaCooperativa cuerda = Buscar("Cuerda Cooperativa")?.GetComponent<CuerdaCooperativa>();
            if (indicador == null || cuerda == null) return;
            SerializedObject so = new SerializedObject(indicador);
            so.FindProperty("cuerda").objectReferenceValue = cuerda;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConectarFlujoUi()
        {
            GameObject flujoObj = Buscar("Flujo Juego UI") ?? new GameObject("Flujo Juego UI");
            FlujoJuegoUI flujo = flujoObj.GetComponent<FlujoJuegoUI>(); if (flujo == null) flujo = flujoObj.AddComponent<FlujoJuegoUI>();
            SerializedObject so = new SerializedObject(flujo);
            so.FindProperty("panelMenu").objectReferenceValue = Buscar("Panel Menu Principal");
            so.FindProperty("panelPausa").objectReferenceValue = Buscar("Panel Pausa");
            so.FindProperty("panelVictoria").objectReferenceValue = Buscar("Panel Victoria");
            so.FindProperty("textoVictoria").objectReferenceValue = Buscar("Texto Panel Victoria")?.GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AjustarCamara()
        {
            var camara = Buscar("Camara Cooperativa")?.GetComponent<JuegoCooperativo.Camara.CamaraCooperativa>();
            if (camara == null) return;
            SerializedObject so = new SerializedObject(camara);
            so.FindProperty("limiteMinimo").vector2Value = new Vector2(-12f, -4f);
            so.FindProperty("limiteMaximo").vector2Value = new Vector2(40f, 22f);
            so.FindProperty("tamanoMaximo").floatValue = 8.2f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CrearPanelTexto(Transform canvas, string nombre, string texto, Vector2 pos, Vector2 size, int fontSize)
        {
            GameObject panel = CrearUi(canvas, nombre);
            Image img = panel.GetComponent<Image>(); if (img == null) img = panel.AddComponent<Image>();
            img.color = new Color(0.04f, 0.07f, 0.10f, 0.90f);
            ConfigRect(panel.GetComponent<RectTransform>(), pos, size);

            Text t = CrearTexto(panel.transform, "Texto " + nombre.Replace("Panel ", "Panel "), texto, fontSize, TextAnchor.MiddleCenter);
            ConfigRect(t.GetComponent<RectTransform>(), Vector2.zero, size - new Vector2(50f, 40f));
            return panel;
        }

        private static Text CrearTexto(Transform padre, string nombre, string contenido, int fontSize, TextAnchor anchor)
        {
            GameObject obj = CrearUi(padre, nombre);
            Text t = obj.GetComponent<Text>(); if (t == null) t = obj.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = Color.white;
            t.text = contenido;
            t.raycastTarget = false;
            return t;
        }

        private static GameObject CrearUi(Transform padre, string nombre)
        {
            GameObject obj = Buscar(nombre) ?? new GameObject(nombre);
            obj.transform.SetParent(padre, false);
            if (obj.GetComponent<RectTransform>() == null) obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void CrearFondo(string nombre, Vector2 pos, Vector2 escala, Color color, int order)
        {
            GameObject obj = Buscar(nombre) ?? new GameObject(nombre);
            obj.transform.position = pos;
            obj.transform.localScale = escala;
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
            sr.color = color;
            sr.sortingOrder = order;
        }

        private static GameObject CrearBloque(string nombre, Vector2 pos, Vector2 escala, Color color, int layer, bool colision, int order)
        {
            GameObject obj = Buscar(nombre) ?? new GameObject(nombre);
            obj.layer = layer;
            obj.transform.position = pos;
            obj.transform.localScale = escala;
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
            sr.color = color;
            sr.sortingOrder = order;
            if (colision && obj.GetComponent<BoxCollider2D>() == null) obj.AddComponent<BoxCollider2D>();
            return obj;
        }

        private static void ConfigRect(RectTransform rect, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static GameObject Buscar(string nombre)
        {
            foreach (GameObject raiz in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                GameObject encontrado = BuscarHijo(raiz.transform, nombre);
                if (encontrado != null) return encontrado;
            }
            return null;
        }

        private static GameObject BuscarHijo(Transform actual, string nombre)
        {
            if (actual.name == nombre) return actual.gameObject;
            for (int i = 0; i < actual.childCount; i++)
            {
                GameObject r = BuscarHijo(actual.GetChild(i), nombre);
                if (r != null) return r;
            }
            return null;
        }
    }
}


