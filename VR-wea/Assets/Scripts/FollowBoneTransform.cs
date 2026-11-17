using UnityEngine;

public class FollowBoneTransform : MonoBehaviour
{
    [Tooltip("Bone034 / Flame_v2")]
    public Transform targetToFollow;
    [Tooltip("Offset")]
    public Vector3 localOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (targetToFollow == null) return;

        
        transform.position = targetToFollow.TransformPoint(localOffset);
        transform.rotation = targetToFollow.rotation;
    }
}
