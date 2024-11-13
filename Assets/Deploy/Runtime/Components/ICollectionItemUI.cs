
using UnityEngine;

namespace Causeless3t.UI
{
    public interface ICollectionItemUI
    {
        int Index { get; set; }
        RectTransform RootRectTransform { get; }
        void UpdateItem(int index, ICollectionItemData data);
    }

    
    public interface IItemUI
    {  
        void UpdateItem(IItemData data);
    }

    public interface IItemData
    {
    }
    
    public interface ICollectionItemData : IItemData
    {
    }

    public interface IFlexCollectionItemData : ICollectionItemData
    {
        float GetItemSize();
    }
}
