# Maps

## Map Roster

| Map ID | Mode | Mechanic | Difficulty |
|--------|------|----------|------------|
| rookie_kitchen | 2v2 | No hazards | Easy |
| sushi_shuffle | 2v2 | Conveyor belt | Medium |
| burger_boulevard | 2v2 | Crosswalk | Medium |
| pirate_pot | 2v2 | Sliding counters | Hard |
| taco_truck | 3v3 | Dual trucks | Medium |
| space_station | 3v3 | Zero-G throws | Hard |
| volcano_kitchen | 3v3 | Lava vent timers | Hard |
| clash_kitchen | 3v3 | Shared kitchen | Hard |

## Map Rotation (Firebase Remote Config)

| Key | Default |
|-----|---------|
| slot_a_map_id | sushi_shuffle |
| slot_a_mode | quick_2v2 |
| slot_a_rotation_hrs | 6 |
| slot_b_map_id | pirate_pot |
| slot_b_mode | quick_3v3 |
| slot_c_map_id | burger_boulevard |
| slot_c_mode | ranked |
| slot_d_map_id | clash_kitchen |
| slot_d_mode | event |
| slot_d_duration_hrs | 48 |

## Home Screen Map Display

```
<  [SUSHI SHUFFLE -- 2v2 -- Quick Match -- 5h 23m left]  >
o o * o o   (5 dot indicators for event slots)
```
