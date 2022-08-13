using NUnit.Framework;
using Unity.Mathematics;
using SM.Physics.Authoring;
using UnityEngine;

namespace SM.Physics.Tests.Authoring
{
    class CapsuleGeometryAuthoring_UnitTests
    {
        [Test]
        public void SetOrientation_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new CapsuleGeometryAuthoring { Orientation = quaternion.identity });
        }
    }
}
