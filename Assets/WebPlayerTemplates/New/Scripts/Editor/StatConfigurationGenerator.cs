using Gameplay.Character.Stats;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class StatConfigurationGenerator : EditorWindow
    {
        [MenuItem("Game/Generate Character Stats Configurations")]
        public static void GenerateStatConfigurations()
        {
            StatConfigurationData config = CreateInstance<StatConfigurationData>();

            config.characterClasses = new[] {
                // Flame Master (Offensive Mage)
                new StatConfigurationData.CharacterClassStats {
                    className = "FlameMaster",
                    stats = new[] {
                        new StatConfigurationData.StatDefinition {
                            type = StatType.Health,
                            baseValue = 100f,
                            minValue = 0f,
                            maxValue = 200f,
                            description = "Base health for Flame Master"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.AttackPower,
                            baseValue = 65f,
                            minValue = 0f,
                            maxValue = 100f,
                            description = "High attack power for offensive spells"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.MovementSpeed,
                            baseValue = 5f,
                            minValue = 3f,
                            maxValue = 8f,
                            description = "Standard movement speed"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.CookingSpeed,
                            baseValue = 1f,
                            minValue = 0.5f,
                            maxValue = 2f,
                            description = "Base cooking speed"
                        }
                    }
                },

                // Sous Ninja (Speedy Assassin)
                new StatConfigurationData.CharacterClassStats {
                    className = "SousNinja",
                    stats = new[] {
                        new StatConfigurationData.StatDefinition {
                            type = StatType.Health,
                            baseValue = 90f,
                            minValue = 0f,
                            maxValue = 180f,
                            description = "Lower health for assassin class"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.MovementSpeed,
                            baseValue = 7f,
                            minValue = 5f,
                            maxValue = 10f,
                            description = "High movement speed for mobility"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.AttackPower,
                            baseValue = 55f,
                            minValue = 0f,
                            maxValue = 90f,
                            description = "Moderate attack power"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.CriticalChance,
                            baseValue = 0.15f,
                            minValue = 0f,
                            maxValue = 0.5f,
                            isPercentage = true,
                            description = "High critical strike chance"
                        }
                    }
                },

                // Garden Guru (Support/Healer)
                new StatConfigurationData.CharacterClassStats {
                    className = "GardenGuru",
                    stats = new[] {
                        new StatConfigurationData.StatDefinition {
                            type = StatType.Health,
                            baseValue = 110f,
                            minValue = 0f,
                            maxValue = 220f,
                            description = "Above average health for support"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.ResourceRegenRate,
                            baseValue = 2f,
                            minValue = 1f,
                            maxValue = 4f,
                            description = "High resource regeneration for healing"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.BuffDuration,
                            baseValue = 1.3f,
                            minValue = 1f,
                            maxValue = 2f,
                            description = "Increased buff duration"
                        }
                    }
                },

                // Metal Mixer (Tank/Defender)
                new StatConfigurationData.CharacterClassStats {
                    className = "MetalMixer",
                    stats = new[] {
                        new StatConfigurationData.StatDefinition {
                            type = StatType.Health,
                            baseValue = 130f,
                            minValue = 0f,
                            maxValue = 260f,
                            description = "High health for tank role"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.Defense,
                            baseValue = 40f,
                            minValue = 20f,
                            maxValue = 80f,
                            description = "High defense for survivability"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.MovementSpeed,
                            baseValue = 4f,
                            minValue = 3f,
                            maxValue = 6f,
                            description = "Slower movement speed"
                        }
                    }
                },

                // Spice Sorcerer (Debuffer)
                new StatConfigurationData.CharacterClassStats {
                    className = "SpiceSorcerer",
                    stats = new[] {
                        new StatConfigurationData.StatDefinition {
                            type = StatType.Health,
                            baseValue = 95f,
                            minValue = 0f,
                            maxValue = 190f,
                            description = "Below average health"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.CooldownReduction,
                            baseValue = 0.15f,
                            minValue = 0f,
                            maxValue = 0.4f,
                            isPercentage = true,
                            description = "Cooldown reduction for frequent ability use"
                        },
                        new StatConfigurationData.StatDefinition {
                            type = StatType.DebuffResistance,
                            baseValue = 0.2f,
                            minValue = 0f,
                            maxValue = 0.5f,
                            isPercentage = true,
                            description = "Resistance to debuffs"
                        }
                    }
                }
            };

            // Save the configuration asset
            AssetDatabase.CreateAsset(config, "Assets/ScriptableObjects/Stats/DefaultCharacterStats.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Generated character stat configurations!");
        }
    }
}