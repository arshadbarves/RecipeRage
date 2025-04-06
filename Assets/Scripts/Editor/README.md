# RecipeRage Setup System

This setup system allows you to quickly generate all the necessary assets for the RecipeRage game, including:
- Ingredient scriptable objects
- Recipe scriptable objects
- Game prefabs (stations, player, UI elements)
- Game scene setup

## How to Use

1. Open the RecipeRage Setup Manager from the Unity menu: **RecipeRage > Setup Manager**
2. In the setup window, select which components you want to generate:
   - **Generate Ingredients**: Creates ingredient scriptable objects
   - **Generate Recipes**: Creates recipe scriptable objects (requires ingredients)
   - **Generate Prefabs**: Creates game prefabs for stations, player, and UI elements
   - **Setup Scene**: Creates and sets up the game scene with all necessary objects

3. Click **Generate All** to generate all selected components, or use the individual buttons to generate specific components.

## Generation Sequence

The setup system follows this sequence when generating assets:
1. Creates necessary directories if they don't exist
2. Generates ingredient scriptable objects
3. Generates recipe scriptable objects (using the ingredients)
4. Generates game prefabs
5. Sets up the game scene (using the prefabs)

## Manual Setup Steps

While the setup system automates most of the process, there are a few things you'll need to set up manually:

1. **Input System**: You'll need to set up the input system for player controls.
2. **Network Manager Configuration**: You may need to adjust the NetworkManager settings for your specific needs.
3. **UI Refinement**: The generated UI is functional but basic. You may want to improve its appearance.
4. **TextMeshPro**: For better text rendering, import TextMeshPro from the Package Manager.

## Troubleshooting

If you encounter any issues during setup:

1. **Missing References**: Make sure you generate assets in the correct order (ingredients → recipes → prefabs → scene).
2. **Script Errors**: Ensure all required scripts are in the project and have no compilation errors.
3. **Prefab Issues**: If prefabs don't appear correctly, try regenerating them.
4. **Scene Setup Fails**: Make sure all prefabs have been generated successfully before setting up the scene.

## Extending the Setup System

To add new ingredients, recipes, or game elements:

1. Edit the `IngredientGenerator.cs` file to add new ingredient definitions.
2. Edit the `RecipeGenerator.cs` file to add new recipe definitions.
3. Edit the `PrefabGenerator.cs` file to add new prefab types.
4. Edit the `SceneSetupGenerator.cs` file to modify the scene layout.

## Notes

- The setup system creates simple placeholder visuals. You'll want to replace these with proper art assets.
- The generated scriptable objects use basic properties. You may want to customize them further.
- The scene setup is designed for a basic multiplayer cooking game. Adjust it to fit your specific game design.
