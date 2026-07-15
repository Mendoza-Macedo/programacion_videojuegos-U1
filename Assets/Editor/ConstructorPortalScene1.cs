using JuegoCooperativo.Mundo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConstructorPortalScene1
{
    private const string RutaScene1 = "Assets/Scenes/Scene1.unity";
    private const string RutaScenes = "Assets/Scenes";
    private const string NombrePortal = "Portal siguiente escena";
    private const string NombreBase = "Base portal Scene1";
    private const string NombreMonedaPoder = "Moneda azul potenciadora";

    [MenuItem("Herramientas/BreadFred/Agregar portal solo a Scene1")]
    public static void AgregarPortalSoloScene1()
    {
        LimpiarPortalesFueraDeScene1();

        Scene escena = EditorSceneManager.OpenScene(RutaScene1, OpenSceneMode.Single);
        Vector3 posicionPortal = ObtenerPosicionPortal();
        CrearOActualizarPortal(posicionPortal);
        CrearOActualizarBase(posicionPortal + Vector3.down * 1.15f);
        CrearOActualizarMonedaPoder(new Vector3(5.6f, 13.2f, 0f));
        CrearOActualizarBloqueEmpujable("Bloque empujable 1", new Vector3(2.2f, 12.5f, 0f), new Vector3(1.2f, 1.2f, 1f), 3.5f);
        CrearOActualizarBloqueEmpujable("Bloque empujable 2", new Vector3(8.7f, 8.6f, 0f), new Vector3(1.25f, 1.25f, 1f), 4.2f);
        CrearOActualizarBloqueEmpujable("Bloque empujable 3", new Vector3(13.4f, 7.1f, 0f), new Vector3(1.15f, 1.15f, 1f), 3.8f);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena, RutaScene1);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Portal agregado solo en Scene1.");
    }

    private static Vector3 ObtenerPosicionPortal()
    {
        GameObject meta = GameObject.Find("Meta");
        if (meta != null)
        {
            return meta.transform.position + new Vector3(2.2f, 0.35f, 0f);
        }

        return new Vector3(17.5f, 9.2f, 0f);
    }

    private static void CrearOActualizarPortal(Vector3 posicion)
    {
        GameObject portal = GameObject.Find(NombrePortal);
        if (portal == null)
        {
            portal = new GameObject(NombrePortal);
        }

        portal.transform.position = posicion;
        portal.transform.localScale = new Vector3(1.25f, 2.15f, 1f);

        SpriteRenderer render = portal.GetComponent<SpriteRenderer>();
        if (render == null) render = portal.AddComponent<SpriteRenderer>();
        render.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
        render.color = new Color(0.35f, 0.85f, 1f, 0.84f);
        render.sortingOrder = 30;

        BoxCollider2D colision = portal.GetComponent<BoxCollider2D>();
        if (colision == null) colision = portal.AddComponent<BoxCollider2D>();
        colision.isTrigger = true;
        colision.size = Vector2.one;

        PortalSiguienteEscena script = portal.GetComponent<PortalSiguienteEscena>();
        if (script == null) portal.AddComponent<PortalSiguienteEscena>();
    }

    private static void CrearOActualizarMonedaPoder(Vector3 posicion)
    {
        GameObject moneda = GameObject.Find(NombreMonedaPoder);
        if (moneda == null)
        {
            moneda = new GameObject(NombreMonedaPoder);
        }

        moneda.transform.position = posicion;
        moneda.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer render = moneda.GetComponent<SpriteRenderer>();
        if (render == null) render = moneda.AddComponent<SpriteRenderer>();
        render.sprite = ObtenerSpriteMoneda();
        render.color = new Color(0.18f, 0.72f, 1f, 1f);
        render.sortingOrder = 32;

        CircleCollider2D colision = moneda.GetComponent<CircleCollider2D>();
        if (colision == null) colision = moneda.AddComponent<CircleCollider2D>();
        colision.isTrigger = true;
        colision.radius = 0.42f;

        MonedaPotenciadora script = moneda.GetComponent<MonedaPotenciadora>();
        if (script == null) moneda.AddComponent<MonedaPotenciadora>();
    }

    private static Sprite ObtenerSpriteMoneda()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Gordinillo_assets/Spritesheet/collectibles_coin_silver.png");
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite) return sprite;
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
    }

    private static void CrearOActualizarBloqueEmpujable(string nombre, Vector3 posicion, Vector3 escala, float masa)
    {
        GameObject bloque = GameObject.Find(nombre);
        if (bloque == null)
        {
            bloque = new GameObject(nombre);
        }

        bloque.transform.position = posicion;
        bloque.transform.localScale = escala;

        SpriteRenderer render = bloque.GetComponent<SpriteRenderer>();
        if (render == null) render = bloque.AddComponent<SpriteRenderer>();
        render.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
        render.color = new Color(0.46f, 0.32f, 0.22f, 1f);
        render.sortingOrder = 15;

        BoxCollider2D colision = bloque.GetComponent<BoxCollider2D>();
        if (colision == null) colision = bloque.AddComponent<BoxCollider2D>();
        colision.isTrigger = false;
        colision.size = Vector2.one;

        Rigidbody2D cuerpo = bloque.GetComponent<Rigidbody2D>();
        if (cuerpo == null) cuerpo = bloque.AddComponent<Rigidbody2D>();
        cuerpo.bodyType = RigidbodyType2D.Dynamic;
        cuerpo.gravityScale = 1f;
        cuerpo.mass = masa;
        cuerpo.freezeRotation = true;
        cuerpo.linearDamping = 0.8f;

        BloqueEmpujable script = bloque.GetComponent<BloqueEmpujable>();
        if (script == null) bloque.AddComponent<BloqueEmpujable>();
    }

    private static void CrearOActualizarBase(Vector3 posicion)
    {
        GameObject basePortal = GameObject.Find(NombreBase);
        if (basePortal == null)
        {
            basePortal = new GameObject(NombreBase);
        }

        basePortal.transform.position = posicion;
        basePortal.transform.localScale = new Vector3(3.25f, 0.35f, 1f);

        SpriteRenderer render = basePortal.GetComponent<SpriteRenderer>();
        if (render == null) render = basePortal.AddComponent<SpriteRenderer>();
        render.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
        render.color = new Color(0.14f, 0.40f, 0.48f, 1f);
        render.sortingOrder = 12;

        BoxCollider2D colision = basePortal.GetComponent<BoxCollider2D>();
        if (colision == null) colision = basePortal.AddComponent<BoxCollider2D>();
        colision.isTrigger = false;
        colision.size = Vector2.one;
    }

    private static void LimpiarPortalesFueraDeScene1()
    {
        for (int i = 2; i <= 8; i++)
        {
            string ruta = $"{RutaScenes}/Scene{i}.unity";
            Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            bool cambio = DestruirSiExiste(NombrePortal);
            cambio |= DestruirSiExiste($"Base portal Scene{i}");
            cambio |= DestruirSiExiste(NombreBase);

            if (cambio)
            {
                EditorSceneManager.MarkSceneDirty(escena);
                EditorSceneManager.SaveScene(escena, ruta);
            }
        }
    }

    private static bool DestruirSiExiste(string nombre)
    {
        GameObject objeto = GameObject.Find(nombre);
        if (objeto == null) return false;

        Object.DestroyImmediate(objeto);
        return true;
    }
}
