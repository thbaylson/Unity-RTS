using UnityEngine;

namespace GameDevTV.RTS.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class HoldGunIK : MonoBehaviour
    {
        [SerializeField] private Transform leftHandIKTarget;
        [SerializeField] private Transform rightHandIKTarget;
        [SerializeField] private Transform leftElbowIKTarget;
        [SerializeField] private Transform rightElbowIKTarget;

        [SerializeField] [Range(0, 1f)] private float handIKAmount = 1f;
        [SerializeField] [Range(0, 1f)] private float elbowIKAmount = 1f;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (leftHandIKTarget != null)
            {
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKAmount);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKAmount);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
            }
            if (rightHandIKTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKAmount);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKAmount);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
            }
            if (leftElbowIKTarget != null)
            {
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowIKTarget.position);
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowIKAmount);
            }

            if (rightElbowIKTarget != null)
            {
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowIKTarget.position);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowIKAmount);
            }
        }
    }
}
