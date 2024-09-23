// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace GameSystem.UI.Components
// {
//     [UxmlElement]
//     public partial class ShopSectionComponent : VisualElement
//     {
//         public new class UxmlFactory : UxmlFactory<ShopSectionComponent, UxmlTraits> { }
//
//         public new class UxmlTraits : VisualElement.UxmlTraits
//         {
//             [UxmlAttribute]
//             public string SectionName { get; set; }
//             [UxmlAttribute]
//             public string RefreshTime { get; set; }
//             [UxmlAttribute]
//             public bool ShowNew { get; set; }
//             [UxmlAttribute]
//             public bool ShowFree { get; set; }
//             
//             public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
//             {
//                 base.Init(ve, bag, cc);
//                 var section = ve as ShopSectionComponent;
//
//                 if (section == null) return;
//                 section.SectionName = _mSectionName.GetValueFromBag(bag, cc);
//                 section.RefreshTime = _mRefreshTime.GetValueFromBag(bag, cc);
//                 section.ShowNew = _mShowNew.GetValueFromBag(bag, cc);
//                 section.ShowFree = _mShowFree.GetValueFromBag(bag, cc);
//
//                 section.Refresh();
//             }
//         }
//
//         public string SectionName { get; set; }
//         public string RefreshTime { get; set; }
//         public bool ShowNew { get; set; }
//         public bool ShowFree { get; set; }
//
//         private VisualElement _mItemsContainer;
//
//         public ShopSectionComponent()
//         {
//             AddToClassList("shop-section");
//             var styleSheet = Resources.Load<StyleSheet>("ShopSectionComponentStyles");
//             if (styleSheet != null)
//                 styleSheets.Add(styleSheet);
//         }
//
//         public void Refresh()
//         {
//             Clear();
//
//             var header = new VisualElement();
//             header.AddToClassList("section-header");
//
//             var nameLabel = new Label(SectionName);
//             nameLabel.AddToClassList("section-name");
//             header.Add(nameLabel);
//
//             var timeLabel = new Label(RefreshTime);
//             timeLabel.AddToClassList("refresh-time");
//             header.Add(timeLabel);
//
//             if (ShowNew)
//             {
//                 var newLabel = new Label("NEW");
//                 newLabel.AddToClassList("new-label");
//                 header.Add(newLabel);
//             }
//
//             if (ShowFree)
//             {
//                 var freeLabel = new Label("FREE");
//                 freeLabel.AddToClassList("free-label");
//                 header.Add(freeLabel);
//             }
//
//             Add(header);
//
//             _mItemsContainer = new VisualElement();
//             _mItemsContainer.AddToClassList("items-container");
//             Add(_mItemsContainer);
//         }
//
//         public void AddItem(ShopItemComponent item)
//         {
//             _mItemsContainer.Add(item);
//         }
//     }
// }