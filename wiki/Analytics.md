# Analytics

## Firebase Analytics Events

| Event | Trigger | Key Params |
|-------|---------|------------|
| match_start | Match begins | map_id, mode, chef_id, trophy_count |
| match_complete | Results screen | won, score, duration_sec, map_id |
| dish_served | Correct dish | recipe_id, time_taken, combo_count |
| iap_completed | Purchase | item_id, usd_value |
| ad_shown | Ad displayed | ad_type, placement |
| daily_streak_claimed | Streak claimed | day_number, reward_type |
| connectivity_lost | Goes offline | context (menu/match) |
| auth_completed | Login done | method, was_guest |

## Implementation

Firebase Analytics + Crashlytics registered in `RootLifetimeScope`:

```csharp
builder.Register<FirebaseAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();
```

## Tracking Rules

- Track match start/end for retention analysis
- Track dish_served for gameplay balance tuning
- Track IAP for revenue analytics
- Track ad_shown for ad performance
- Track connectivity_lost for network quality monitoring
- Track auth_completed for conversion funnel
