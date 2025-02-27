# RecipeRage

A multiplayer cooking competition game where players compete in various game modes to become the ultimate chef!

## 🎮 Features

- Multiple game modes (Classic, Time Attack, Team Battle)
- Character selection with unique abilities
- In-game shop with skins and power-ups
- Real-time multiplayer using Epic Online Services
- Season Pass and progression system

## 🛠 Tech Stack

- Unity 2022.3 LTS
- C# (.NET Standard 2.1)
- Epic Online Services SDK
- Mirror Networking
- DOTween Pro
- TextMeshPro

## 📋 Prerequisites

- Unity 2022.3 LTS or newer
- Visual Studio 2022 or JetBrains Rider
- Git LFS
- .NET SDK 6.0 or newer

## 🚀 Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/RecipeRage-New.git
git lfs pull
```

2. Open the project in Unity Hub

3. Install required packages:
   - Window > Package Manager
   - Install all dependencies from the manifest

4. Set up Epic Online Services:
   - Create an EOS account
   - Create a new project
   - Add your credentials to `Assets/Settings/EOSConfig.asset`

## 📁 Project Structure

```
Assets/
├── Scripts/           # C# source code
├── Prefabs/          # Reusable game objects
├── Models/           # 3D models and animations
├── Materials/        # Materials and shaders
├── Scenes/           # Unity scenes
├── ScriptableObjects/# Game configuration
└── Documentation/    # Project documentation
```

## 🔧 Development

1. Create a new feature branch:
```bash
git checkout -b feature/your-feature-name
```

2. Make your changes following our [Development Guidelines](Assets/Documentation/DevelopmentGuidelines.md)

3. Run tests:
   - Open Test Runner (Window > General > Test Runner)
   - Run all tests
   - Ensure all tests pass

4. Create a pull request:
   - Push your changes
   - Create a PR against the develop branch
   - Fill out the PR template
   - Request review

## 📚 Documentation

- [Technical Specification](Assets/Documentation/TechnicalSpecification.md)
- [Feature Specification](Assets/Documentation/FeatureSpecification.md)
- [Development Guidelines](Assets/Documentation/DevelopmentGuidelines.md)

## 🧪 Testing

Run tests using the Unity Test Runner:
- Edit > Project Settings > Editor
- Enable "Run in Background"
- Window > General > Test Runner
- Run All Tests

## 📦 Building

1. Set up build settings:
   - File > Build Settings
   - Add necessary scenes
   - Configure player settings

2. Create a build:
   - Select platform
   - Click "Build"
   - Choose output location

## 🔐 Security

- Never commit sensitive data
- Use environment variables for keys
- Follow security guidelines in documentation
- Report vulnerabilities to security@reciperage.com

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 👥 Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📞 Support

For support:
- Check documentation
- Create an issue
- Contact support@reciperage.com

## 🗺 Roadmap

- [ ] Additional game modes
- [ ] More characters and skins
- [ ] Tournament system
- [ ] Social features
- [ ] Cross-platform play

## 🙋‍♂️ FAQ

**Q: How do I set up multiplayer testing?**
A: Follow the local multiplayer testing guide in the documentation.

**Q: Where do I report bugs?**
A: Create an issue on GitHub with the bug template.

## 🎉 Acknowledgments

- Unity Technologies
- Epic Games
- Mirror Networking
- Our amazing community 