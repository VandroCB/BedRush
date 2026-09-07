using UnityEngine;

/// <summary>
/// Barra de vida flotante sobre una Entity. Se construye sola con SpriteRenderers,
/// asi que no necesita Canvas, prefabs ni sprites en disco: la pones y ya.
/// Sirve igual para el jugador y para los enemigos.
/// </summary>
[RequireComponent(typeof(Entity))]
public class HealthBar : MonoBehaviour
{
    [Header("Colocacion")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.7f);
    [SerializeField] private Vector2 size = new Vector2(1f, 0.12f);
    [Tooltip("Alto para que quede por encima de sprites y enemigos.")]
    [SerializeField] private int sortingOrder = 100;

    [Header("Colores")]
    [SerializeField] private Color fillColor = new Color(0.85f, 0.15f, 0.15f);
    [SerializeField] private Color backColor = new Color(0f, 0f, 0f, 0.7f);

    [Header("Comportamiento")]
    [Tooltip("Ocultarla mientras la vida este llena. Comodo para enemigos.")]
    [SerializeField] private bool hideWhenFull;
    [Tooltip("Ocultarla al morir.")]
    [SerializeField] private bool hideOnDeath = true;

    private static Sprite _pixel;

    private Entity _entity;
    private Transform _fill;
    private SpriteRenderer _fillRenderer;
    private SpriteRenderer _backRenderer;

    private void Awake()
    {
        _entity = GetComponent<Entity>();
        Build();
    }

    private void LateUpdate()
    {
        // Consultamos la vida en vez de escuchar eventos: asi tambien recoge Heal()
        // y cualquier cambio futuro sin tener que acordarse de disparar nada.
        float ratio = _entity.MaxHealth > 0
            ? Mathf.Clamp01((float)_entity.Health / _entity.MaxHealth)
            : 0f;

        bool visible = !(hideOnDeath && !_entity.IsAlive) && !(hideWhenFull && ratio >= 1f);

        _backRenderer.enabled = visible;
        _fillRenderer.enabled = visible && ratio > 0f;

        if (!visible)
            return;

        // El relleno se encoge hacia la izquierda, no hacia el centro.
        _fill.localScale = new Vector3(size.x * ratio, size.y, 1f);
        _fill.localPosition = new Vector3(-size.x * (1f - ratio) * 0.5f, 0f, 0f);
    }

    private void Build()
    {
        var root = new GameObject("Health Bar").transform;
        root.SetParent(transform, false);
        root.localPosition = offset;

        _backRenderer = CreateQuad(root, "Back", backColor, sortingOrder);
        _backRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);

        _fillRenderer = CreateQuad(root, "Fill", fillColor, sortingOrder + 1);
        _fill = _fillRenderer.transform;
    }

    private static SpriteRenderer CreateQuad(Transform parent, string name, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPixel();
        renderer.color = color;
        renderer.sortingOrder = order;

        // Material sin iluminar a proposito: con la casa a oscuras, una barra
        // afectada por las luces 2D se veria negra justo cuando mas la necesitas.
        Shader unlit = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                       ?? Shader.Find("Sprites/Default");

        if (unlit != null)
            renderer.material = new Material(unlit);

        return renderer;
    }

    private static Sprite GetPixel()
    {
        // Comparacion con == para que use el null falso de UnityEngine.Object.
        if (_pixel == null)
        {
            var texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // pixelsPerUnit 1 => el localScale del transform es el tamaño en unidades del mundo.
            _pixel = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            _pixel.name = "HealthBarPixel";
        }

        return _pixel;
    }
}
