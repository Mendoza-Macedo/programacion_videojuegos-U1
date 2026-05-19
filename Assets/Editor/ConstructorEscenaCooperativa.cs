using System.IO;
using JuegoCooperativo.Camara;
using JuegoCooperativo.Cuerda;
using JuegoCooperativo.Juego;
using JuegoCooperativo.Mundo;
using JuegoCooperativo.Personajes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JuegoCooperativo.Editor
{
    public static class ConstructorEscenaCooperativa
    {
        private const string RutaEscena = "Assets/Escenas/EscenaCooperativa.unity";
        private const string RutaSprites = "Assets/Sprites";
        private const string RutaPrefabs = "Assets/Prefabs";
        private const string RutaMateriales = "Assets/Materiales";
        private const string NombreLayerSuelo = "Suelo";
        private const string NombreTagJugador = "Jugador";

        [InitializeOnLoadMethod]
        private static void ConstruirSiHaceFalta()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(RutaEscena))
                {
                    ConstruirEscenaCompleta();
                }
            };
        }

        [MenuItem("Herramientas/BreadFred/Re construir escena completa")]
        public static void ConstruirEscenaCompleta()
        {
            CrearCarpetas();
            CrearTag(NombreTagJugador);
            CrearLayer(NombreLayerSuelo, 8);
            CrearSpritesBasicos();
            CrearMateriales();

            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            escena.name = "EscenaCooperativa";

            int layerSuelo = LayerMask.NameToLayer(NombreLayerSuelo);
            Material materialCuerda = AssetDatabase.LoadAssetAtPath<Material>($"{RutaMateriales}/MaterialCuerda.mat");

            GameObject jugadorAzul = CrearJugador("Jugador Azul", new Vector2(-1.4f, 1.4f), new Color(0.2f, 0.62f, 1f), true, layerSuelo);
            GameObject jugadorRojo = CrearJugador("Jugador Rojo", new Vector2(1.4f, 1.4f), new Color(1f, 0.28f, 0.25f), false, layerSuelo);

            CrearNivel(layerSuelo);
            GameObject cuerda = CrearCuerda(jugadorAzul, jugadorRojo, materialCuerda);
            GameObject camara = CrearCamara(jugadorAzul.transform, jugadorRojo.transform);
            GameObject gestor = CrearGestor(jugadorAzul, jugadorRojo);
            CrearInterfaz(gestor.GetComponent<GestorJuego>());
            CrearPrefabs(jugadorAzul, jugadorRojo, cuerda);

            EditorSceneManager.SaveScene(escena, RutaEscena);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEscena);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Escena cooperativa creada en " + RutaEscena);
        }

        private static void CrearCarpetas()
        {
            string[] carpetas =
            {
                "Assets/Scripts", "Assets/Scripts/Personajes", "Assets/Scripts/Cuerda", "Assets/Scripts/Camara",
                "Assets/Scripts/Mundo", "Assets/Scripts/Juego", "Assets/Editor", "Assets/Escenas",
                "Assets/Prefabs", "Assets/Sprites", "Assets/Materiales"
            };

            foreach (string carpeta in carpetas)
            {
                if (!AssetDatabase.IsValidFolder(carpeta))
                {
                    string padre = Path.GetDirectoryName(carpeta).Replace('\\', '/');
                    string nombre = Path.GetFileName(carpeta);
                    AssetDatabase.CreateFolder(padre, nombre);
                }
            }
        }

        private static void CrearTag(string nombre)
        {
            SerializedObject etiquetas = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = etiquetas.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == nombre) return;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = nombre;
            etiquetas.ApplyModifiedProperties();
        }

        private static void CrearLayer(string nombre, int indice)
        {
            SerializedObject etiquetas = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = etiquetas.FindProperty("layers");
            SerializedProperty layer = layers.GetArrayElementAtIndex(indice);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = nombre;
                etiquetas.ApplyModifiedProperties();
            }
        }

        private static void CrearSpritesBasicos()
        {
            CrearSpriteColor("Jugador", new Color(1f, 1f, 1f, 1f), 64, 64);
            CrearSpriteColor("Bloque", new Color(1f, 1f, 1f, 1f), 64, 64);
            CrearSpriteColor("Bandera", new Color(1f, 1f, 1f, 1f), 48, 96);
        }

        private static void CrearSpriteColor(string nombre, Color color, int ancho, int alto)
        {
            string ruta = $"{RutaSprites}/{nombre}.png";
            if (File.Exists(ruta)) return;

            Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
            Color[] pixeles = new Color[ancho * alto];
            for (int i = 0; i < pixeles.Length; i++) pixeles[i] = color;
            textura.SetPixels(pixeles);
            textura.Apply();
            File.WriteAllBytes(ruta, textura.EncodeToPNG());
            Object.DestroyImmediate(textura);
            AssetDatabase.ImportAsset(ruta);

            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
            importador.textureType = TextureImporterType.Sprite;
            importador.spritePixelsPerUnit = 64f;
            importador.filterMode = FilterMode.Point;
            importador.SaveAndReimport();
        }

        private static void CrearMateriales()
        {
            string rutaMaterial = $"{RutaMateriales}/MaterialCuerda.mat";
            if (File.Exists(rutaMaterial)) return;

            Shader shader = Shader.Find("Sprites/Default");
            Material material = new Material(shader);
            material.color = new Color(0.95f, 0.76f, 0.35f);
            AssetDatabase.CreateAsset(material, rutaMaterial);
        }

        private static GameObject CrearJugador(string nombre, Vector2 posicion, Color color, bool esJugadorUno, int layerSuelo)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RutaSprites}/Jugador.png");
            GameObject jugador = new GameObject(nombre);
            jugador.tag = NombreTagJugador;
            jugador.transform.position = posicion;

            SpriteRenderer render = jugador.AddComponent<SpriteRenderer>();
            render.sprite = sprite;
            render.color = color;
            render.sortingOrder = 10;

            Rigidbody2D cuerpo = jugador.AddComponent<Rigidbody2D>();
            cuerpo.gravityScale = 3.2f;
            cuerpo.freezeRotation = true;
            cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;

            CapsuleCollider2D colision = jugador.AddComponent<CapsuleCollider2D>();
            colision.size = new Vector2(0.72f, 0.95f);

            JugadorCooperativo controlador = jugador.AddComponent<JugadorCooperativo>();
            SerializedObject datos = new SerializedObject(controlador);
            datos.FindProperty("nombreJugador").stringValue = nombre;
            datos.FindProperty("colorJugador").colorValue = color;
            datos.FindProperty("teclaIzquierda").intValue = esJugadorUno ? (int)UnityEngine.InputSystem.Key.A : (int)UnityEngine.InputSystem.Key.LeftArrow;
            datos.FindProperty("teclaDerecha").intValue = esJugadorUno ? (int)UnityEngine.InputSystem.Key.D : (int)UnityEngine.InputSystem.Key.RightArrow;
            datos.FindProperty("teclaSalto").intValue = esJugadorUno ? (int)UnityEngine.InputSystem.Key.W : (int)UnityEngine.InputSystem.Key.UpArrow;
            datos.FindProperty("teclaAgarrar").intValue = esJugadorUno ? (int)UnityEngine.InputSystem.Key.LeftShift : (int)UnityEngine.InputSystem.Key.RightShift;
            datos.FindProperty("capaSuelo").intValue = 1 << layerSuelo;
            datos.ApplyModifiedProperties();

            return jugador;
        }

        private static void CrearNivel(int layerSuelo)
        {
            CrearBloque("Suelo inicial", new Vector2(0f, -0.6f), new Vector2(8f, 0.7f), new Color(0.18f, 0.22f, 0.28f), layerSuelo);
            CrearBloque("Escalon 1", new Vector2(4.5f, 1.1f), new Vector2(2.4f, 0.45f), new Color(0.26f, 0.32f, 0.36f), layerSuelo);
            CrearBloque("Escalon 2", new Vector2(7.4f, 2.8f), new Vector2(2.2f, 0.45f), new Color(0.26f, 0.32f, 0.36f), layerSuelo);
            CrearBloque("Puente angosto", new Vector2(10.5f, 4.2f), new Vector2(1.2f, 0.35f), new Color(0.22f, 0.36f, 0.32f), layerSuelo);
            CrearBloque("Repisa alta", new Vector2(14.2f, 6.2f), new Vector2(3f, 0.45f), new Color(0.26f, 0.32f, 0.36f), layerSuelo);
            CrearBloque("Pared agarre izquierda", new Vector2(12.3f, 5.1f), new Vector2(0.35f, 2.6f), new Color(0.18f, 0.27f, 0.31f), layerSuelo);
            CrearBloque("Pared agarre derecha", new Vector2(16.1f, 7.4f), new Vector2(0.35f, 2.8f), new Color(0.18f, 0.27f, 0.31f), layerSuelo);
            CrearBloque("Cima", new Vector2(20f, 9.1f), new Vector2(5f, 0.55f), new Color(0.23f, 0.38f, 0.32f), layerSuelo);

            GameObject movil = CrearBloque("Plataforma movil", new Vector2(17.5f, 8f), new Vector2(2f, 0.35f), new Color(0.42f, 0.42f, 0.18f), layerSuelo);
            movil.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            PlataformaMovil plataforma = movil.AddComponent<PlataformaMovil>();
            SerializedObject datosMovil = new SerializedObject(plataforma);
            datosMovil.FindProperty("desplazamiento").vector2Value = new Vector2(2.5f, 0.8f);
            datosMovil.ApplyModifiedProperties();

            CrearPuntoControl("Punto Control 1", new Vector2(7.4f, 3.45f));
            CrearPuntoControl("Punto Control 2", new Vector2(14.2f, 6.85f));
            CrearZonaMuerte();
            CrearObstaculoGiratorio(new Vector2(9.2f, 3.4f));
            CrearMeta(new Vector2(21.2f, 10.05f));
        }

        private static GameObject CrearBloque(string nombre, Vector2 posicion, Vector2 escala, Color color, int layer)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RutaSprites}/Bloque.png");
            GameObject bloque = new GameObject(nombre);
            bloque.layer = layer;
            bloque.transform.position = posicion;
            bloque.transform.localScale = escala;

            SpriteRenderer render = bloque.AddComponent<SpriteRenderer>();
            render.sprite = sprite;
            render.color = color;
            render.sortingOrder = 0;

            BoxCollider2D colision = bloque.AddComponent<BoxCollider2D>();
            colision.size = Vector2.one;
            return bloque;
        }

        private static void CrearPuntoControl(string nombre, Vector2 posicion)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RutaSprites}/Bandera.png");
            GameObject punto = new GameObject(nombre);
            punto.transform.position = posicion;

            SpriteRenderer render = punto.AddComponent<SpriteRenderer>();
            render.sprite = sprite;
            render.color = new Color(0.9f, 0.7f, 0.25f);
            render.sortingOrder = 3;

            BoxCollider2D trigger = punto.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.8f, 1.4f);
            punto.AddComponent<PuntoControl>();
        }

        private static void CrearZonaMuerte()
        {
            GameObject zona = new GameObject("Zona de muerte");
            zona.transform.position = new Vector2(10f, -6f);
            BoxCollider2D trigger = zona.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(40f, 1f);
            zona.AddComponent<ZonaMuerte>();
        }

        private static void CrearObstaculoGiratorio(Vector2 posicion)
        {
            GameObject obstaculo = CrearBloque("Aspa peligrosa", posicion, new Vector2(2.2f, 0.22f), new Color(0.9f, 0.16f, 0.12f), LayerMask.NameToLayer(NombreLayerSuelo));
            Rigidbody2D cuerpo = obstaculo.AddComponent<Rigidbody2D>();
            cuerpo.bodyType = RigidbodyType2D.Kinematic;
            obstaculo.AddComponent<ObstaculoGiratorio>();
        }

        private static void CrearMeta(Vector2 posicion)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RutaSprites}/Bandera.png");
            GameObject meta = new GameObject("Meta");
            meta.transform.position = posicion;

            SpriteRenderer render = meta.AddComponent<SpriteRenderer>();
            render.sprite = sprite;
            render.color = new Color(0.4f, 1f, 0.65f);
            render.sortingOrder = 4;

            BoxCollider2D trigger = meta.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.3f, 1.6f);
            meta.AddComponent<MetaNivel>();
        }

        private static GameObject CrearCuerda(GameObject jugadorUno, GameObject jugadorDos, Material material)
        {
            GameObject cuerda = new GameObject("Cuerda Cooperativa");
            LineRenderer linea = cuerda.AddComponent<LineRenderer>();
            linea.material = material;
            CuerdaCooperativa logica = cuerda.AddComponent<CuerdaCooperativa>();

            SerializedObject datos = new SerializedObject(logica);
            datos.FindProperty("jugadorUno").objectReferenceValue = jugadorUno.GetComponent<JugadorCooperativo>();
            datos.FindProperty("jugadorDos").objectReferenceValue = jugadorDos.GetComponent<JugadorCooperativo>();
            datos.ApplyModifiedProperties();
            return cuerda;
        }

        private static GameObject CrearCamara(Transform jugadorUno, Transform jugadorDos)
        {
            GameObject camara = new GameObject("Camara Cooperativa");
            Camera componente = camara.AddComponent<Camera>();
            componente.orthographic = true;
            componente.orthographicSize = 6f;
            componente.backgroundColor = new Color(0.5f, 0.78f, 0.95f);
            camara.transform.position = new Vector3(0f, 2f, -10f);
            camara.AddComponent<AudioListener>();

            CamaraCooperativa seguimiento = camara.AddComponent<CamaraCooperativa>();
            SerializedObject datos = new SerializedObject(seguimiento);
            datos.FindProperty("jugadorUno").objectReferenceValue = jugadorUno;
            datos.FindProperty("jugadorDos").objectReferenceValue = jugadorDos;
            datos.ApplyModifiedProperties();
            return camara;
        }

        private static GameObject CrearGestor(GameObject jugadorUno, GameObject jugadorDos)
        {
            GameObject gestor = new GameObject("Gestor del Juego");
            GestorJuego logica = gestor.AddComponent<GestorJuego>();
            SerializedObject datos = new SerializedObject(logica);
            datos.FindProperty("jugadorUno").objectReferenceValue = jugadorUno.GetComponent<JugadorCooperativo>();
            datos.FindProperty("jugadorDos").objectReferenceValue = jugadorDos.GetComponent<JugadorCooperativo>();
            datos.ApplyModifiedProperties();
            return gestor;
        }

        private static void CrearInterfaz(GestorJuego gestor)
        {
            GameObject canvasObjeto = new GameObject("Interfaz");
            Canvas canvas = canvasObjeto.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObjeto.AddComponent<CanvasScaler>();
            canvasObjeto.AddComponent<GraphicRaycaster>();

            GameObject textoObjeto = new GameObject("Texto Estado");
            textoObjeto.transform.SetParent(canvasObjeto.transform, false);
            Text texto = textoObjeto.AddComponent<Text>();
            texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            texto.fontSize = 22;
            texto.color = Color.white;
            texto.alignment = TextAnchor.UpperLeft;
            texto.raycastTarget = false;
            texto.text = "Listo";

            RectTransform rect = texto.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(18f, -18f);
            rect.sizeDelta = new Vector2(720f, 120f);

            SerializedObject datos = new SerializedObject(gestor);
            datos.FindProperty("textoEstado").objectReferenceValue = texto;
            datos.ApplyModifiedProperties();
        }

        private static void CrearPrefabs(GameObject jugadorAzul, GameObject jugadorRojo, GameObject cuerda)
        {
            PrefabUtility.SaveAsPrefabAsset(jugadorAzul, $"{RutaPrefabs}/JugadorAzul.prefab");
            PrefabUtility.SaveAsPrefabAsset(jugadorRojo, $"{RutaPrefabs}/JugadorRojo.prefab");
            PrefabUtility.SaveAsPrefabAsset(cuerda, $"{RutaPrefabs}/CuerdaCooperativa.prefab");
        }
    }
}

