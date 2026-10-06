# H25D-00：06 基线冻结与独立验证工程

修改内容：

- 当前任务为 H25D-00，角色为 06 测试与集成。已建立 `../Validation/Game` 独立 Unity 源工程，并同时封存 `../Validation/tests`、`../Validation/tools`。
- 从 `D:\projects\神秘复苏` 白名单复制 `Game/Assets`、`Game/Packages`、`Game/ProjectSettings`、`tests`、`tools` 的全部文件，含隐藏文件及 `.meta`。共 **62 文件、236,766 字节**：Game 56、tests 1、tools 5。
- 源文件及 SHA256 逐项记录在 [baseline.json](baseline.json) 和 [source-inventory-before.json](source-inventory-before.json)。清单整体 SHA256 为 `2962D6827FAB8D12C2F40DD7E721506084D584CAA79C72E9CF0BBCF09E98A541`；摘要算法与相对路径均已记录。
- 副本唯一源差异为两个 Packages 文件：manifest 新增 `com.unity.modules.physics: 1.0.0`；lock 新增相同版本、`source=builtin`、`depth=0`、空 dependencies 的记录。原 audio/imgui/screencapture 及间接 imageconversion 完整保留。
- 没有复制 `Library`、`Library.*`（包括旧 stuck 目录）、`Temp`、`Logs`、`UserSettings` 或内部审计资料。没有导入 01/02 模块。
- 主工程、旧模型与 BLD 候选保持只读；当前 **无产品同步授权**。00 唯一维护公共状态；H25D-03 尚未放行。

修改原因：

本副本为三维 XZ 地面、二维像素角色和固定斜俯视镜头提供可复核的源基线。独立副本与原样 tests/tools 便于后续核对规则保留和模块差异；加入内置 Physics 是 CharacterController 方案的已授权前置。

验证结果：

| 检查项 | 当前证据 | 结果 |
|---|---|---|
| 写入范围与冲突 | 新原型根目录最初不存在；目标 Game/tests/tools 和本任务报告无旧文件；其他模块目录未写入 | 通过 |
| 主源冻结 | 62 项完整清单；复制前后、最终验证均未发现源文件集合或 SHA 变化 | 通过 |
| 副本字节核对 | 配置调整前 62/62 与源相同；调整后 60 项相同，差异严格为两个批准 Packages 文件 | 通过 |
| Unity 版本 | `6000.5.10f1 (3bd4f66ad299)`；ProjectVersion 字节与源一致 | 通过 |
| 渲染与依赖配置 | GraphicsSettings 与原配置字节一致；所有原依赖条目保持 | 通过 |
| 内置 Physics 定义 | 锁定 Editor 的 `Data/Resources/PackageManager/BuiltInPackages/com.unity.modules.physics/package.json` 声明 1.0.0、module、空依赖；路径和 SHA 已记入验证证据 | 通过（本地包定义与副本配置） |
| 缓存及审计隔离 | 副本根目录与目录集合实查，排除项不存在 | 通过 |
| 并发与运行态 | 前检 Unity 相关进程 0；本任务没有启动 Unity，最终验证仍为 0；没有 Unity 自身运行态写入 | 通过（前检范围） |
| Git 可追溯性 | C、D 都没有有效 HEAD，源文件未跟踪；Git 有用户 ignore 读取警告 | 未覆盖（Git 版本基线）；使用全量 SHA 替代 |
| Unity 导入、编译、Physics 行为、画面 | 本轮没有执行 | 未覆盖 |

实际命令及返回：

```powershell
& .\prototypes\hybrid25d-20261004\reports\freeze-baseline.ps1
# exit 0; SNAPSHOT_PASS files=62 bytes=236766

& .\prototypes\hybrid25d-20261004\reports\verify-baseline.ps1
# exit 0; BASELINE_VERIFY=PASS checks=20 failed=0 source=62 unchangedCopies=60 allowedConfigDeltas=2
```

本轮使用原生 PowerShell LiteralPath 复制及 SHA256 核对。原始证据为两个清单 JSON、[baseline-verification-final.json](baseline-verification-final.json)、[verify-baseline.stdout.txt](verify-baseline.stdout.txt) 和保留的执行脚本。首轮验证也保留在 `baseline-verification.json`；首次汇总的 DateTime 时区转换造成工时计算错误，已以 ISO offset 和 DateTimeOffset 修正，再执行完整 20 项核对。错误计算未作为预算或放行依据。

旧 BLD-13–15 只读登记：

- 原目录为 `building_samples/corridor-wall-integration-20261003`。当前读取最终 Editor/newEXE 原始 result、日志及 run.json：两者各 **949 条 SMOKE_PASS**，均为 `SMOKE_RESULT=PASS checks=949`，原记录退出码均为 0。
- `review-final.md` 为限定静态审查通过，未发现候选阻断问题。Editor 原日志仍有一次 Search `ArgumentOutOfRangeException`，随后完成冒烟；该异常披露保留。
- 这些是历史证据核对，本次没有重新执行。较早 `06-report.md` 中的未覆盖记录不替代最终原始结果。
- 候选未同步主工程、未补旧 ZIP；报告/原始日志的路径与 SHA 已放入 baseline.json。旧模型和候选文件未写入。

H25D-03 检查清单（准备完毕，执行未放行）：

| 项目 | 后续检查及所需证据 | 当前状态 |
|---|---|---|
| 模块接口与导入身份 | 00 核对 01/02 报告；`MysteryRevival.Hybrid25D`；按源 SHA 导入并保留 meta/GUID | 未覆盖，待派发 |
| 入口组合 | 01 `HybridPlayerFactory.Create(Transform, Camera, Sprite, Vector3)`；02 `HybridWorldFactory.Create(Transform)` 的 `ViewCamera`、`Spawn`、`Door`；06 专用 `Hybrid25D.unity` 与自己的 Editor 入口 | 未覆盖，待派发 |
| 同路径运动 | `Update` 与 `Step(Vector2,float)` 同一路径；W 向屏幕内上方、D 向右；直行/斜行等时距离误差 ≤1%；暂停/停止输入无移动 | 未覆盖，待派发 |
| 三维碰撞 | CharacterController 保持竖直；墙不可穿；门净宽 1.2m、候选高 2.2m；关闭阻挡、打开可通过；反复开关与门框保留 | 未覆盖，待派发 |
| 镜头与角色比例 | 正交镜头候选俯角35°/水平45°/size6；角色候选可见高1.7m；Sprite 视觉子节点面向镜头、脚底稳定 | 未覆盖，候选参数待画面核对 |
| 遮挡 | 02 组件 `public Transform target` 由06赋值；实际绕行时前景显隐/恢复，不能只由对象启用状态宣告可见性通过 | 未覆盖，待派发 |
| 生命周期 | 销毁重建3次后 Player/Camera/AudioListener 各1份；没有重复根对象与残留组件 | 未覆盖，待派发 |
| 画面证据 | 在1366×768、1920×1080实际输出尺寸核对脚底、人物辨识、门口、遮挡切换及恢复；只有无图形结果时标视觉未覆盖 | 未覆盖，待派发 |
| Windows演示 | H25D-03 放行后独立构建与后台验证；静态Idle不等于Walk/八方向动作；输出版本、命令、退出码和日志 | 未覆盖，待派发 |
| 主源与旧规则保护 | 导入与运行前后重新核对62项主源；旧调查/Domain 保留，不调用会改写旧Prototype场景的 Build 方法 | 未覆盖，待派发 |

未覆盖项：

尚未运行 Unity、核心测试、模块集成、Windows构建或视觉验证；未确认 Physics 在实际导入后可用。H25D-00 只完成隔离准备，不能据此判定 P1、T003 或玩法体验通过。

风险：

独立副本保留了旧 Prototype 场景与 Editor 入口，后续集成须创建专用入口并核对生成文件范围。tests/tools 为原样封存，其迁移工具未执行；后续使用前须核对输出路径。源仓库没有有效 HEAD，后续每批必须继续使用文件清单/SHA守卫。

实际成本：

- 客户端日期/时区：2026-10-04，America/Los_Angeles。
- 快照操作开始：2026-10-04 20:21:10 -07:00；最终核对：20:24:37 -07:00。UTC 精确时间见 baseline.json。
- 含前检、快照、验证和报告，本轮有效工时按约 **12分钟**记账；等待为0；本轮30分钟上限尚余约 **18分钟**。这是本轮额度，历史累计投入未知，不归零。

是否需要总控介入：

H25D-00 可交 00 审阅。01/02 交付后由 00 核对接口，并明确派发 H25D-03；本会话本轮到此结束。

实际能力：PowerShell 源文件复制/清单/SHA256/JSON及只读 Git 查询；apply_patch 编辑副本 Packages 与自身报告；`verification-before-completion` Skill 要求的完整证据核对。本轮没有桌面操作或外部下载。
