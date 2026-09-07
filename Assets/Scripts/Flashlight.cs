using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Linterna del jugador: ilumina hacia donde mira y quema a lo que caiga dentro del cono.
/// La luz que se ve y el area que hace daño salen de los mismos campos (range y angle),
/// asi no se pueden despegar.
/// </summary>
[RequireComponent(typeof(Entity))]
public class Flashlight : MonoBehaviour
{
    [Header("Haz")]
    [Tooltip("Alcance en unidades del mundo.")]
    [SerializeField] private float range = 4f;
    [Tooltip("Apertura total del cono en grados.")]
    [SerializeField] private float angle = 60f;
    [SerializeField] private float intensity = 1.4f;
    [SerializeField] private Color color = new Color(1f, 0.96f, 0.8f);

    [Header("Daño")]
    [Tooltip("Daño por segundo a lo que este dentro del cono.")]
    [SerializeField] private float damagePerSecond = 25f;
    [SerializeField] private LayerMask targetLayers = ~0;

    [Header("Referencias")]
    [Tooltip("Opcional. Si lo dejas vacio se crea un Light2D hijo automaticamente.")]
    [SerializeField] private Light2D beam;

    /// <summary>Permite apagarla luego (bateria, cinematicas) sin desactivar el componente.</summary>
    public bool IsOn { get; private set; } = true;

    private Entity _owner;
    private Transform _beamPivot;
    private float _damageBudget;

    private readonly List<Collider2D> _overlaps = new();
    private ContactFilter2D _filter;

    private void Awake()
    {
        _owner = GetComponent<Entity>();

        if (beam == null)
            beam = CreateBeam();

        _beamPivot = beam.transform;

        _filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = targetLayers,
            useTriggers = true
        };

        ApplyBeamShape();
    }

    private void Update()
    {
        if (!IsOn || !_owner.IsAlive)
            return;

        Aim();
        Burn();
    }

    public void SetOn(bool on)
    {
        IsOn = on;

        if (beam != null)
            beam.enabled = on;
    }

    /// <summary>Gira solo el hijo de la luz, nunca al jugador: rotar la raiz torceria sprite y collider.</summary>
    private void Aim()
    {
        Vector2 facing = _owner.Facing;
        if (facing.sqrMagnitude < 0.0001f)
            return;

        // El cono de un Light2D de tipo Point sale por su eje local +Y, de ahi el -90.
        float degrees = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
        _beamPivot.rotation = Quaternion.Euler(0f, 0f, degrees);
    }

    private void Burn()
    {
        // Acumulamos el daño para poder tunear en daño/segundo aunque TakeDamage sea entero.
        _damageBudget += damagePerSecond * Time.deltaTime;

        int damage = Mathf.FloorToInt(_damageBudget);
        if (damage <= 0)
            return;

        _damageBudget -= damage;

        Vector2 origin = transform.position;
        Vector2 facing = _owner.Facing;
        float halfAngle = angle * 0.5f;

        Physics2D.OverlapCircle(origin, range, _filter, _overlaps);

        foreach (Collider2D hit in _overlaps)
        {
            Entity victim = hit.GetComponentInParent<Entity>();
            if (victim == null || victim == _owner || !victim.IsAlive)
                continue;

            Vector2 toVictim = (Vector2)victim.transform.position - origin;
            if (Vector2.Angle(facing, toVictim) > halfAngle)
                continue;

            victim.TakeDamage(damage, _owner, ignoreInvulnerability: true);
        }
    }

    private Light2D CreateBeam()
    {
        var go = new GameObject("Flashlight Beam");
        go.transform.SetParent(transform, false);

        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        return light;
    }

    private void ApplyBeamShape()
    {
        beam.color = color;
        beam.intensity = intensity;
        beam.pointLightOuterRadius = range;
        beam.pointLightInnerRadius = range * 0.15f;
        beam.pointLightOuterAngle = angle;
        beam.pointLightInnerAngle = angle * 0.5f;
    }

    private void OnValidate()
    {
        // Para que tocar los sliders en el inspector actualice la luz al vuelo.
        if (beam != null)
            ApplyBeamShape();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        float facingDegrees = Application.isPlaying && _owner != null
            ? Mathf.Atan2(_owner.Facing.y, _owner.Facing.x) * Mathf.Rad2Deg
            : -90f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, Rotate(facingDegrees - angle * 0.5f) * range);
        Gizmos.DrawRay(origin, Rotate(facingDegrees + angle * 0.5f) * range);
    }

    private static Vector3 Rotate(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
    }
}
