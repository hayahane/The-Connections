using UnityEditor;
using UnityEngine.UIElements;
using Monologist.KCC;

//[CustomEditor(typeof(KinematicCharacterController))]
public class KRCC_Inspector : Editor
{
    public VisualTreeAsset VisualTreeAsset;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement tree = new VisualElement();
        VisualTreeAsset.CloneTree(tree);
        return tree;
    }
}
