using Causeless3t.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.Editor
{
    [CustomEditor(typeof(ReusableScrollView))]
    public sealed class ReusableScrollViewEditor : UnityEditor.Editor
    {
        #region Context Menu

        private const string StandardSpritePath       = "UI/Skin/UISprite.psd";
        private const string BackgroundSpritePath     = "UI/Skin/Background.psd";
        private const string InputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string KnobPath                 = "UI/Skin/Knob.psd";
        private const string CheckmarkPath            = "UI/Skin/Checkmark.psd";
        private const string DropdownArrowPath        = "UI/Skin/DropdownArrow.psd";
        private const string MaskPath                 = "UI/Skin/UIMask.psd";

        private static DefaultControls.Resources _standardResources;
        private static DefaultControls.Resources GetStandardResources()
        {
            if (_standardResources.standard == null)
            {
                _standardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(StandardSpritePath);
                _standardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(BackgroundSpritePath);
                _standardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(InputFieldBackgroundPath);
                _standardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(KnobPath);
                _standardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(CheckmarkPath);
                _standardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(DropdownArrowPath);
                _standardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(MaskPath);
            }
            return _standardResources;
        }
        
        [MenuItem("GameObject/UI/Reusable Scroll View", false)]
        private static void CreateReusableScrollView(MenuCommand menuCommand)
        {
            var scrollviewObject = DefaultControls.CreateScrollView(GetStandardResources());
            scrollviewObject.name = "ReusableScrollView";
            GameObjectUtility.SetParentAndAlign(scrollviewObject, menuCommand.context as GameObject);
            DestroyImmediate(scrollviewObject.GetComponent<ScrollRect>());
            var scrollView = scrollviewObject.AddComponent<ReusableScrollView>();
            scrollView.content = scrollviewObject.transform.Find("Viewport/Content").GetComponent<RectTransform>();
            scrollView.viewport = scrollviewObject.transform.Find("Viewport").GetComponent<RectTransform>();
            scrollView.horizontalScrollbar = scrollviewObject.transform.Find("Scrollbar Horizontal").GetComponent<Scrollbar>();
            scrollView.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollView.horizontalScrollbarSpacing = -3f;
            scrollView.verticalScrollbar = scrollviewObject.transform.Find("Scrollbar Vertical").GetComponent<Scrollbar>();
            scrollView.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollView.verticalScrollbarSpacing = -3f;
            
            Undo.RegisterCreatedObjectUndo(scrollviewObject, "Create " + scrollviewObject.name);
            Selection.activeObject = scrollviewObject;
        }

        #endregion Context Menu
        
        private ReusableScrollView _target;
        private bool _prevVertical, _prevHorizontal;

        private void OnEnable()
        {
            _target = target as ReusableScrollView;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_target.vertical != _prevVertical)
                _target.horizontal = !_target.vertical;
            else if (_target.horizontal != _prevHorizontal)
                _target.vertical = !_target.horizontal;
            _prevHorizontal = _target.horizontal;
            _prevVertical = _target.vertical;
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
