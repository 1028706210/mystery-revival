# H25D-02：02 世界与地图交付

日期：2026-10-04（America/Los_Angeles）。当前修订：H25D-02-R1。结论：模块源码、附近 E 交互入口与离线检查已交付；Unity 运行、真实按键、物理通行和实际画面均未覆盖。本报告不放行 H25D-03 或 P2。

## 修改内容与原因

在 `modules/02/Assets/Hybrid25D/World/` 新增 `HybridWorldFactory.cs`、`HybridDoor.cs`、`HybridForegroundOccluder.cs` 及各自 `.meta`，实现已批准的 P1 最小空间。三维构件均使用 Unity 内置 Cube，暂不需要 Blender 或外部资源。批量整栋建筑精修停止，本批没有修改主工程、01 模块、公共文档或旧模型。

离线编译脚本、真实程序集接口检查、编译日志与六个模块文件的 SHA-256 位于 `assets/02-world-selfcheck/`。导入 Unity 时只复制上述 World 目录中的三个 `.cs` 及三个 `.meta`；自检目录中的 DLL/EXE 是离线检查产物，不属于 Unity 资产。

## 接口和集成用法

命名空间为 `MysteryRevival.Hybrid25D`。`HybridWorldFactory.Create(Transform parent)` 返回 `GameObject` 世界根 `HybridWorld`；以下对象是直接子对象：

- `ViewCamera`：唯一 Camera 和 AudioListener。
- `Spawn`：脚底出生点。
- `Door`：含 `HybridDoor`，默认关闭，公开 `SetOpen(bool)`、`TryInteract()`、`Transform interactor` 和只读 `IsOpen`。
- `Foreground`：含 `HybridForegroundOccluder`；06 给其 `public Transform target` 赋值。`RefreshVisibility()` 与 `LateUpdate` 使用同一路径，提供只读 `IsHidden` 供验证。

06 的最小组合方式：

```csharp
GameObject world = HybridWorldFactory.Create(parent);
Camera view = world.transform.Find("ViewCamera").GetComponent<Camera>();
Vector3 spawn = world.transform.Find("Spawn").position;
GameObject player = HybridPlayerFactory.Create(parent, view, sprite, spawn);
world.transform.Find("Foreground").GetComponent<HybridForegroundOccluder>().target = player.transform;
HybridDoor door = world.transform.Find("Door").GetComponent<HybridDoor>();
door.interactor = player.transform;
// 门默认关闭；靠近后按 E，或用 door.TryInteract() 走同一距离/状态切换路径。
```

R1 补齐附近 E 交互：`TryInteract()` 没有目标或门脚底原点与角色的 XZ 平面距离超过 1.6 米时返回 false，状态不变；范围内（含边界）调用 `SetOpen(!IsOpen)` 并返回 true。`Update` 只在 `Input.GetKeyDown(KeyCode.E)` 时调用它，持续按住不会逐帧切换。06 注入 interactor，02 不查找全局角色。`SetOpen(bool)` 仍可直接用于行为检查，不受距离限制。门开关以门叶显隐表现，无门叶旋转动画；门框和墙楣始终保留。

世界根归调用方持有；重建前销毁旧世界根和旧玩家，并等待 Unity 销毁完成，再创建新实例。没有全局单例、静态对象或新建材质。所有 Camera/Listener/Light 都挂在世界根下，颜色使用默认 Cube 材质及 MaterialPropertyBlock；工厂不改全局 RenderSettings。

## 尺寸、原点和空间

下面坐标均相对于世界根，要求父节点缩放为 `(1,1,1)`。XZ 为地面，Y 向上，1 单位暂按 1 米；这些是本次验证参数。

| 构件 | 中心位置 | 尺寸 / 说明 |
| --- | --- | --- |
| Ground | `(0,-0.1,0)` | `12 × 0.2 × 10`；顶面 Y=0，有 BoxCollider |
| WallLeft / WallRight | `(-2.6,1.5,1.5)` / `(2.6,1.5,1.5)` | 同一 `4 × 3 × 0.2` 墙件由同一方法实例化两次；内边分别 X=-0.6 / +0.6 |
| Door | 脚底原点 `(0,0,1.5)` | 净开口宽 1.2、高 2.2；门叶 `1.2 × 2.2 × 0.18`，中心相对门根 `(0,1.1,0)` |
| Door frame | 相对门根 | 左右柱宽 0.15、柱高 2.2；柱内边在 X=±0.6；上框底面 Y=2.2，墙楣补至 Y=3 |
| Foreground | `(2,1.25,-1.5)` | `2 × 2.5 × 0.2`，左右均可绕行；隐藏只改变 Renderer，BoxCollider 保留 |
| Spawn | `(0,0,-3)` | 脚底点，地面顶面 |

镜头固定为正交，Unity 欧拉角 `(35,45,0)`、orthographicSize=6，观察点 `(0,0.6,0)`，沿镜头后方距离 14。近/远裁剪 0.1/100。世界根另持有一盏方向光。W/D 的镜头相对移动由 01 实现。

遮挡使用相机 `WorldToViewportPoint` → `ViewportPointToRay`，射线终点为玩家脚底上方 0.85 米；只有射线在终点之前命中此处前景 BoxCollider 时隐藏该墙。正交射线从相应视口位置出发，避免用相机中心射线造成侧向判断错误。目标为空、离开遮挡线或组件禁用时恢复原显示状态。

## 自检结果与原始证据

| 检查 | 结论 | 实际证据 |
| --- | --- | --- |
| 三份源码对真实 Unity 程序集编译 | 通过 | 本机 Mono/mcs，C# 7.2、警告视为错误；最终 `COMPILE_EXIT=0` |
| 公开模块接口元数据 | 通过 | 读取真实生成 DLL，不实例化 Unity 对象；R1 `CONTRACT_PASS checks=10`、`CONTRACT_EXIT=0`，包含 interactor 和 TryInteract |
| 文件与 meta 身份 | 通过 | 三个 `.cs`、三个对应 `.meta`；三个 GUID 唯一；SHA 清单见 `delivery-manifest.json` |
| 尺寸、交互与状态切换代码自查 | 通过（静态范围） | 门内边 ±0.6、上框底 2.2；XZ 范围判断与 GetKeyDown 都汇入 TryInteract/SetOpen；同一门状态写入 Renderer/Collider；前景显隐不改碰撞；不是运行证据 |
| S13 精修文件保护 | 通过（该文件范围） | 前后 SHA-256 相同，见清单；主工程完整基线保护由 06 另做 |
| Unity Editor/PlayMode/距离边界/真实 E/物理与三次重建 | 未覆盖 | 按本任务限制没有启动 Unity |
| 1366×768 / 1920×1080 实际画面 | 未覆盖 | 没有渲染截图；不能从离线编译推定视觉遮挡正确 |
| 独立审查、阶段放行 | 未覆盖 | 等待 00/06 按前置和任务包处理 |

重跑命令（工作目录为 C 工作区）：

```powershell
& 'prototypes\hybrid25d-20261004\assets\02-world-selfcheck\Compile-World.ps1'
```

编译用的是 `D:\Unity\Editors\6000.5.10f1\Editor\Data\MonoBleedingEdge` 中的 Mono/mcs，R1 引用同版真实 CoreModule、PhysicsModule、AudioModule、InputLegacyModule 和 .NET Standard 2.1 参考程序集。首批编译脚本默认 mscorlib 与 netstandard 重复定义，退出码 1（30 个 CS1685）；增加 `-nostdlib+` 后编译退出码 0，初次日志保留在 `compile-initial.log`。首批 8 项通过日志为 `compile-h25d02.log`；R1 新增接口检查先对旧实现运行，因缺少 interactor 预期失败，退出码 1，保留在 `compile-r1-red.log`。R1 最终完整结果在 `compile.log`：模块编译、检查器编译及 10 项接口检查均退出码 0。这些工具调用没有启动 Unity，元数据检查不能证明实际距离和按键行为。

## 给 06 的运行检查点

在身份父节点下创建世界，物理定位后按需要同步 Transform：

1. 关闭门，玩家从 `(0,0,0.5)` 向 `(0,0,2.5)` 移动应被门叶阻挡；打开后可通行。多次切换，门框 Renderer/Collider 始终存在。
2. 左/右墙均阻挡玩家；不能以 Renderer 显示推定物理通过。
3. target 在 `(2.4,0,-0.9)` 时，前景应挡住玩家中心射线并隐藏；在 `(0,0,-3)` 或 `(4,0,-0.9)` 时恢复。隐藏时前景仍阻挡通行；清空 target 或禁用组件应恢复显示。以上是供运行验证的预期，尚未执行。
4. 调用实际玩家 Step 检查门洞通行，观察脚底、门口和遮挡的真实画面。
5. 销毁旧世界/玩家后等待一帧，再创建；重复三次，检查 Player、Camera、AudioListener 各一份。
6. 给门 interactor 注入玩家后验证 TryInteract：空目标、XZ 超距 1.61 米应返回 false 且状态不变；距离 1.6 米及范围内应返回 true 并切换，Y 高差不参与距离。反复切换仍同步门叶 Renderer/BoxCollider，门框不变。
7. 实际按下 E 只切换一次；持续按住不再次切换，松开再按可以切换；超距或 interactor 为空时 E 不改门。此项真实输入仍由 06 处理，不使用桌面键鼠注入。

## 剩余问题、风险与成本

当前无已确认的接口冲突；视觉颜色、像素角色和三维墙体风格需要实际画面评估。单射线是 P1 的最小显隐方案，画面边缘是否有切换突兀留给 06 验证，不扩展透明 Shader。00 仅需按正常流程核对本报告与模块接口，H25D-03 是否派发仍由 00 决定。

本轮首个可核对时钟为 `2026-10-05 03:19:58 UTC`（用户时区 2026-10-04 20:19:58），报告收尾时钟为 `2026-10-05 03:32:26 UTC`；记录区间 12 分 28 秒，包含秒级工具等待。计时前的任务包/规格读取约 3 分钟为估算；本轮有效工作约 16 分钟估算，60 分钟首轮余量约 44 分钟估算，没有等待外部审批或运行结果。未伪称逐秒工时记录；历史投入未知，不重置历史额度。模块与报告在本轮交付后停止扩展，等待正常审计。
