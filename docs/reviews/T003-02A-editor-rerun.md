# T003-02A 受控 Editor 复跑报告

日期：2026-09-19  
执行会话：06 测试与集成  
目标工程：`D:\projects\神秘复苏`  
授权：有效工时上限 1.5 小时；用户确认独占 Unity、D 盘工程及可见桌面窗口。  
结论：**Editor 门禁失败，不具备进入 T003-02B 的条件。T003、M0a、M0 与 T004 均未批准。**

## 1. 边界与变更

- 未修改产品源码、测试源码、Unity 配置、Packages 配置或 `docs/TASKS.md`。
- 未升级 Unity，未删除或清理缓存、锁文件、历史日志或备份。
- 未运行旧 `run-smoke.bat`，未启动 Windows 构建。
- 保留用于交付的测试与诊断证据仅归档到 `artifacts/external/T003-02A/`。Unity 启动按正常运行行为更新了 5 个工程运行态文件：`Game/Library/ArtifactDB`、`Game/Library/SourceAssetDB`、`Game/Logs/upm.log`、`Game/Temp/FSTimeGet-6242b5fd335b75c4ea84a0580c86cae7`、`Game/Temp/UnityLockfile`；未清理或回滚这些运行态文件。
- 本报告为唯一新增文档：`docs/reviews/T003-02A-editor-rerun.md`。

## 2. 基线复核

| 检查项 | 实际操作/证据 | 结果 | 影响范围 |
|---|---|---|---|
| 实际路径 | `git -C D:\projects\神秘复苏 rev-parse --show-toplevel` → `D:/projects/神秘复苏`；`D:\projects\mr` 仅为同工程 Junction | 通过 | 只使用正式路径启动 Unity |
| 并发占用 | 启动前 Unity、AssetImportWorker、UnityPackageManager、bee_backend 等进程计数为 0；用户确认独占窗口 | 通过 | 无已发现并发进程 |
| Git 状态 | `master` 无可用 HEAD，全部文件仍为未跟踪状态；原始输出见 `git-status.txt`、`git-head.txt` | 失败（可追溯性） | 继续依赖关键文件 SHA256，而非 Git diff |
| Unity 版本 | `6000.5.10f1_3bd4f66ad299`，文件版本 `6000.5.10.3921142` | 通过 | 与接管基线一致 |
| 关键文件哈希 | 六个关键源码/测试文件均与 T003-01 基线一致；Editor 运行后再次核对仍一致 | 通过 | 未发现未解释的产品代码变化 |
| 旧锁文件 | 前检时 `Game\Temp\UnityLockfile` 为 2026-09-17 遗留文件；未删除。本次 Unity 启动后该文件被重新创建为 2026-09-19 21:40:44 | 记录 | 锁文件本身未阻止本次 Editor 初始化；结束后相关进程为 0 |

关键哈希：

- `IncidentState.cs`：`992DB48AF10A0F3817A63F7E4313038C71AAB56ABA8D5D1C5FC8DDEA048D3D83`
- `PrototypeGame.cs`：`6E3A9C55BC7381402D63168B90ECDAAA2F7D3F473D36CDC9FD6D2458C12AE7EB`
- `PrototypeSmoke.cs`：`B245453DF109A9732C47DC4DC10D84698FAE01FE2DCEEE93EC5C1793807A3750`
- `PrototypeBuild.cs`：`B149239FB7C9CD346C3A9A899983527A2086DFE78BB24E69598A01FD24FF9568`
- `IncidentStateTests.cs`：`F2033B08446F0324328885F437FA826794F18E2F1FD3C8EDA622CD92962F6BFC`
- `Test-Core.ps1`：`CDB37B198C4A7AF468A5165BF553865E263F3FD648487EE5AD2976BEA3A33532`

## 3. 核心规则测试

未调用会写入旧 `artifacts/tests` 的脚本；使用同一 Unity 内置 Mono/mcs，直接把编译产物和全部输出写入 `T003-02A/core/`。

第一次执行已实际生成可执行文件并输出 24/24，但汇总封装使用了当前 PowerShell 不兼容的空值安全调用写法，未保存退出码。该次所有文件保留，未覆盖。随后仅修正封装写法，用 `*-02` 新文件名复跑；未改变源码、编译器、运行时或环境。

| 检查项 | 结果 | 证据 |
|---|---|---|
| 编译 | `COMPILE_EXIT=0`，stdout/stderr 均为空 | `core/exit-summary-02.txt`、`core/compile-02.*.txt` |
| 运行 | `RUN_EXIT=0` | `core/exit-summary-02.txt` |
| 测试结果 | `RESULT passed=24 failed=0` | `core/run-02.stdout.txt` |
| 错误输出 | 空 | `core/run-02.stderr.txt` |

结论：**本次核心规则测试通过 24/24。** 这只是 T003-02A 执行证据，不代替后续完整独立技术验收。

## 4. Editor 受控复跑

实际参数：

```text
D:\Unity\Editors\6000.5.10f1\Editor\Unity.exe
-batchmode -nographics
-projectPath D:\projects\神秘复苏\Game
-executeMethod MysteryRevival.PrototypeBuild.SmokeEditor
--smoke
-logFile D:\projects\神秘复苏\artifacts\external\T003-02A\editor\unity-editor-smoke.log
```

仅为当前子进程设置：

```text
MYSTERY_SMOKE_OUTPUT=D:\projects\神秘复苏\artifacts\external\T003-02A\smoke-editor
```

没有清除或修改代理变量，没有调整 Unity/Package Manager 配置。

| 检查项 | 实际结果 | 状态 | 证据 |
|---|---|---|---|
| 开始/结束 | 2026-09-19 21:40:43.587 至 21:40:47.710，持续 4.123 秒 | 记录 | `editor/launch.txt`、`editor/exit-summary.txt` |
| 父 Editor 退出 | PID 12932，自然结束，未超时，真实退出码 `1` | 失败 | `editor/exit-summary.txt` |
| Package Manager | `project:list-packages` 与 `project:resolve-packages` 均返回 500；Editor 报 `Failed to resolve packages` 后退出 | 失败 | `editor/upm.log`、`editor/unity-editor-smoke.log` |
| `result.txt` | 未生成 | 失败 | `editor/exit-summary.txt`、`editor/smoke-execution-counts.txt` |
| 冒烟检查 | `SMOKE_PASS` 0 条，`SMOKE_RESULT` 0 条 | 失败 | `editor/smoke-execution-counts.txt` |
| Search 索引异常 | 本次没有进入启动索引阶段，异常计数 0 | 未覆盖 | 不能据此写“Search 已修复” |
| AssetImportWorker | 本次没有启动新 Worker；无 `Starting new worker`、`Worker ready` 或 transport 事件 | 未覆盖 | `editor/smoke-execution-counts.txt`；仅复制了旧 Worker 日志作为前后对照 |
| Windows 构建 | 按门禁未启动；`T003-02A/build` 不存在 | 未覆盖（按计划停止） | Editor 未通过，不具备构建前置 |

### 4.1 本次直接失败点

父 Editor 日志在 `Application.AssetDatabase Initial Refresh Start` 后记录：

```text
[Package Manager] UNKNOWN: unknown error, open
'C:\Users\李帅朋\AppData\Local\Unity\cache\upm\db\index-v5\21\28\88d76cc6c28d4aa428ef01affa7c5c504b539749b6e85f488bc91c7ace05'
Failed to resolve packages ... No packages loaded.
Application will terminate with return code 1
```

同一日志开头还记录 `CurlRequestCache.db` 无法打开、无法删除并重建，随后退回内存缓存。

只读探针结果：

- 失败目标文件存在，长度 248 字节，时间为 2026-09-19 12:07:02。
- `C:\Users\李帅朋\AppData\Local\Unity\cache` 标记为 `Directory, ReparsePoint`。
- 对失败目标执行哈希、ACL 查询，以及对 `cache` 执行 reparse 查询，均返回 Windows 错误 649：名称包含装入点，解析到未与指定设备对象连接的卷。
- 没有执行删除、重建、权限调整、重新挂载或任何配置变化，因此**只能登记为“用户级 Unity 缓存/重解析点访问异常的强关联证据”，不能宣布根因已确认**。

证据：`editor/cache-readonly-probe.txt`。本次失败发生在新的 Worker 和 Search 启动索引之前，与此前 Worker `code=10054`、Curl 3/35 及 Search `ArgumentOutOfRangeException` 必须分别记录。

## 5. 当前预期检查数核对

外部报告声称检查数由 75 增至 79，但当前源码的动态检查计数应为 **80**：

- 接管前 `PrototypeSmoke.cs` 哈希 `F4FD...0C8C`，历史运行结果为 `checks=75`。
- 变更前相关路径会执行 5 次 `Check`：3 次 `Walk` 内断言，加 2 次显式楼层/返回断言。
- 当前相关路径会执行 10 次 `Check`：5 次 `Walk` 内断言，加 5 次显式十字、楼层、区内及返回断言。
- 净增 5，因此当前预期为 `75 + 5 = 80`。
- 报告列出的 4 个新命名检查没有计入路径变化带来的额外 `Walk` 断言净增。

本次 Editor 在 Package Manager 阶段退出，实际执行 **0/80**；既不能写 79 项通过，也不能写 80 项通过。

证据：

- `editor/expected-check-count.txt`
- `editor/prototype-smoke-baseline-to-current.diff`
- `editor/smoke-execution-counts.txt`
- 历史 `artifacts/unity/editor-release.log` 的 `SMOKE_RESULT=PASS checks=75`

## 6. 有效工时

- 授权上限：1.5 小时。
- 受控执行与证据核对：2026-09-19 21:36:26 至 21:45:08，共 8.69 分钟。
- 加上报告整理，本任务实际有效工时约 **0.25 小时**。
- 授权上限尚有约 **1.25 小时**未使用，但本任务已因 Editor 门禁失败按规则停止；剩余时长不构成自动继续或清理缓存的授权。

## 7. 剩余阻塞与下一步条件

当前阻塞：

1. Unity 用户级 Package Manager 缓存目标无法打开，UPM resolve 返回 500，Editor 以 1 退出。
2. `Unity\cache` 是异常访问的 ReparsePoint，读元数据可见，但哈希/ACL/reparse 查询返回 Windows 错误 649；其设计目标、挂载状态和安全修复方式尚未确认。
3. 当前 Editor 冒烟预期是 80 项，不是外部报告的 79 项；实际 0 项执行。
4. Search 本次未到达，不能复核持续性或影响。
5. AssetImportWorker 本次未启动，不能复核此前 transport/Curl 故障。
6. Windows 构建未运行，符合“Editor 未通过即停止”的边界。

建议由 00 总控另行决定是否授权一个**独立的 Unity 用户缓存/重解析点诊断与修复任务**。在确认 `C:\Users\李帅朋\AppData\Local\Unity\cache` 的重解析目标和恢复方式前，不应直接删除缓存、改 ACL、重建链接或继续多次 Editor 尝试。环境恢复后只进行一次新的 Editor 受控复跑，并以 `result.txt`、`SMOKE_RESULT=PASS checks=80`、80 条 `SMOKE_PASS`、父进程退出码 0 同时成立作为进入后续构建的前置。

## 8. 放行结论

**不具备进入 T003-02B 的条件。**

原因：Editor 在 Package Manager 解析阶段失败，退出码 1，未生成 `result.txt`，当前 80 项冒烟实际执行 0 项。Search 与 Worker 本次均未到达，Windows 构建按门禁未启动。

本报告不批准 T003、T004、M0a 或 M0。下一步交 00 总控安排环境诊断/恢复；任何修改过的环境与后续运行结果仍需保留新证据，并在技术收尾后进入独立验收。
