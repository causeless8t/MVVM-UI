using System;
using TMPro;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class UIRegisterAttribute : Attribute
    { 
        public string Key;
        public abstract Type DelegateType { get; }
        /// <summary>
        /// 키 등록 안하면 자동으로 함수명으로 등록됨
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="actionType">버튼이벤트면 Action<Button> 처럼 입력 </param>
        public UIRegisterAttribute(string Key)
        { 
            this.Key = Key;
        }  
        public UIRegisterAttribute()
        { 
 
        }
    }

    public class ButtonClickEventRegisterAttribute : UIRegisterAttribute
    {
        public ButtonClickEventRegisterAttribute(string Key) : base(Key)
        {
        } 
        public ButtonClickEventRegisterAttribute()
        {
        }

        public override Type DelegateType => typeof(Action<Button>); 
    }
    
    public class ToggleValueChangedEventRegisterAttribute : UIRegisterAttribute
    {
        public ToggleValueChangedEventRegisterAttribute(string Key) : base(Key)
        {
        } 
        public ToggleValueChangedEventRegisterAttribute()
        {
        }

        public override Type DelegateType => typeof(Action<Toggle, bool>); 
    }
    
    public class LongTapButtonClickEventRegisterAttribute : UIRegisterAttribute
    {
        public LongTapButtonClickEventRegisterAttribute(string Key) : base(Key)
        {
        } 
        public LongTapButtonClickEventRegisterAttribute()
        {
        }

        public override Type DelegateType => typeof(Action<LongTapButton>); 
    }
    
    public class DropdownValueChangedEventRegisterAttribute : UIRegisterAttribute
    {
        public DropdownValueChangedEventRegisterAttribute(string Key) : base(Key)
        {
        } 
        public DropdownValueChangedEventRegisterAttribute()
        {
        }

        public override Type DelegateType => typeof(Action<TMP_Dropdown, int>); 
    }
    
    public class SliderValueChangedEventRegisterAttribute : UIRegisterAttribute
    {
        public SliderValueChangedEventRegisterAttribute(string Key) : base(Key)
        {
        } 
        public SliderValueChangedEventRegisterAttribute()
        {
        }

        public override Type DelegateType => typeof(Action<Slider, float>); 
    }
    
    public class InputFieldEventRegisterAttribute : UIRegisterAttribute
    {
        public InputFieldEventRegisterAttribute(string Key) : base(Key)
        {
        } 
        public InputFieldEventRegisterAttribute()
        {
        }

        public override Type DelegateType => typeof(Action<TMP_InputField, string>); 
    }
}