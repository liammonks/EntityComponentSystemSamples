using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Assert = UnityEngine.Assertions.Assert;

namespace SM.Physics.Tests.Dynamics.Jacobians
{
    class JacobiansTests
    {
        [Test]
        public void JacobianUtilitiesCalculateTauAndDampingTest()
        {
            float springFrequency = 1.0f;
            float springDampingRatio = 1.0f;
            float timestep = 1.0f;
            int iterations = 4;

            JacobianUtilities.CalculateConstraintTauAndDamping(springFrequency, springDampingRatio, timestep, iterations, out float tau, out float damping);

            Assert.AreApproximatelyEqual(0.4774722f, tau);
            Assert.AreApproximatelyEqual(0.6294564f, damping);
        }

        [Test]
        public void JacobianUtilitiesCalculateTauAndDampingFromConstraintTest()
        {
            var constraint = new Constraint { SpringFrequency = 1.0f, SpringDamping = 1.0f };
            float timestep = 1.0f;
            int iterations = 4;

            float tau;
            float damping;
            JacobianUtilities.CalculateConstraintTauAndDamping(constraint.SpringFrequency, constraint.SpringDamping, timestep, iterations, out tau, out damping);

            Assert.AreApproximatelyEqual(0.4774722f, tau);
            Assert.AreApproximatelyEqual(0.6294564f, damping);
        }

        [Test]
        public void JacobianUtilitiesCalculateErrorTest()
        {
            float x = 5.0f;
            float min = 0.0f;
            float max = 10.0f;

            Assert.AreApproximatelyEqual(0.0f, JacobianUtilities.CalculateError(x, min, max));
        }

        [Test]
        public void JacobianUtilitiesCalculateCorrectionTest()
        {
            float predictedError = 0.2f;
            float initialError = 0.1f;
            float tau = 0.6f;
            float damping = 1.0f;

            Assert.AreApproximatelyEqual(0.16f, JacobianUtilities.CalculateCorrection(predictedError, initialError, tau, damping));
        }

        [Test]
        public void JacobianUtilitiesIntegrateOrientationBFromATest()
        {
            var bFromA = quaternion.identity;
            var angularVelocityA = float3.zero;
            var angularVelocityB = float3.zero;
            var timestep = 1.0f;

            Assert.AreEqual(quaternion.identity, JacobianUtilities.IntegrateOrientationBFromA(bFromA, angularVelocityA, angularVelocityB, timestep));
        }

        [Test]
        public void JacobianIteratorHasJacobiansLeftTest()
        {
            var jacobianStream = new NativeStream(1, Allocator.Temp);
            NativeStream.Reader jacobianStreamReader = jacobianStream.AsReader();
            int workItemIndex = 0;
            var jacIterator = new JacobianIterator(jacobianStreamReader, workItemIndex);

            Assert.IsFalse(jacIterator.HasJacobiansLeft());

            jacobianStream.Dispose();
        }
    }
}
