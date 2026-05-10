# Shina

Shina is an interactive novel-style game project developed using the Unity engine, supporting Chinese and other languages. It includes a wide range of features such as auto-reading, chapter management, multi-language support, a QTE (Quick Time Event) system, save/load system, and various UI animations and transition effects.

## Features

- **Multi-language Support**: Provides multi-language translation through Excel files.
- **QTE System**: Supports various QTE types, such as hit QTE, long-press QTE, and double-click QTE.
- **Auto-Reading**: Allows users to automatically play text content.
- **Save and Load**: Supports quick save and load functionality.
- **UI Animations**: Offers multiple UI animations, such as fade-in/fade-out, scaling, and camera shake.
- **Chapter Management**: Supports managing chapters through tags and external files.
- **History Records**: Records player's progress and choices.
- **Floating Cursor**: Supports customizable floating cursor effects.
- **Background Adaptation**: Supports background adaptation to screen aspect ratios.

## Instructions

1. **Start the Game**: Run the `LoadMenu.unity` scene to launch the game main menu.
2. **Chapter Selection**: Load chapter files (e.g., `Chapter0.xlsx`) through the `MainMenu` system.
3. **Auto-Reading**: Click the auto-read button to enable automatic text playback.
4. **Quick Save/Load**: Use `FastSaveButton` and `ReadSaveButton` for saving and loading.
5. **QTE Interaction**: During gameplay, complete QTE events according to the prompts.

## Technical Details

- Uses Unity's `TextMeshPro` for efficient text display.
- Uses `VideoPlayer` to support video playback.
- Uses `ExcelPackage` (such as EPPlus) to process Excel files for loading and saving data.
- Multi-language support is implemented through `LanguageManager` and `TranslationKeys.xlsx`.
- UI animations are implemented using components such as `FadeIn`, `FadeOut`, `CameraShake`, and `ZoomIn`.
- Various in-game tags are managed using `TagManager` and `CollectedTags`.

## Directory Structure

- `Assets/Scenes/` - Contains game scene files (e.g., `Audiobook.unity` and `LoadMenu.unity`).
- `Assets/Script/` - Contains the main logic code.
- `Assets/Resources/` - Contains resource files loaded at runtime.
- `Assets/StreamingAssets/` - Contains chapter data (Excel files), language translation files, and configuration files.
- `Assets/Plugins/TextMesh Pro/` - Contains TextMeshPro resources and shaders.
- `Packages/` - Contains Unity project configurations.
- `ProjectSettings/` - Contains Unity project settings files.

## Developer

- The project is developed and maintained by [MoHanBi](https://gitee.com/MoHanBi).

## License

Please check the `LICENSE` file in the repository for detailed licensing information.