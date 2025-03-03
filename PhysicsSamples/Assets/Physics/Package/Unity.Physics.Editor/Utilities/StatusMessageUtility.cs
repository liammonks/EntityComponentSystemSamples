using System.Collections.Generic;
using System.Linq;
using SM.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
#if LEGACY_PHYSICS
using LegacyRigidBody = UnityEngine.Rigidbody;
#endif

namespace SM.Physics.Editor
{
    static class StatusMessageUtility
    {
        public static MessageType GetHierarchyStatusMessage(IReadOnlyList<UnityObject> targets, out string statusMessage)
        {
            statusMessage = string.Empty;
            if (targets.Count == 0)
                return MessageType.None;

            var numChildTargets = 0;
            foreach (Component c in targets)
            {
                // hierarchy roots and leaf shapes do not emit a message
                if (
                    c == null
                    || c.transform.parent == null
                    || PhysicsShapeExtensions.GetPrimaryBody(c.gameObject) != c.gameObject
                )
                    continue;

                var targetType = c.GetType();
                // only bodies (both explicit and implicit static bodies) will emit a message
                if (
                    targetType == typeof(PhysicsBodyAuthoring)
#if LEGACY_PHYSICS
                    || targetType == typeof(LegacyRigidBody)
#endif
                    || c.GetComponent<PhysicsBodyAuthoring>() == null
#if LEGACY_PHYSICS
                    && c.GetComponent<LegacyRigidBody>() == null
#endif
                )
                    ++numChildTargets;
            }

            switch (numChildTargets)
            {
                case 0:
                    return MessageType.None;
                case 1:
                    statusMessage =
                        L10n.Tr("Target will be un-parented during the conversion process in order to take part in physics simulation.");
                    return MessageType.Warning;
                default:
                    statusMessage =
                        L10n.Tr("One or more targets will be un-parented during the conversion process in order to take part in physics simulation.");
                    return MessageType.Warning;
            }
        }

        public static MessageType GetMatrixStatusMessage(
            IReadOnlyList<MatrixState> matrixStates, out string statusMessage
        )
        {
            statusMessage = string.Empty;
            if (matrixStates.Contains(MatrixState.NotValidTRS))
            {
                statusMessage = L10n.Tr(
                    matrixStates.Count == 1
                    ? "Target's local-to-world matrix is not a valid transformation."
                    : "One or more targets' local-to-world matrices are not valid transformations."
                );
                return MessageType.Error;
            }

            if (matrixStates.Contains(MatrixState.ZeroScale))
            {
                statusMessage =
                    L10n.Tr(matrixStates.Count == 1 ? "Target has zero scale." : "One or more targets has zero scale.");
                return MessageType.Warning;
            }

            if (matrixStates.Contains(MatrixState.NonUniformScale))
            {
                statusMessage = L10n.Tr(
                    matrixStates.Count == 1
                    ? "Target has non-uniform scale. Shape data will be transformed during conversion in order to bake scale into the run-time format."
                    : "One or more targets has non-uniform scale. Shape data will be transformed during conversion in order to bake scale into the run-time format."
                );
                return MessageType.Warning;
            }

            return MessageType.None;
        }
    }
}
