using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using SM.Physics;
using SM.Physics.Authoring;
using SM.Physics.Systems;
using UnityEditor;
using UnityEngine;

namespace SM.Physics.Authoring
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public partial class DebugStream : SystemBase
    {
        readonly List<NativeStream> m_DebugStreams = new List<NativeStream>();
        DrawComponent m_DrawComponent;
        internal JobHandle FinalJobHandle => Dependency;

        public struct Context
        {
            public void Begin(int index)
            {
                Writer.BeginForEachIndex(index);
            }

            public void End()
            {
                Writer.EndForEachIndex();
            }

            internal void Point(float3 x, float size, Unity.DebugDisplay.ColorIndex color)
            {
                var lines = new Unity.DebugDisplay.Lines(3);

                lines.Draw(x - new float3(size, 0, 0), x + new float3(size, 0, 0), color);
                lines.Draw(x - new float3(0, size, 0), x + new float3(0, size, 0), color);
                lines.Draw(x - new float3(0, 0, size), x + new float3(0, 0, size), color);
            }

            internal void Line(float3 x0, float3 x1, Unity.DebugDisplay.ColorIndex color)
            {
                Unity.DebugDisplay.Line.Draw(x0, x1, color);
            }

            internal void Arrow(float3 x, float3 v, Unity.DebugDisplay.ColorIndex color)
            {
                Unity.DebugDisplay.Arrow.Draw(x, v, color);
            }

            internal void Plane(float3 x, float3 v, Unity.DebugDisplay.ColorIndex color)
            {
                Unity.DebugDisplay.Plane.Draw(x, v, color);
            }

            internal void Arc(float3 center, float3 normal, float3 arm, float angle,
                Unity.DebugDisplay.ColorIndex color)
            {
                Unity.DebugDisplay.Arc.Draw(center, normal, arm, angle, color);
            }

            internal void Cone(float3 point, float3 axis, float angle, Unity.DebugDisplay.ColorIndex color)
            {
                Unity.DebugDisplay.Cone.Draw(point, axis, angle, color);
            }

            internal void Box(float3 Size, float3 Center, quaternion Orientation, Unity.DebugDisplay.ColorIndex color)
            {
                Unity.DebugDisplay.Box.Draw(Size, Center, Orientation, color);
            }

            public void Point(float3 x, float size, Color color)
            {
                Writer.Write(Type.Point);
                Writer.Write(new Point {X = x, Size = size, Color = color});
            }

            public void Line(float3 x0, float3 x1, Color color)
            {
                Writer.Write(Type.Line);
                Writer.Write(new Line {X0 = x0, X1 = x1, Color = color});
            }

            public void Arrow(float3 x, float3 v, Color color)
            {
                Writer.Write(Type.Arrow);
                Writer.Write(new Line {X0 = x, X1 = x + v, Color = color});
            }

            public void Plane(float3 x, float3 v, Color color)
            {
                Writer.Write(Type.Plane);
                Writer.Write(new Line {X0 = x, X1 = x + v, Color = color});
            }

            public void Circle(float3 x, float3 v, Color color)
            {
                Writer.Write(Type.Circle);
                Writer.Write(new Line {X0 = x, X1 = x + v, Color = color});
            }

            public void Arc(float3 center, float3 normal, float3 arm, float angle, Color color)
            {
                Writer.Write(Type.Arc);
                Writer.Write(new Arc {Center = center, Normal = normal, Arm = arm, Angle = angle, Color = color});
            }

            public void Cone(float3 point, float3 axis, float angle, Color color)
            {
                Writer.Write(Type.Cone);
                Writer.Write(new Cone {Point = point, Axis = axis, Angle = angle, Color = color});
            }

            public void Box(float3 size, float3 center, quaternion orientation, Color color)
            {
                Writer.Write(Type.Box);
                Writer.Write(new Box {Size = size, Center = center, Orientation = orientation, Color = color});
            }

            public void Text(char[] text, float3 x, Color color)
            {
                Writer.Write(Type.Text);
                Writer.Write(new Text {X = x, Color = color, Length = text.Length});

                foreach (char c in text)
                {
                    Writer.Write(c);
                }
            }

            internal NativeStream.Writer Writer;
        }

        public Context GetContext(int foreachCount)
        {
            var stream = new NativeStream(foreachCount, Allocator.TempJob);
            m_DebugStreams.Add(stream);
            return new Context {Writer = stream.AsWriter()};
        }

        public enum Type
        {
            Point,
            Line,
            Arrow,
            Plane,
            Circle,
            Arc,
            Cone,
            Text,
            Box
        }

        public struct Point
        {
            public float3 X;
            public float Size;
            public Color Color;

            public void Draw()
            {
#if UNITY_EDITOR
                Handles.color = Color;
                Handles.DrawLine(X - new float3(Size, 0, 0), X + new float3(Size, 0, 0));
                Handles.DrawLine(X - new float3(0, Size, 0), X + new float3(0, Size, 0));
                Handles.DrawLine(X - new float3(0, 0, Size), X + new float3(0, 0, Size));
#endif
            }
        }

        public struct Line
        {
            public float3 X0;
            public float3 X1;
            public Color Color;

            public void Draw()
            {
#if UNITY_EDITOR
                Handles.color = Color;
                Handles.DrawLine(X0, X1);
#endif
            }

            public void DrawArrow()
            {
                if (!math.all(X0 == X1))
                {
#if UNITY_EDITOR
                    Handles.color = Color;

                    Handles.DrawLine(X0, X1);
                    float3 v = X1 - X0;
                    float3 dir;
                    float length = Math.NormalizeWithLength(v, out dir);
                    float3 perp, perp2;
                    Math.CalculatePerpendicularNormalized(dir, out perp, out perp2);
                    float3 scale = length * 0.2f;

                    Handles.DrawLine(X1, X1 + (perp - dir) * scale);
                    Handles.DrawLine(X1, X1 - (perp + dir) * scale);
                    Handles.DrawLine(X1, X1 + (perp2 - dir) * scale);
                    Handles.DrawLine(X1, X1 - (perp2 + dir) * scale);
#endif
                }
            }

            public void DrawPlane()
            {
                if (!math.all(X0 == X1))
                {
#if UNITY_EDITOR
                    Handles.color = Color;

                    Handles.DrawLine(X0, X1);
                    float3 v = X1 - X0;
                    float3 dir;
                    float length = Math.NormalizeWithLength(v, out dir);
                    float3 perp, perp2;
                    Math.CalculatePerpendicularNormalized(dir, out perp, out perp2);
                    float3 scale = length * 0.2f;

                    Handles.DrawLine(X1, X1 + (perp - dir) * scale);
                    Handles.DrawLine(X1, X1 - (perp + dir) * scale);
                    Handles.DrawLine(X1, X1 + (perp2 - dir) * scale);
                    Handles.DrawLine(X1, X1 - (perp2 + dir) * scale);

                    perp *= length;
                    perp2 *= length;
                    Handles.DrawLine(X0 + perp + perp2, X0 + perp - perp2);
                    Handles.DrawLine(X0 + perp - perp2, X0 - perp - perp2);
                    Handles.DrawLine(X0 - perp - perp2, X0 - perp + perp2);
                    Handles.DrawLine(X0 - perp + perp2, X0 + perp + perp2);
#endif
                }
            }

            public void DrawCircle()
            {
                if (!math.all(X0 == X1))
                {
#if UNITY_EDITOR
                    Handles.color = Color;

                    float3 v = X1 - X0;
                    float3 dir;
                    float length = Math.NormalizeWithLength(v, out dir);
                    float3 perp, perp2;
                    Math.CalculatePerpendicularNormalized(dir, out perp, out perp2);
                    float3 scale = length * 0.2f;

                    const int res = 16;
                    quaternion q = quaternion.AxisAngle(dir, 2.0f * (float)math.PI / res);
                    float3 arm = perp * length;
                    for (int i = 0; i < res; i++)
                    {
                        float3 nextArm = math.mul(q, arm);
                        Handles.DrawLine(X0 + arm, X0 + nextArm);
                        arm = nextArm;
                    }
#endif
                }
            }
        }

        public struct Arc
        {
            public float3 Center;
            public float3 Normal;
            public float3 Arm;
            public float Angle;
            public Color Color;

            public void Draw()
            {
#if UNITY_EDITOR
                Handles.color = Color;

                const int res = 16;
                quaternion q = quaternion.AxisAngle(Normal, Angle / res);
                float3 currentArm = Arm;
                Handles.DrawLine(Center, Center + currentArm);
                for (int i = 0; i < res; i++)
                {
                    float3 nextArm = math.mul(q, currentArm);
                    Handles.DrawLine(Center + currentArm, Center + nextArm);
                    currentArm = nextArm;
                }

                Handles.DrawLine(Center, Center + currentArm);
#endif
            }
        }

        public struct Cone
        {
            public float3 Point;
            public float3 Axis;
            public float Angle;
            public Color Color;

            public void Draw()
            {
#if UNITY_EDITOR
                Handles.color = Color;

                float3 dir;
                float scale = Math.NormalizeWithLength(Axis, out dir);

                float3 arm;
                {
                    float3 perp1, perp2;
                    Math.CalculatePerpendicularNormalized(dir, out perp1, out perp2);
                    arm = math.mul(quaternion.AxisAngle(perp1, Angle), dir) * scale;
                }

                const int res = 16;
                quaternion q = quaternion.AxisAngle(dir, 2.0f * (float)math.PI / res);
                for (int i = 0; i < res; i++)
                {
                    float3 nextArm = math.mul(q, arm);
                    Handles.DrawLine(Point, Point + arm);
                    Handles.DrawLine(Point + arm, Point + nextArm);
                    arm = nextArm;
                }
#endif
            }
        }

        struct Box
        {
            public float3 Size;
            public float3 Center;
            public quaternion Orientation;
            public Color Color;

            public void Draw()
            {
#if UNITY_EDITOR
                Matrix4x4 orig = Handles.matrix;

                Matrix4x4 mat = Matrix4x4.TRS(Center, Orientation, Vector3.one);
                Handles.matrix = mat;
                Handles.color = Color;
                Handles.DrawWireCube(Vector3.zero, new Vector3(Size.x, Size.y, Size.z));

                Handles.matrix = orig;
#endif
            }
        }

        struct Text
        {
            public float3 X;
            public Color Color;
            public int Length;

            public void Draw(ref NativeStream.Reader reader)
            {
                // Read string data.
                char[] stringBuf = new char[Length];
                for (int i = 0; i < Length; i++)
                {
                    stringBuf[i] = reader.Read<char>();
                }

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color;
#if UNITY_EDITOR
                Handles.Label(X, new string(stringBuf), style);
#endif
            }
        }

        private void Draw()
        {
            for (int i = 0; i < m_DebugStreams.Count; i++)
            {
                NativeStream.Reader reader = m_DebugStreams[i].AsReader();
                for (int j = 0; j != reader.ForEachCount; j++)
                {
                    reader.BeginForEachIndex(j);
                    while (reader.RemainingItemCount != 0)
                    {
                        switch (reader.Read<Type>())
                        {
                            case Type.Point:
                                reader.Read<Point>().Draw();
                                break;
                            case Type.Line:
                                reader.Read<Line>().Draw();
                                break;
                            case Type.Arrow:
                                reader.Read<Line>().DrawArrow();
                                break;
                            case Type.Plane:
                                reader.Read<Line>().DrawPlane();
                                break;
                            case Type.Circle:
                                reader.Read<Line>().DrawCircle();
                                break;
                            case Type.Arc:
                                reader.Read<Arc>().Draw();
                                break;
                            case Type.Cone:
                                reader.Read<Cone>().Draw();
                                break;
                            case Type.Text:
                                reader.Read<Text>().Draw(ref reader);
                                break;
                            case Type.Box:
                                reader.Read<Box>().Draw();
                                break;
                            default: return; // unknown type
                        }
                    }

                    reader.EndForEachIndex();
                }
            }
#if UNITY_EDITOR
            Unity.DebugDisplay.DebugDisplay.Render();
#endif
        }

        private class DrawComponent : MonoBehaviour
        {
            public DebugStream DebugDraw;

            public void OnDrawGizmos()
            {
                if (DebugDraw != null)
                {
                    // Make sure all potential debug display jobs are finished
                    DebugDraw.FinalJobHandle.Complete();
                    DebugDraw.Draw();
                }
            }
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            this.RegisterPhysicsRuntimeSystemReadOnly();
        }

        protected override void OnUpdate()
        {
            // Read/write the display singleton to register as writing data
            if (HasSingleton<PhysicsDebugDisplayData>())
            {
                var data = GetSingleton<PhysicsDebugDisplayData>();
                SetSingleton(data);
            }

            // Reset
            ResetDebugStreams();

            // Set up component to draw
            if (m_DrawComponent == null)
            {
                m_DrawComponent = new GameObject("DebugStream.DrawComponent", typeof(DrawComponent))
                {
                    hideFlags = HideFlags.DontSave
                }.GetComponent<DrawComponent>();
                m_DrawComponent.DebugDraw = this;
                Unity.DebugDisplay.DebugDisplay.Instantiate();
            }
        }

        protected override void OnDestroy()
        {
            // Clean up the DrawComponent GameObject.
            // If we're running the systems outside of PlayMode, we should destroy with DestroyImmediate.
            if (m_DrawComponent != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(m_DrawComponent.gameObject);
                else
                    Object.DestroyImmediate(m_DrawComponent.gameObject);
                m_DrawComponent = null;
            }

            for (int i = 0; i < m_DebugStreams.Count; i++)
                m_DebugStreams[i].Dispose();
            m_DebugStreams.Clear();
        }

        internal void ResetDebugStreams()
        {
            // Make sure all potential debug display jobs are finished
            FinalJobHandle.Complete();

            for (int i = 0; i < m_DebugStreams.Count; i++)
            {
                m_DebugStreams[i].Dispose();
            }

            m_DebugStreams.Clear();
        }
    }

// Workaround to avoid "JobTempAlloc has allocations that are more than 4 frames old - this is not allowed and likely a leak"
// warnings.
// The DebugStreams from the previous physics tick are cleared at the beginning of the next tick, in DebugStream.OnUpdate().
// However, if the fixed timestep is sufficiently large (especially if vsync is disabled), there may be several frames rendered
// between each pair of physics ticks, causing the temp allocator to believe the memory is much older than it really is and
// emit a leak warning.
// As a temporary solution, we'll just clear the streams during the initialization system group, so no allocation lasts longer
// than a single rendered frame (as originally intended).
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    partial class DebugStreamClear : SystemBase
    {
        private DebugStream _debugStreamSys;

        protected override void OnCreate()
        {
            _debugStreamSys = World.GetExistingSystem<DebugStream>();
            // Instantiate DebugDisplay on MainTread
            Unity.DebugDisplay.DebugDisplay.Instantiate();
        }

        protected override void OnUpdate()
        {
            // Read/write the display singleton to register as writing data
            if (HasSingleton<PhysicsDebugDisplayData>())
            {
                var data = GetSingleton<PhysicsDebugDisplayData>();
                SetSingleton(data);
            }

#if UNITY_EDITOR
            Unity.DebugDisplay.DebugDisplay.Clear();
#endif
            Dependency.Complete();
            _debugStreamSys.ResetDebugStreams();
        }
    }
}
