# CrosshairPro.Core 数据模型

## 模型关系图

```
┌─────────────────────────────────────────────────────┐
│                   CrosshairConfig                     │
│  - Id: string                                        │
│  - Name: string                                      │
│  - Style: CrosshairStyle                             │
│  - Size, Gap, Thickness, Color, Opacity...         │
│  - Effects: EffectsConfig                            │
│  - Display: DisplayConfig                            │
└───────────────────┬─────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
┌───────────────┐       ┌───────────────┐
│ EffectsConfig │       │ DisplayConfig │
│ - Outline     │       │ - Monitor     │
│ - Shadow      │       │ - ClickThrough│
│ - Glow        │       │ - AlwaysOnTop │
└───────┬───────┘       └───────────────┘
        │
        ├────────────────┬────────────────┐
        ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│OutlineConfig│  │ShadowConfig │  │ GlowConfig  │
│- Enabled    │  │- Enabled    │  │- Enabled    │
│- Color      │  │- Color      │  │- Color      │
│- Thickness  │  │- Blur       │  │- Intensity  │
└─────────────┘  │- OffsetX/Y  │  │- Range      │
                 └─────────────┘  └─────────────┘

┌─────────────────────────────────────────────────────┐
│                       Preset                          │
│  - Id: string                                        │
│  - Name: string                                      │
│  - Config: CrosshairConfig                           │
│  - GameAssociation: string?                          │
│  - HotkeyBinding: string?                            │
│  - CreatedAt, UpdatedAt: DateTime                    │
│  - IsDefault: bool                                   │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                 AppPersistedState                     │
│  - CurrentPresetId: string?                          │
│  - LastUsedConfig: CrosshairConfig?                  │
│  - AutoStart: bool                                   │
│  - MinimizeToTray: bool                              │
└─────────────────────────────────────────────────────┘
```

## 核心数据模型

### 1. CrosshairConfig - 准心配置

**职责**: 定义准心的所有可配置属性

**属性**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | string | GUID | 唯一标识符 |
| Name | string | "默认配置" | 配置名称 |
| Style | CrosshairStyle | Cross | 准心样式 |
| Size | int | 20 | 准心大小（像素） |
| Gap | int | 4 | 间隙大小（像素） |
| Thickness | int | 2 | 线条厚度（像素） |
| Color | string | "#00FF00" | 颜色（十六进制） |
| Opacity | int | 100 | 不透明度（0-100） |
| Brightness | int | 100 | 亮度（0-100） |
| CenterSize | int | 4 | 中心点大小（像素） |
| Rotation | int | 0 | 旋转角度（度） |
| CustomImagePath | string? | null | 自定义图片路径 |
| Effects | EffectsConfig | new() | 效果配置 |
| Display | DisplayConfig | new() | 显示配置 |

**方法**:
- `Clone()`: 创建深拷贝，生成新ID
- `CopyFrom(other)`: 复制其他配置的值（保持ID不变）

### 2. EffectsConfig - 效果配置

**职责**: 管理准心的视觉效果

**属性**:

| 属性 | 类型 | 说明 |
|------|------|------|
| Outline | OutlineConfig | 描边效果 |
| Shadow | ShadowConfig | 阴影效果 |
| Glow | GlowConfig | 发光效果 |

### 3. OutlineConfig - 描边效果

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | true | 是否启用 |
| Color | string | "#000000" | 描边颜色 |
| Thickness | int | 1 | 描边厚度 |

### 4. ShadowConfig - 阴影效果

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#000000" | 阴影颜色 |
| Blur | int | 3 | 模糊半径 |
| OffsetX | int | 0 | X偏移 |
| OffsetY | int | 2 | Y偏移 |

### 5. GlowConfig - 发光效果

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#00FFFF" | 发光颜色 |
| Intensity | int | 50 | 发光强度 |
| Range | int | 10 | 发光范围 |

### 6. DisplayConfig - 显示配置

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Monitor | string | "primary" | 显示器标识 |
| ClickThrough | bool | true | 鼠标穿透 |
| AlwaysOnTop | bool | true | 始终置顶 |
| PositionX | int | 0 | X位置 |
| PositionY | int | 0 | Y位置 |

### 7. Preset - 预设

**职责**: 保存完整的准心配置快照，支持游戏关联

**属性**:

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | 预设ID |
| Name | string | 预设名称 |
| Config | CrosshairConfig | 准心配置 |
| GameAssociation | string? | 关联的游戏进程名 |
| HotkeyBinding | string? | 热键绑定ID |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime | 更新时间 |
| IsDefault | bool | 是否为默认预设 |

**方法**:
- `Clone()`: 创建深拷贝，标记为非默认预设

### 8. AppPersistedState - 应用持久化状态

**职责**: 记录应用运行时状态，下次启动时恢复

**属性**:

| 属性 | 类型 | 说明 |
|------|------|------|
| CurrentPresetId | string? | 当前使用的预设ID |
| LastUsedConfig | CrosshairConfig? | 最后使用的配置 |
| AutoStart | bool | 是否开机自启 |
| MinimizeToTray | bool | 是否最小化到托盘 |

### 9. HotkeyBinding - 热键绑定

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | 绑定ID |
| Key | string | 按键 |
| Modifiers | string[] | 修饰键（Ctrl/Alt/Shift） |
| Action | string | 动作类型 |

### 10. GameInfo - 游戏信息

| 属性 | 类型 | 说明 |
|------|------|------|
| ProcessName | string | 进程名 |
| DisplayName | string | 显示名称 |
| Icon | string? | 图标路径 |

## 枚举类型

### CrosshairStyle - 准心样式

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Cross | 十字准心 |
| 1 | Dot | 点状准心 |
| 2 | Circle | 圆形准心 |
| 3 | TShape | T形准心 |
| 4 | XShape | X形准心 |
| 5 | CustomImage | 自定义图片 |

### AppState - 应用状态

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Idle | 空闲状态 |
| 1 | GameMode | 游戏模式 |
| 2 | CrosshairVisible | 准心显示中 |
| 3 | CrosshairHidden | 准心隐藏 |

## 数据验证规则

1. **Size**: 5-100 像素
2. **Gap**: 0-50 像素
3. **Thickness**: 1-10 像素
4. **Opacity**: 0-100 百分比
5. **Brightness**: 0-100 百分比
6. **CenterSize**: 0-20 像素
7. **Rotation**: 0-360 度
8. **Color**: 十六进制颜色值 (#RRGGBB)