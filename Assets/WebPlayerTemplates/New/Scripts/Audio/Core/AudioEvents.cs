public static class AudioEvents
{
    public static class Game
    {
        public const string Start = "game_start";
        public const string Pause = "game_pause";
        public const string Resume = "game_resume";
        public const string Over = "game_over";
        public const string Victory = "game_victory";
    }

    public static class Kitchen
    {
        public static class Cooking
        {
            public const string Start = "cooking_start";
            public const string Progress = "cooking_progress";
            public const string Complete = "cooking_complete";
            public const string Fail = "cooking_fail";
        }

        public static class Chopping
        {
            public const string Start = "chop_start";
            public const string Progress = "chop_progress";
            public const string Complete = "chop_complete";
        }

        public static class Items
        {
            public const string Pickup = "item_pickup";
            public const string Drop = "item_drop";
            public const string Throw = "item_throw";
            public const string Catch = "item_catch";
        }

        public static class Orders
        {
            public const string New = "order_new";
            public const string Success = "order_success";
            public const string Fail = "order_fail";
            public const string Urgent = "order_urgent";
        }
    }

    public static class Player
    {
        public static class Movement
        {
            public const string Footstep = "player_footstep";
            public const string Dash = "player_dash";
            public const string Collision = "player_collision";
            public const string Slip = "player_slip";
        }

        public static class Actions
        {
            public const string Interact = "player_interact";
            public const string Throw = "player_throw";
            public const string Catch = "player_catch";
            public const string Stun = "player_stun";
        }
    }

    public static class UI
    {
        public const string ButtonClick = "ui_button_click";
        public const string Hover = "ui_hover";
        public const string MenuOpen = "ui_menu_open";
        public const string MenuClose = "ui_menu_close";
        public const string PopupOpen = "ui_popup_open";
        public const string PopupClose = "ui_popup_close";
        public const string Achievement = "ui_achievement";
        public const string Score = "ui_score";
    }

    public static class Ambient
    {
        public const string KitchenAmbience = "ambient_kitchen";
        public const string Fire = "ambient_fire";
        public const string Sizzle = "ambient_sizzle";
        public const string Crowd = "ambient_crowd";
    }
}