# T003 外部修复报告（任务 A：M0a 技术收尾）· 局部完成，待独立验收

日期：2026-09-16 至 2026-09-17。执行者：外部 Agent（WorkBuddy / GLM）。
工作目录：D:\projects\神秘复苏（Unity 工程 Game/）。C 盘原件未做任何写操作。
状态：**局部完成，待独立验收**。本报告不宣布 T003 或 M0 通过；Editor Play Mode 与 Windows 构建在受控环境下未能完整复跑，明确列为未覆盖。

## 1. 接管记录（只读核对，全部通过）

| 检查项 | 实际操作/命令 | 结果 | 证据 |
|---|---|---|---|
| Git 状态 | `git status` / `git log` | master 无任何提交，全部 untracked；未做 git 写操作 | — |
| Unity 完整版本 | Unity.exe VersionInfo | 6000.5.10f1_3bd4f66ad299（File 6000.5.10.3921142），与 ProjectVersion.txt 一致 | artifacts/external/T003/version-cd-diff.txt |
| 关键文件哈希 | Get-FileHash ×6 | 已登记（IncidentState/PrototypeGame/PrototypeSmoke/PrototypeBuild/Tests/Test-Core） | artifacts/external/T003/handoff-hashes.txt |
| C/D 差异 | 6 文件 SHA256 只读对照 | 全部一致；C 盘原件未动 | 同上 |
| 现有进程 | Get-Process | 接管时无 Unity 进程占用 | artifacts/external/T003/env-check.txt |
| Library | 目录检查 | Game\Library 缺失（D 盘项目未在本机开过 Editor）；后从 C 盘只读复制缓存 49MB，损坏备份保留于 Game/Library.stuck-203207 | artifacts/external/T003/library-probe.txt、library-copy-note.txt |

## 2. 已完成修复（代码变更，均已验证写入）

变更文件与目的：

1. **Game/Assets/Scripts/PrototypeGame.cs**（最小修改，规则核心 IncidentState.cs 未动）
   - 粉笔标记键位 3→**1**，渲染由横短线改为**小十字**（横+竖双 Box），文案改"你留下了粉笔记号（小十字）"——对齐 T002 规格"按数字键1放置一个小十字"。
   - 手电筒键位 1→**2**；底部提示行同步为"WASD 移动 E 交互 1 粉笔标记 2 手电筒 J 档案 Tab 物品 Esc 暂停"。
   - **楼梯可行走跨层**（规格差距修复）：新增 OnStairs/ChangeFloor；西梯（x≤-6.4）可向上走越 y=4.55 跨至上层，东梯（x≥6.4）可向下走越 y=-3.05 跨至下层；每层固定 10 级台阶视觉，跨一层=两跑共 20 级，与既有档案文案"分成两跑，各有十级台阶"一致；CanTraverse 对楼梯区（|x|≥6.4）豁免分区墙，Partition 在 x=±7 挖口。E 交互切层保留并改为同侧落位（up→(-6,0)、down→(6,0)），拓扑一致。
2. **Game/Assets/Scripts/PrototypeSmoke.cs**
   - 新增 4 项真实路径检查：chalk-cross-on-steps（台阶上留标记）、walk-up-stairs-crosses-floor（行走上行跨层）、stair-walk-stays-inside（跨层后保持区内）、walk-down-stairs-crosses-floor（行走下行跨层）；检查数 75→79。未增加镜像断言。
3. **PLAYTEST.md**：键位与楼梯可行走说明同步（玩家可见材料，无剧透）。

## 3. 验证结果

| 检查项 | 实际操作/命令 | 结果 | 证据 |
|---|---|---|---|
| 核心规则测试（复跑） | mono mcs 编译 IncidentState+Tests 后运行（Test-Core.ps1 等价命令，字节级重定向） | **通过：24/24，COMPILE_EXIT=0，RUN_EXIT=0** | artifacts/external/T003/core-rerun-exit.txt、artifacts/tests/core-t003-ext-run.out |
| Editor Play Mode 冒烟（79 项） | run-smoke.bat STAGE 1 / Unity -batchmode -executeMethod MysteryRevival.PrototypeBuild.SmokeEditor --smoke | **未完成**：2026-09-17 09:02 独立 cmd 环境运行，导入阶段 AssetImportWorker 崩溃（code=10054）后 Editor 退出，result.txt 未产出 | artifacts/external/T003/unity-editor-smoke-t003.log（20402B）、worker-crash-log.txt |
| Windows 构建 | run-smoke.bat STAGE 2 / -executeMethod MysteryRevival.PrototypeBuild.Build | **未覆盖**：STAGE 1 未通过，未进入 | —（unity-build-t003.log 不存在） |
| 可见窗口回归 | 待构建产出后执行 input-verify.ps1 | **未覆盖** | artifacts/external/T003/input-verify.ps1（已备好） |
| Search 索引异常复现 | 三次独立 Editor 启动 | **复现成功×3**（见 §4） | artifacts/unity/editor-release.log:342、T003/unity-editor-smoke-t003.log:11、final-log-tail.txt |
| 方法调用断言不能冒充真实输入 | — | 遵守：自动交互仍是方法调用；真实键鼠/IME 明确列未覆盖 | — |

## 4. Editor 启动搜索索引异常（专项）

- 现象：每次 Editor 启动延迟函数阶段输出 `ArgumentOutOfRangeException`，堆栈固定：
  `SearchInit.IndexationOnStartup → SearchDatabase.GetDefaultSearchDatabase → Enumerable.First → SearchDatabase.EnumerateAll(List.get_Item)`。
- 复现：**3 次独立启动全部复现**（含 C 盘历史成功日志 editor-release.log:342——历史上异常与冒烟通过并存）。
- 影响面：堆栈全部在 UnityEditor.Search 引擎模块，项目代码不在链路；异常后 Play Mode 检查与构建历史上均正常完成（C 盘 75 项通过记录）。对编辑器 Search 面板可用性的影响未单独验证。
- 已尝试的最小可回滚处理：重建 Library（含从 C 盘复制完整缓存）——**异常仍复现**，排除缓存损坏单一根因。
- 遗留处置建议（留独立验收者决定，本报告不豁免）：
  a) 接受为已知引擎侧启动噪声（有 75/79 项通过的历史旁证）；
  b) 或在 ProjectSettings 禁用启动索引（配置级规避，损失 Editor 搜索索引功能）；
  c) 或升级前保持登记。本执行者未做 b/c，避免"吞异常/改配置宣布修复"。

## 5. 执行环境阻塞记录（如实登记，非项目代码缺陷）

- WorkBuddy 宿主内启动 Unity batchmode 5 次全部死锁于 bee_backend 启动点（Package Manager 注册完成后 0 CPU 挂起）；逐项排除代理变量、Library 缓存、内存（曾低至 422MB，后 7.6GB 仍卡）、UnityLockfile。完整诊断：artifacts/external/T003/host-blocking-diagnosis.md。
- 宿主内 WMI/schtasks/COM/cmd/Start-Process/Start-Job 全部被安全策略拦截，无法创建干净进程树 → 生成 run-smoke.bat 交用户独立环境执行。
- bat 失败链（已定位根因）：①21:09 版无 pause 且 cmd 对 GUI 程序不等待——窗口秒关；②23:25 版换行符为 LF——cmd 解析崩溃必闪退（字节证据 bat-bytes.txt：0A 无 0D）；③23:4x 版（CRLF+全 ASCII junction 路径 D:\projects\mr）语法修复成功，2026-09-17 09:02 用户运行后 Unity 真正启动、AssetImportWorker 就绪（构建链首次推进），但导入阶段 worker 崩溃（code=10054，worker 日志末尾 Curl error 3: URL rejected——环境代理变量污染 worker 请求为高嫌疑），Editor 退出，冒烟未完成。
- 已建 junction：D:\projects\mr → D:\projects\神秘复苏（辅助用，验收后可删）。

## 6. 未覆盖项（不给绿）

1. Editor Play Mode 79 项冒烟——本轮未完成（导入 worker 崩溃）。
2. Windows 构建与可运行构建交付——未进入。
3. 真实键盘、鼠标、中文输入法操作验证（含输入法组合中 Esc）——input-verify.ps1 已备好（keybd_event/mouse_event/IME 布局切换/截图序列），待构建产出后执行；不用 State.Notes 赋值冒充。
4. 1366×768 / 1920×1080 / 窗口缩放与 Windows 显示缩放下的档案、按钮、长文本检查——未执行。
5. 声画真人听感（首次异常三脉冲、暂停冻结音频）——无人工，未覆盖。
6. 楼梯可行走/十字标记的实机视觉确认——冒烟断言已写，实机截图未取得。
7. 楼梯台阶"可数"的玩家主观体验确认——现为可行走+固定 10 级台阶示意，是否满足"可观察与复核"由独立验收判断。

## 7. 工时与预算

- 历史净工时：未知（沿用评审登记，不填零、不冒算）。
- 本轮：开始 2026-09-16 19:42，报告落盘 2026-09-17 09:2x；有效工时约 6h（环境阻塞等待另记：宿主诊断与用户协作约 3.5h 墙钟）；未超 Roadmap"修订 8 有效工时"上限，但历史存量未知，预算合规性不声称。
- 未重置旧预算；新批次额度由用户明确。

## 8. 可复现命令

```powershell
# 核心测试（已通过）
powershell -NoProfile -File "D:\projects\神秘复苏\tools\Test-Core.ps1" -LogName "core-t003-external-20260916.log"

# Editor Play Mode 冒烟（待环境修复后复跑；须在独立 cmd，非 WorkBuddy 宿主）
set MYSTERY_SMOKE_OUTPUT=D:\projects\神秘复苏\artifacts\external\T003\smoke-editor
"D:\Unity\Editors\6000.5.10f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\projects\神秘复苏\Game" -executeMethod MysteryRevival.PrototypeBuild.SmokeEditor --smoke -logFile "D:\projects\神秘复苏\artifacts\external\T003\unity-editor-smoke-t003.log"

# Windows 构建（冒烟通过后）
set MYSTERY_BUILD_PATH=D:\projects\神秘复苏\artifacts\external\T003\build\MysteryRevival.exe
"D:\Unity\Editors\6000.5.10f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "D:\projects\神秘复苏\Game" -executeMethod MysteryRevival.PrototypeBuild.Build -logFile "D:\projects\神秘复苏\artifacts\external\T003\unity-build-t003.log"

# 可见窗口+真实输入（构建后）
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\projects\神秘复苏\artifacts\external\T003\input-verify.ps1"
```

## 9. 交付物清单

- 代码：Game/Assets/Scripts/PrototypeGame.cs、PrototypeSmoke.cs；文档：PLAYTEST.md。
- 证据目录：artifacts/external/T003/（接管哈希、环境诊断、各日志与退出码、input-verify.ps1、run-smoke.bat、host-blocking-diagnosis.md）。
- 运行须知：构建产物须整目录复制（含 _Data/MonoBleedingEdge），见 PLAYTEST.md。
- 待办交接：修复 worker 崩溃环境因素（嫌疑：HTTP_PROXY 等代理变量；建议在干净环境 set HTTP_PROXY= 后重跑 STAGE 1）→ 完成冒烟 79 项与构建 → 执行 input-verify.ps1 → 补齐 §6 未覆盖项 → 交独立验收（任务 B）。

## 10. 结论

任务 A 范围内的规格差距修复（粉笔十字/键位、楼梯可行走、E 映射、冒烟增强）已实现且核心规则 24 项回归通过；搜索索引异常已三次复现并按规则提交验收处置，未吞异常未升级 Editor。Editor 冒烟、Windows 构建、真实输入与声画验证因执行环境问题未完成，如实列为未覆盖。**整体状态：局部完成，待独立验收；不自行宣布 T003 或 M0 通过。**
