using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Friends via Unity Gaming Services. Friend code display + add-by-code +
    /// online/offline lists + invite buttons.
    /// </summary>
    [UIScreen]
    public sealed class FriendsScreen : BaseUIScreen
    {
        protected override async void OnShow()
        {
            var friends = ServiceLocator.Get<IFriendsService>();

            Root.Q<Label>("my-code").text = $"My Code: {friends.MyFriendCode}";
            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();

            var addField = Root.Q<TextField>("friend-code-input");
            Root.Q<Button>("add-button").clicked += async () =>
            {
                var added = await friends.AddFriendByCode(addField.value);
                if (added)
                {
                    RefreshList(friends);
                }
            };

            RefreshList(friends);
        }

        private async void RefreshList(IFriendsService friends)
        {
            var list = Root.Q<ScrollView>("friends-list");
            list.Clear();

            var all = await friends.GetFriends();
            foreach (var friend in all)
            {
                var row = new VisualElement();
                row.AddToClassList("friend-row");
                row.Add(new Label($"{friend.DisplayName} — {friend.Presence}"));

                var invite = new Button(() => friends.InviteFriend(friend.FriendId)) { text = "Invite" };
                row.Add(invite);
                list.Add(row);
            }
        }
    }
}
