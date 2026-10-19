using UnityEngine;
using static UnityEngine.GraphicsBuffer;


// Camera 'Dead zone' size and follow smoothness come from GameConfig.txt.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (target == null)
            return;

        // Start centred on the target.
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 position = transform.position;
        Vector2 halfZone = new Vector2(GameConfig.CameraDeadZoneWidth, GameConfig.CameraDeadZoneHeight) * 0.5f;
        Vector2 offset = (Vector2)target.position - (Vector2)position;

        // How far the target is outside the box, per axis (0 while inside).
        Vector2 push = Vector2.zero;

        if (offset.x > halfZone.x) push.x = offset.x - halfZone.x;
        else if (offset.x < -halfZone.x) push.x = offset.x + halfZone.x;

        if (offset.y > halfZone.y) push.y = offset.y - halfZone.y;
        else if (offset.y < -halfZone.y) push.y = offset.y + halfZone.y;

        Vector3 desired = position + (Vector3)push;

        float sharpness = GameConfig.CameraFollowSharpness;
        float t = sharpness <= 0f ? 1f : 1f - Mathf.Exp(-sharpness * Time.deltaTime);

        transform.position = Vector3.Lerp(position, desired, t);
    }
}
