using SM.Physics.Authoring;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SM.Physics.Editor
{
    [CustomEditor(typeof(PhysicsCategoryNames))]
    [CanEditMultipleObjects]
    class PhysicsCategoryNamesEditor : BaseEditor
    {
        #pragma warning disable 649
        [AutoPopulate(ElementFormatString = "Category {0}", Resizable = false, Reorderable = false)]
        ReorderableList m_CategoryNames;
        #pragma warning restore 649
    }
}
