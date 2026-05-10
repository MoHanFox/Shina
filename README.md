# 一些想说的话

这个项目横跨了三年，从我的高中到现在，所以代码也挺割裂的，见谅。

# Shina

Shina 是一个基于 Unity 引擎开发的互动小说类游戏项目，支持中文和其他语言。它包含了丰富的功能，如自动阅读、章节管理、多语言支持、QTE（快速反应事件）系统、存读档系统以及各种 UI 动画和过渡效果。

## 特性

- **多语言支持**：通过 Excel 文件提供多语言翻译功能。
- **QTE 系统**：支持多种 QTE 类型，例如击中 QTE、长按 QTE 和双击 QTE。
- **自动阅读**：允许用户自动播放文本内容。
- **存档和读档**：支持快速存档和读档功能。
- **UI 动画**：提供多种 UI 动画，如淡入淡出、缩放和镜头抖动。
- **章节管理**：支持通过标签和外部文件管理章节。
- **历史记录**：记录玩家的游戏进度和选择。
- **浮动光标**：支持自定义浮动光标效果。
- **背景自适应**：支持背景自适应屏幕比例。

## 使用说明

1. **开始游戏**：运行 `LoadMenu.unity` 场景以启动游戏主菜单。
2. **章节选择**：通过 `MainMenu` 系统加载章节文件（例如 `Chapter0.xlsx`）。
3. **自动阅读**：点击自动阅读按钮以启用自动播放文本功能。
4. **快速存档/读档**：使用 `FastSaveButton` 和 `ReadSaveButton` 进行存档和读档。
5. **QTE 互动**：在游戏过程中，根据提示完成 QTE 事件。

## 技术细节

- 使用 Unity 的 `TextMeshPro` 实现高效文本显示。
- 使用 `VideoPlayer` 支持视频播放功能。
- 使用 `ExcelPackage`（如 EPPlus）处理 Excel 文件以加载和保存数据。
- 多语言支持通过 `LanguageManager` 和 `TranslationKeys.xlsx` 实现。
- UI 动画通过 `FadeIn`, `FadeOut`, `CameraShake`, `ZoomIn` 等组件实现。
- 通过 `TagManager` 和 `CollectedTags` 管理游戏内的各种标签。

## 目录结构说明

- `Assets/Scenes/` - 包含游戏场景文件（如 `Audiobook.unity` 和 `LoadMenu.unity`）。
- `Assets/Script/` - 包含主要的逻辑代码。
- `Assets/Resources/` - 包含游戏运行时加载的资源文件。
- `Assets/StreamingAssets/` - 包含章节数据（Excel 文件）、语言翻译文件和设定文件。
- `Assets/Plugins/TextMesh Pro/` - 医用 TextMeshPro 的资源和着色器。
- `Packages/` - 包含 Unity 项目配置。
- `ProjectSettings/` - 包含 Unity 项目设置文件。

## 开发者

- 项目由 [MoHanBi](https://gitee.com/MoHanBi) 开发维护。

## 许可证

请查看仓库中的 `LICENSE` 文件以了解详细授权信息。