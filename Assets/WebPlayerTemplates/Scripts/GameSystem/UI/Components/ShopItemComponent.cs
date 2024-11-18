// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace GameSystem.UI.Components
// {
//     [UxmlElement]
//     public partial class ShopItemComponent : VisualElement
//     {
//         public new class UxmlFactory : UxmlFactory<ShopItemComponent, UxmlTraits> { }
//
//         public new class UxmlTraits : VisualElement.UxmlTraits
//         {
//             private readonly UxmlStringAttributeDescription _mItemName = new UxmlStringAttributeDescription { name = "item-name", defaultValue = "" };
//             private readonly UxmlStringAttributeDescription _mItemType = new UxmlStringAttributeDescription { name = "item-type", defaultValue = "" };
//             private readonly UxmlIntAttributeDescription _mNormalPrice = new UxmlIntAttributeDescription { name = "normal-price", defaultValue = 0 };
//             private readonly UxmlIntAttributeDescription _mDiscountedPrice = new UxmlIntAttributeDescription { name = "discounted-price", defaultValue = 0 };
//             private readonly UxmlIntAttributeDescription _mBonusAmount = new UxmlIntAttributeDescription { name = "bonus-amount", defaultValue = 0 };
//             private readonly UxmlStringAttributeDescription _mCurrencyType = new UxmlStringAttributeDescription { name = "currency-type", defaultValue = "gems" };
//             private readonly UxmlBoolAttributeDescription _mIsFree = new UxmlBoolAttributeDescription { name = "is-free", defaultValue = false };
//             private readonly UxmlStringAttributeDescription _mImagePath = new UxmlStringAttributeDescription { name = "image-path", defaultValue = "" };
//             private readonly UxmlBoolAttributeDescription _mShowNew = new UxmlBoolAttributeDescription { name = "show-new", defaultValue = false };
//
//             public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
//             {
//                 base.Init(ve, bag, cc);
//                 var shopItem = ve as ShopItemComponent;
//
//                 if (shopItem == null) return;
//                 shopItem.ItemName = _mItemName.GetValueFromBag(bag, cc);
//                 shopItem.ItemType = _mItemType.GetValueFromBag(bag, cc);
//                 shopItem.NormalPrice = _mNormalPrice.GetValueFromBag(bag, cc);
//                 shopItem.DiscountedPrice = _mDiscountedPrice.GetValueFromBag(bag, cc);
//                 shopItem.BonusAmount = _mBonusAmount.GetValueFromBag(bag, cc);
//                 shopItem.CurrencyType = _mCurrencyType.GetValueFromBag(bag, cc);
//                 shopItem.IsFree = _mIsFree.GetValueFromBag(bag, cc);
//                 shopItem.ImagePath = _mImagePath.GetValueFromBag(bag, cc);
//                 shopItem.ShowNew = _mShowNew.GetValueFromBag(bag, cc);
//
//                 shopItem.Refresh();
//             }
//         }
//
//         public string ItemName { get; set; }
//         public string ItemType { get; set; }
//         public int NormalPrice { get; set; }
//         public int DiscountedPrice { get; set; }
//         public int BonusAmount { get; set; }
//         public string CurrencyType { get; set; }
//         public bool IsFree { get; set; }
//         public string ImagePath { get; set; }
//         public bool ShowNew { get; set; }
//
//         public ShopItemComponent()
//         {
//             AddToClassList("shop-item");
//             var styleSheet = Resources.Load<StyleSheet>("ShopItemComponentStyles");
//             if (styleSheet != null)
//                 styleSheets.Add(styleSheet);
//         }
//
//         public void Refresh()
//         {
//             Clear();
//
//             if (ShowNew)
//             {
//                 var newLabel = new Label("NEW");
//                 newLabel.AddToClassList("new-label");
//                 Add(newLabel);
//             }
//
//             if (!string.IsNullOrEmpty(ItemType))
//             {
//                 var itemTypeLabel = new Label(ItemType);
//                 itemTypeLabel.AddToClassList("item-type");
//                 Add(itemTypeLabel);
//             }
//
//             if (!string.IsNullOrEmpty(ItemName))
//             {
//                 var itemNameLabel = new Label(ItemName);
//                 itemNameLabel.AddToClassList("item-name");
//                 Add(itemNameLabel);
//             }
//
//             if (!string.IsNullOrEmpty(ImagePath))
//             {
//                 var image = new Image();
//                 image.AddToClassList("item-image");
//                 image.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>(ImagePath));
//                 Add(image);
//             }
//
//             var priceSection = new VisualElement();
//             priceSection.AddToClassList("price-section");
//
//             if (IsFree)
//             {
//                 var freeLabel = new Label("FREE");
//                 freeLabel.AddToClassList("free-label");
//                 priceSection.Add(freeLabel);
//             }
//             else
//             {
//                 var priceLabel = new Label(DiscountedPrice > 0 ? DiscountedPrice.ToString() : NormalPrice.ToString());
//                 priceLabel.AddToClassList("price-label");
//                 priceSection.Add(priceLabel);
//
//                 var currencyIcon = new VisualElement();
//                 currencyIcon.AddToClassList($"currency-icon-{CurrencyType.ToLower()}");
//                 priceSection.Add(currencyIcon);
//
//                 if (DiscountedPrice > 0 && DiscountedPrice < NormalPrice)
//                 {
//                     var normalPriceLabel = new Label(NormalPrice.ToString());
//                     normalPriceLabel.AddToClassList("normal-price");
//                     priceSection.Add(normalPriceLabel);
//                 }
//
//                 if (BonusAmount > 0)
//                 {
//                     var bonusLabel = new Label($"+{BonusAmount}");
//                     bonusLabel.AddToClassList("bonus-label");
//                     priceSection.Add(bonusLabel);
//                 }
//             }
//
//             Add(priceSection);
//         }
//     }
// }
