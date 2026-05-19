using System.Collections.Generic;
using System.IO;
using JuegoCooperativo.Animales;
using JuegoCooperativo.Personajes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace JuegoCooperativo.Editor
{
    public static class ConstructorAnimalesSierra
    {
        private const string RutaAnimales = "Assets/Sprites/AnimalesSierra";
        private const string RutaEscenaPrincipal = "Assets/Escenas/EscenaCooperativa.unity";
        private const string RutaEscenaSample = "Assets/Scenes/SampleScene.unity";

        private static readonly string[] Estados = { "Quieto", "CaminarUno", "CaminarDos", "Saltar", "Caer", "Agarrarse" };

        private struct DatosDibujoAnimal
        {
            public string archivo;
            public string nombre;
            public Color cuerpo;
            public Color pecho;
            public Color detalle;
            public int orejas;
            public bool colaLarga;
            public bool alas;

            public DatosDibujoAnimal(string archivo, string nombre, Color cuerpo, Color pecho, Color detalle, int orejas, bool colaLarga, bool alas)
            {
                this.archivo = archivo;
                this.nombre = nombre;
                this.cuerpo = cuerpo;
                this.pecho = pecho;
                this.detalle = detalle;
                this.orejas = orejas;
                this.colaLarga = colaLarga;
                this.alas = alas;
            }
        }

        private static readonly DatosDibujoAnimal[] Animales =
        {
            new DatosDibujoAnimal("Vicuna", "Vicuna", new Color(0.83f, 0.54f, 0.24f), new Color(0.98f, 0.86f, 0.56f), new Color(0.45f, 0.25f, 0.12f), 2, false, false),
            new DatosDibujoAnimal("Alpaca", "Alpaca", new Color(0.92f, 0.85f, 0.72f), new Color(1f, 0.96f, 0.84f), new Color(0.55f, 0.44f, 0.33f), 2, false, false),
            new DatosDibujoAnimal("Llama", "Llama", new Color(0.74f, 0.44f, 0.27f), new Color(0.94f, 0.79f, 0.56f), new Color(0.35f, 0.19f, 0.12f), 2, true, false),
            new DatosDibujoAnimal("Vizcacha", "Vizcacha", new Color(0.55f, 0.54f, 0.50f), new Color(0.78f, 0.76f, 0.68f), new Color(0.28f, 0.28f, 0.27f), 2, true, false),
            new DatosDibujoAnimal("Condor", "Condor Andino", new Color(0.10f, 0.11f, 0.12f), new Color(0.95f, 0.95f, 0.90f), new Color(0.67f, 0.08f, 0.08f), 0, false, true)
        };

        [MenuItem("Herramientas/BreadFred/Agregar animales de la sierra")]
        public static void AgregarAnimalesALaEscena()
        {
            CrearCarpetaAnimales();
            CrearSpritesAnimales();
            ActualizarEscena(RutaEscenaPrincipal);
            ActualizarEscena(RutaEscenaSample);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Animales de la sierra agregados a las escenas.");
        }

        private static void CrearCarpetaAnimales()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Sprites")) AssetDatabase.CreateFolder("Assets", "Sprites");
            if (!AssetDatabase.IsValidFolder(RutaAnimales)) AssetDatabase.CreateFolder("Assets/Sprites", "AnimalesSierra");
        }

        private static void CrearSpritesAnimales()
        {
            if (File.Exists("Assets/Sprites/AnimalesSierraIA/HojaAnimalesSierraIA.png") && File.Exists(ObtenerRutaSprite("Vicuna", "Quieto")))
            {
                foreach (DatosDibujoAnimal animal in Animales)
                {
                    foreach (string estado in Estados)
                    {
                        string ruta = ObtenerRutaSprite(animal.archivo, estado);
                        AssetDatabase.ImportAsset(ruta);
                        ConfigurarImportadorSprite(ruta);
                    }
                }

                return;
            }

            foreach (DatosDibujoAnimal animal in Animales)
            {
                foreach (string estado in Estados)
                {
                    string ruta = ObtenerRutaSprite(animal.archivo, estado);
                    Texture2D textura = DibujarAnimal(animal, estado);
                    File.WriteAllBytes(ruta, textura.EncodeToPNG());
                    Object.DestroyImmediate(textura);
                    AssetDatabase.ImportAsset(ruta);

                    TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
                    importador.textureType = TextureImporterType.Sprite;
                    importador.spritePixelsPerUnit = 64f;
                    importador.filterMode = FilterMode.Point;
                    importador.textureCompression = TextureImporterCompression.Uncompressed;
                    importador.alphaIsTransparency = true;
                    importador.SaveAndReimport();
                }
            }
        }

        private static void ConfigurarImportadorSprite(string ruta)
        {
            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
            if (importador == null) return;
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.spritePixelsPerUnit = 96f;
            importador.filterMode = FilterMode.Bilinear;
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.alphaIsTransparency = true;
            importador.SaveAndReimport();
        }

        private static Texture2D DibujarAnimal(DatosDibujoAnimal animal, string estado)
        {
            Texture2D textura = new Texture2D(96, 96, TextureFormat.RGBA32, false);
            Limpiar(textura);

            int subida = estado == "Saltar" ? 7 : 0;
            int caida = estado == "Caer" ? -5 : 0;
            int abrazo = estado == "Agarrarse" ? 3 : 0;
            int pasoA = estado == "CaminarUno" ? 4 : 0;
            int pasoB = estado == "CaminarDos" ? -4 : 0;
            int baseY = 30 + subida + caida;
            Color borde = Oscurecer(animal.detalle, 0.45f);
            Color luz = Aclarar(animal.cuerpo, 1.18f);

            if (animal.alas)
            {
                DibujarElipseConBorde(textura, 20, baseY + 10 + abrazo, 25, 19, borde, animal.cuerpo);
                DibujarElipseConBorde(textura, 76, baseY + 10 - abrazo, 25, 19, borde, animal.cuerpo);
                DibujarRect(textura, 10, baseY + 2 + abrazo, 22, 4, animal.pecho);
                DibujarRect(textura, 64, baseY + 2 - abrazo, 22, 4, animal.pecho);
                DibujarElipseConBorde(textura, 48, baseY + 14, 19, 25, borde, animal.cuerpo);
                DibujarElipse(textura, 48, baseY + 15, 10, 19, animal.pecho);
                DibujarElipseConBorde(textura, 48, baseY + 38, 16, 14, borde, animal.cuerpo);
                DibujarRect(textura, 39, baseY + 47, 18, 7, animal.pecho);
                DibujarRect(textura, 43, baseY + 51, 10, 4, animal.detalle);
            }
            else
            {
                int cuelloAlto = animal.archivo == "Vizcacha" ? 10 : 21;
                DibujarElipseConBorde(textura, 48, baseY + 16, 25, 20, borde, animal.cuerpo);
                DibujarElipse(textura, 39, baseY + 25, 9, 5, luz);
                DibujarElipse(textura, 48, baseY + 14, 12, 13, animal.pecho);
                DibujarRect(textura, 43, baseY + 25, 10, cuelloAlto, animal.cuerpo);
                DibujarRect(textura, 41, baseY + 25, 14, 3, borde);
                DibujarElipseConBorde(textura, 48, baseY + 38, 17, 18, borde, animal.cuerpo);
            }

            if (!animal.alas)
            {
                DibujarPierna(textura, 36 + pasoA, baseY - 2, borde, animal.detalle);
                DibujarPierna(textura, 60 + pasoB, baseY - 2, borde, animal.detalle);
                DibujarPierna(textura, 43 + pasoB, baseY - 5, borde, animal.detalle);
                DibujarPierna(textura, 55 + pasoA, baseY - 5, borde, animal.detalle);
            }
            else
            {
                DibujarPierna(textura, 43 + pasoA / 2, baseY - 5, borde, animal.detalle);
                DibujarPierna(textura, 53 + pasoB / 2, baseY - 5, borde, animal.detalle);
            }

            if (animal.orejas > 0)
            {
                int orejaExtra = animal.archivo == "Vizcacha" ? 8 : 0;
                DibujarTriangulo(textura, new Vector2Int(36, baseY + 52), new Vector2Int(42, baseY + 75 + orejaExtra), new Vector2Int(48, baseY + 52), borde);
                DibujarTriangulo(textura, new Vector2Int(48, baseY + 52), new Vector2Int(54, baseY + 75 + orejaExtra), new Vector2Int(60, baseY + 52), borde);
                DibujarTriangulo(textura, new Vector2Int(39, baseY + 53), new Vector2Int(42, baseY + 68 + orejaExtra), new Vector2Int(45, baseY + 53), animal.detalle);
                DibujarTriangulo(textura, new Vector2Int(51, baseY + 53), new Vector2Int(54, baseY + 68 + orejaExtra), new Vector2Int(57, baseY + 53), animal.detalle);
            }

            if (animal.colaLarga)
            {
                int colaX = animal.archivo == "Vizcacha" ? 24 : 25;
                DibujarElipseConBorde(textura, colaX, baseY + 18, 8, animal.archivo == "Vizcacha" ? 29 : 21, borde, animal.detalle);
                if (animal.archivo == "Vizcacha")
                {
                    DibujarElipse(textura, colaX, baseY + 39, 6, 6, Aclarar(animal.detalle, 1.35f));
                }
            }

            DibujarDetallesAnimal(textura, animal, baseY, estado);
            DibujarCara(textura, baseY, animal.alas, estado == "Agarrarse", borde);
            textura.Apply();
            return textura;
        }

        private static void DibujarDetallesAnimal(Texture2D textura, DatosDibujoAnimal animal, int baseY, string estado)
        {
            if (animal.archivo == "Alpaca")
            {
                Color lana = Aclarar(animal.pecho, 1.08f);
                for (int i = 0; i < 7; i++)
                {
                    DibujarElipse(textura, 31 + i * 5, baseY + 53 + (i % 2), 4, 4, lana);
                }
            }
            else if (animal.archivo == "Vicuna")
            {
                DibujarRect(textura, 45, baseY + 20, 6, 22, animal.pecho);
                DibujarRect(textura, 52, baseY + 20, 3, 18, Aclarar(animal.pecho, 1.05f));
            }
            else if (animal.archivo == "Llama")
            {
                DibujarRect(textura, 33, baseY + 18, 30, 8, new Color(0.72f, 0.10f, 0.18f));
                DibujarRect(textura, 38, baseY + 20, 6, 4, new Color(0.95f, 0.75f, 0.20f));
                DibujarRect(textura, 51, baseY + 20, 6, 4, new Color(0.18f, 0.55f, 0.80f));
            }
            else if (animal.archivo == "Vizcacha")
            {
                DibujarRect(textura, 30, baseY + 34, 6, 2, new Color(0.92f, 0.92f, 0.86f));
                DibujarRect(textura, 59, baseY + 34, 6, 2, new Color(0.92f, 0.92f, 0.86f));
            }
            else if (animal.archivo == "Condor")
            {
                DibujarElipse(textura, 48, baseY + 52, 13, 5, animal.pecho);
                DibujarRect(textura, 38, baseY + 5, 20, 4, animal.pecho);
            }
        }

        private static void DibujarCara(Texture2D textura, int baseY, bool pico, bool emocion, Color borde)
        {
            Color ojo = Color.black;
            Color brillo = Color.white;
            DibujarElipse(textura, 42, baseY + 42, 4, emocion ? 5 : 4, ojo);
            DibujarElipse(textura, 55, baseY + 42, 4, emocion ? 5 : 4, ojo);
            DibujarPixel(textura, 43, baseY + 44, brillo);
            DibujarPixel(textura, 56, baseY + 44, brillo);

            if (pico)
            {
                DibujarTriangulo(textura, new Vector2Int(46, baseY + 37), new Vector2Int(57, baseY + 37), new Vector2Int(51, baseY + 30), borde);
                DibujarTriangulo(textura, new Vector2Int(47, baseY + 37), new Vector2Int(56, baseY + 37), new Vector2Int(51, baseY + 31), new Color(0.9f, 0.62f, 0.12f));
            }
            else
            {
                DibujarRect(textura, 44, baseY + 33, 9, 3, borde);
                DibujarRect(textura, 52, baseY + 33, 3, 2, borde);
                DibujarPixel(textura, 43, baseY + 36, borde);
                DibujarPixel(textura, 56, baseY + 36, borde);
            }
        }

        private static void ActualizarEscena(string rutaEscena)
        {
            if (!File.Exists(rutaEscena)) return;

            UnityEngine.SceneManagement.Scene escena = EditorSceneManager.OpenScene(rutaEscena);
            GameObject jugadorAzul = BuscarObjetoEnEscena("Jugador Azul");
            GameObject jugadorRojo = BuscarObjetoEnEscena("Jugador Rojo");
            if (jugadorAzul == null || jugadorRojo == null)
            {
                Debug.LogWarning($"No se encontraron jugadores en {rutaEscena}");
                return;
            }

            AnimadorAnimalSierra animadorAzul = ConfigurarAnimador(jugadorAzul, 0);
            AnimadorAnimalSierra animadorRojo = ConfigurarAnimador(jugadorRojo, 1);

            GameObject selector = BuscarObjetoEnEscena("Selector de Animales");
            if (selector == null) selector = new GameObject("Selector de Animales");
            SelectorAnimalesSierra selectorAnimales = selector.GetComponent<SelectorAnimalesSierra>();
            if (selectorAnimales == null) selectorAnimales = selector.AddComponent<SelectorAnimalesSierra>();

            Text textoSeleccion = CrearTextoSeleccion();
            GameObject panelSeleccion = BuscarObjetoEnEscena("Panel Seleccion Animales");
            Image vistaPreviaUno = BuscarObjetoEnEscena("Vista Previa Azul")?.GetComponent<Image>();
            Image vistaPreviaDos = BuscarObjetoEnEscena("Vista Previa Rojo")?.GetComponent<Image>();
            Text nombreUno = BuscarObjetoEnEscena("Nombre Animal Azul")?.GetComponent<Text>();
            Text nombreDos = BuscarObjetoEnEscena("Nombre Animal Rojo")?.GetComponent<Text>();
            Text estadoUno = BuscarObjetoEnEscena("Estado Animal Azul")?.GetComponent<Text>();
            Text estadoDos = BuscarObjetoEnEscena("Estado Animal Rojo")?.GetComponent<Text>();
            Image fondoUno = BuscarObjetoEnEscena("Tarjeta Azul")?.GetComponent<Image>();
            Image fondoDos = BuscarObjetoEnEscena("Tarjeta Rojo")?.GetComponent<Image>();

            SerializedObject datosSelector = new SerializedObject(selectorAnimales);
            datosSelector.FindProperty("jugadorUno").objectReferenceValue = jugadorAzul.GetComponent<JugadorCooperativo>();
            datosSelector.FindProperty("jugadorDos").objectReferenceValue = jugadorRojo.GetComponent<JugadorCooperativo>();
            datosSelector.FindProperty("animadorUno").objectReferenceValue = animadorAzul;
            datosSelector.FindProperty("animadorDos").objectReferenceValue = animadorRojo;
            datosSelector.FindProperty("textoSeleccion").objectReferenceValue = textoSeleccion;
            datosSelector.FindProperty("panelSeleccion").objectReferenceValue = panelSeleccion;
            datosSelector.FindProperty("vistaPreviaUno").objectReferenceValue = vistaPreviaUno;
            datosSelector.FindProperty("vistaPreviaDos").objectReferenceValue = vistaPreviaDos;
            datosSelector.FindProperty("nombreUno").objectReferenceValue = nombreUno;
            datosSelector.FindProperty("nombreDos").objectReferenceValue = nombreDos;
            datosSelector.FindProperty("estadoUno").objectReferenceValue = estadoUno;
            datosSelector.FindProperty("estadoDos").objectReferenceValue = estadoDos;
            datosSelector.FindProperty("fondoUno").objectReferenceValue = fondoUno;
            datosSelector.FindProperty("fondoDos").objectReferenceValue = fondoDos;
            datosSelector.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            bool guardado = EditorSceneManager.SaveScene(escena, rutaEscena);
            Debug.Log($"Escena actualizada con animales: {rutaEscena} guardado={guardado}");
        }

        private static GameObject BuscarObjetoEnEscena(string nombre)
        {
            foreach (GameObject raiz in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                GameObject encontrado = BuscarEnHijos(raiz.transform, nombre);
                if (encontrado != null) return encontrado;
            }

            return null;
        }

        private static GameObject BuscarEnHijos(Transform actual, string nombre)
        {
            if (actual.name == nombre) return actual.gameObject;

            for (int i = 0; i < actual.childCount; i++)
            {
                GameObject encontrado = BuscarEnHijos(actual.GetChild(i), nombre);
                if (encontrado != null) return encontrado;
            }

            return null;
        }

        private static AnimadorAnimalSierra ConfigurarAnimador(GameObject jugador, int indiceInicial)
        {
            SpriteRenderer render = jugador.GetComponent<SpriteRenderer>();
            if (render != null) render.color = Color.white;

            AnimadorAnimalSierra animador = jugador.GetComponent<AnimadorAnimalSierra>();
            if (animador == null) animador = jugador.AddComponent<AnimadorAnimalSierra>();

            SerializedObject datos = new SerializedObject(animador);
            datos.FindProperty("jugador").objectReferenceValue = jugador.GetComponent<JugadorCooperativo>();
            datos.FindProperty("indiceAnimalActual").intValue = indiceInicial;

            SerializedProperty lista = datos.FindProperty("animalesDisponibles");
            lista.arraySize = Animales.Length;
            for (int i = 0; i < Animales.Length; i++)
            {
                SerializedProperty item = lista.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("nombreAnimal").stringValue = Animales[i].nombre;
                item.FindPropertyRelative("quieto").objectReferenceValue = CargarSprite(Animales[i].archivo, "Quieto");
                item.FindPropertyRelative("caminarUno").objectReferenceValue = CargarSprite(Animales[i].archivo, "CaminarUno");
                item.FindPropertyRelative("caminarDos").objectReferenceValue = CargarSprite(Animales[i].archivo, "CaminarDos");
                item.FindPropertyRelative("saltar").objectReferenceValue = CargarSprite(Animales[i].archivo, "Saltar");
                item.FindPropertyRelative("caer").objectReferenceValue = CargarSprite(Animales[i].archivo, "Caer");
                item.FindPropertyRelative("agarrarse").objectReferenceValue = CargarSprite(Animales[i].archivo, "Agarrarse");
            }

            datos.ApplyModifiedProperties();
            animador.AplicarAnimal(indiceInicial);
            return animador;
        }

        private static Text CrearTextoSeleccion()
        {
            GameObject canvas = GameObject.Find("Interfaz Seleccion Animales");
            if (canvas == null)
            {
                canvas = new GameObject("Interfaz Seleccion Animales");
                Canvas canvasComponente = canvas.AddComponent<Canvas>();
                canvasComponente.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                canvas.AddComponent<GraphicRaycaster>();
            }

            GameObject panel = CrearObjetoUi(canvas.transform, "Panel Seleccion Animales");
            Image fondoPanel = ObtenerImagen(panel, new Color(0.05f, 0.08f, 0.11f, 0.86f));
            fondoPanel.raycastTarget = false;
            ConfigurarRect(panel.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1160f, 560f), new Vector2(0.5f, 0.5f));

            Text titulo = CrearTexto(panel.transform, "Texto Seleccion Animales", "ESCOGE TU ANIMAL DE LA SIERRA", 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            ConfigurarRect(titulo.GetComponent<RectTransform>(), new Vector2(0f, 220f), new Vector2(1040f, 70f), new Vector2(0.5f, 0.5f));

            CrearTarjeta(panel.transform, "Azul", new Vector2(-300f, -10f), new Color(0.10f, 0.22f, 0.36f, 0.92f), "JUGADOR AZUL", "A / D cambia    W confirma");
            CrearTarjeta(panel.transform, "Rojo", new Vector2(300f, -10f), new Color(0.34f, 0.13f, 0.14f, 0.92f), "JUGADOR ROJO", "Flechas cambia    Arriba confirma");

            Text ayuda = CrearTexto(panel.transform, "Texto Ayuda Seleccion", "Enter empieza con la seleccion actual de ambos", 22, FontStyle.Normal, TextAnchor.MiddleCenter);
            ayuda.color = new Color(0.88f, 0.92f, 0.96f);
            ConfigurarRect(ayuda.GetComponent<RectTransform>(), new Vector2(0f, -240f), new Vector2(920f, 44f), new Vector2(0.5f, 0.5f));

            return titulo;
        }

        private static void CrearTarjeta(Transform padre, string sufijo, Vector2 posicion, Color colorFondo, string tituloJugador, string ayuda)
        {
            GameObject tarjeta = CrearObjetoUi(padre, $"Tarjeta {sufijo}");
            Image fondo = ObtenerImagen(tarjeta, colorFondo);
            fondo.raycastTarget = false;
            ConfigurarRect(tarjeta.GetComponent<RectTransform>(), posicion, new Vector2(500f, 390f), new Vector2(0.5f, 0.5f));

            Text jugador = CrearTexto(tarjeta.transform, $"Titulo Tarjeta {sufijo}", tituloJugador, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            ConfigurarRect(jugador.GetComponent<RectTransform>(), new Vector2(0f, 150f), new Vector2(390f, 40f), new Vector2(0.5f, 0.5f));

            Image vista = ObtenerImagen(CrearObjetoUi(tarjeta.transform, $"Vista Previa {sufijo}"), Color.white);
            vista.sprite = sufijo == "Azul" ? CargarSprite("Vicuna", "Quieto") : CargarSprite("Alpaca", "Quieto");
            vista.preserveAspect = true;
            vista.raycastTarget = false;
            ConfigurarRect(vista.GetComponent<RectTransform>(), new Vector2(0f, 35f), new Vector2(270f, 210f), new Vector2(0.5f, 0.5f));

            Text nombre = CrearTexto(tarjeta.transform, $"Nombre Animal {sufijo}", sufijo == "Azul" ? "Vicuna" : "Alpaca", 30, FontStyle.Bold, TextAnchor.MiddleCenter);
            nombre.color = new Color(1f, 0.93f, 0.72f);
            ConfigurarRect(nombre.GetComponent<RectTransform>(), new Vector2(0f, -92f), new Vector2(390f, 48f), new Vector2(0.5f, 0.5f));

            Text estado = CrearTexto(tarjeta.transform, $"Estado Animal {sufijo}", ayuda, 20, FontStyle.Normal, TextAnchor.MiddleCenter);
            estado.color = Color.white;
            ConfigurarRect(estado.GetComponent<RectTransform>(), new Vector2(0f, -142f), new Vector2(390f, 46f), new Vector2(0.5f, 0.5f));
        }

        private static GameObject CrearObjetoUi(Transform padre, string nombre)
        {
            GameObject objeto = BuscarObjetoEnEscena(nombre);
            if (objeto == null)
            {
                objeto = new GameObject(nombre);
            }

            objeto.transform.SetParent(padre, false);
            if (objeto.GetComponent<RectTransform>() == null) objeto.AddComponent<RectTransform>();
            return objeto;
        }

        private static Text CrearTexto(Transform padre, string nombre, string contenido, int tamano, FontStyle estilo, TextAnchor alineacion)
        {
            GameObject objeto = CrearObjetoUi(padre, nombre);
            Text texto = objeto.GetComponent<Text>();
            if (texto == null) texto = objeto.AddComponent<Text>();
            texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            texto.fontSize = tamano;
            texto.fontStyle = estilo;
            texto.color = Color.white;
            texto.alignment = alineacion;
            texto.raycastTarget = false;
            texto.text = contenido;
            return texto;
        }

        private static Image ObtenerImagen(GameObject objeto, Color color)
        {
            Image imagen = objeto.GetComponent<Image>();
            if (imagen == null) imagen = objeto.AddComponent<Image>();
            imagen.color = color;
            return imagen;
        }

        private static void ConfigurarRect(RectTransform rect, Vector2 posicion, Vector2 tamano, Vector2 pivote)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = pivote;
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;
        }

        private static Sprite CargarSprite(string animal, string estado)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ObtenerRutaSprite(animal, estado));
        }

        private static string ObtenerRutaSprite(string animal, string estado)
        {
            return $"{RutaAnimales}/{animal}_{estado}.png";
        }

        private static void Limpiar(Texture2D textura)
        {
            Color transparente = new Color(0f, 0f, 0f, 0f);
            Color[] pixeles = new Color[textura.width * textura.height];
            for (int i = 0; i < pixeles.Length; i++) pixeles[i] = transparente;
            textura.SetPixels(pixeles);
        }

        private static void DibujarPixel(Texture2D textura, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= textura.width || y >= textura.height) return;
            textura.SetPixel(x, y, color);
        }

        private static void DibujarRect(Texture2D textura, int x, int y, int ancho, int alto, Color color)
        {
            for (int px = x; px < x + ancho; px++)
            for (int py = y; py < y + alto; py++)
                DibujarPixel(textura, px, py, color);
        }

        private static void DibujarElipse(Texture2D textura, int cx, int cy, int rx, int ry, Color color)
        {
            for (int x = cx - rx; x <= cx + rx; x++)
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                float dx = (x - cx) / (float)rx;
                float dy = (y - cy) / (float)ry;
                if (dx * dx + dy * dy <= 1f) DibujarPixel(textura, x, y, color);
            }
        }

        private static void DibujarElipseConBorde(Texture2D textura, int cx, int cy, int rx, int ry, Color borde, Color relleno)
        {
            DibujarElipse(textura, cx, cy, rx + 2, ry + 2, borde);
            DibujarElipse(textura, cx, cy, rx, ry, relleno);
        }

        private static void DibujarPierna(Texture2D textura, int x, int y, Color borde, Color color)
        {
            DibujarRect(textura, x - 3, y - 1, 6, 16, borde);
            DibujarRect(textura, x - 5, y - 3, 10, 4, borde);
            DibujarRect(textura, x - 2, y, 4, 14, color);
            DibujarRect(textura, x - 4, y - 2, 8, 3, color);
        }

        private static void DibujarTriangulo(Texture2D textura, Vector2Int a, Vector2Int b, Vector2Int c, Color color)
        {
            int minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
            int maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            int minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
            int maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 p = new Vector2(x, y);
                if (PuntoEnTriangulo(p, a, b, c)) DibujarPixel(textura, x, y, color);
            }
        }

        private static bool PuntoEnTriangulo(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1f / (2f * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1f / (2f * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            float u = 1f - s - t;
            return s >= 0f && t >= 0f && u >= 0f;
        }

        private static Color Oscurecer(Color color, float factor)
        {
            return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
        }

        private static Color Aclarar(Color color, float factor)
        {
            return new Color(Mathf.Clamp01(color.r * factor), Mathf.Clamp01(color.g * factor), Mathf.Clamp01(color.b * factor), color.a);
        }
    }
}




