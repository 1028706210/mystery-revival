# T003-02A-R1 独立 UPM 缓存受控 Editor 复跑报告

日期：2026-09-20  
执行会话：06 测试与集成  
目标工程：`D:\projects\神秘复苏`  
Unity：`6000.5.10f1_3bd4f66ad299`（文件版本 `6000.5.10.3921142`）  
结论：**本次 Editor 门禁失败，不具备进入 T003-02B 的条件。** UPM 独立缓存重定向及包解析已成功，但 Editor 在 Search 启动索引异常后未执行冒烟检查，达到 30 分钟上限后仅终止本次进程树。

## 1. 边界与执行次数

- 仅运行 Editor 冒烟一次；未经过 Hub，未运行 Windows 构建，未追加第二次启动或环境试错。
- 仅对子 Editor 进程设置 `UPM_CACHE_ROOT=D:\UnityCaches\T003-02A-R1\upm` 和本次证据输出变量；未设置用户级/系统级环境变量，未改 Unity 全局配置。
- 未修改产品源码、测试源码、`manifest.json`、`packages-lock.json`、`docs/TASKS.md` 或两个全局 Junction。
- 未删除缓存、日志、备份或本次运行产物；新缓存和全部证据均保留。
- 本报告不宣布全局缓存恢复、T003 通过，也不批准或启动 T003-02B、T004、M0b、M1 或 A001。

证据根目录：`artifacts\external\T003-02A-R1\`

## 2. 前检与后检

| 检查项 | 当前证据 | 结果 | 说明 |
|---|---|---|---|
| 独占与进程 | 前检 Unity、Hub、UPM、Worker、bee 等相关进程计数为 0；用户已确认独占窗口 | 通过 | 未强制终止任何前存进程 |
| 目标冲突 | 新缓存、证据目录、报告路径在操作前均不存在 | 通过 | 未覆盖旧证据 |
| Unity 版本 | `6000.5.10f1_3bd4f66ad299` | 通过 | 直接运行锁定 Editor |
| 基线哈希 | 4 个产品源码、1 个测试、`manifest.json`、`packages-lock.json` 前后 SHA256 完全一致 | 通过 | 未发现源码或依赖配置漂移 |
| 结束进程 | 后检 Unity 相关进程计数为 0 | 通过 | 包装器只终止 PID 36764 的本次进程树 |
| 磁盘空间 | 前检 C: 9,074,892,800、D: 50,780,983,296 字节可用；后检 C: 14,636,081,152、D: 59,029,893,120 字节可用 | 记录 | 仅记录读数；未调查或归因读数变化 |

前检证据：`preflight.txt`。后检证据：`postflight.txt`、`post-hashes.txt`。

关键 SHA256（前后相同）：

- `Game\Assets\Editor\PrototypeBuild.cs`：`B149239FB7C9CD346C3A9A899983527A2086DFE78BB24E69598A01FD24FF9568`
- `Game\Assets\Scripts\Domain\IncidentState.cs`：`992DB48AF10A0F3817A63F7E4313038C71AAB56ABA8D5D1C5FC8DDEA048D3D83`
- `Game\Assets\Scripts\PrototypeGame.cs`：`6E3A9C55BC7381402D63168B90ECDAAA2F7D3F473D36CDC9FD6D2458C12AE7EB`
- `Game\Assets\Scripts\PrototypeSmoke.cs`：`B245453DF109A9732C47DC4DC10D84698FAE01FE2DCEEE93EC5C1793807A3750`
- `tests\IncidentStateTests.cs`：`F2033B08446F0324328885F437FA826794F18E2F1FD3C8EDA622CD92962F6BFC`
- `Game\Packages\manifest.json`：`8BB9A44EB6BF360D3261F8BE3161362AAE9585B2FB92EA9990836BAA497C25FB`
- `Game\Packages\packages-lock.json`：`FE7685C3E712EE3D6997F2983F2902DE91253D8A7F5358C3C359CA3334374857`

## 3. 实际启动与终止

实际命令：

```text
"D:\Unity\Editors\6000.5.10f1\Editor\Unity.exe" -batchmode "-nographics" -projectPath "D:\projects\神秘复苏\Game" -executeMethod "MysteryRevival.PrototypeBuild.SmokeEditor" "--smoke" "-logFile" "D:\projects\神秘复苏\artifacts\external\T003-02A-R1\editor\unity-editor-smoke.log"
```

ProcessStartInfo 子进程环境：

```text
UPM_CACHE_ROOT=D:\UnityCaches\T003-02A-R1\upm
MYSTERY_SMOKE_OUTPUT=D:\projects\神秘复苏\artifacts\external\T003-02A-R1\smoke-editor
```

| 字段 | 实际值 |
|---|---|
| 启动时间 | `2026-09-20T21:54:54.9504893+08:00` |
| 父 Editor PID | `36764` |
| 结束时间 | `2026-09-20T22:24:57.1967004+08:00` |
| 持续时间 | `1802.246` 秒 |
| 结束方式 | `timeout-kill-parent-tree` |
| 自然退出 | 否 |
| Editor 退出状态 | `-1`（在包装器超时终止后读取） |
| 包装器真实退出码 | `1` |

证据：`run-editor-r1.ps1`、`editor\launch.txt`、`editor\exit-summary.txt`、命令执行返回。

## 4. UPM 独立缓存

| 检查项 | 当前证据 | 结果 |
|---|---|---|
| 子进程变量 | UPM 日志记录 `Detected environment variables: UPM_CACHE_ROOT=D:\UnityCaches\T003-02A-R1\upm` | 通过 |
| 包列表查询 | `project:list-packages` 均返回 200 | 通过 |
| 包解析 | `project:resolve-packages --> 200 (3902 ms)`；Editor 记录 `Done resolving packages in 3.93 seconds` | 通过 |
| 网络请求 | Unity 自动从 `https://packages.unity.com` 下载 `com.unity.package-manager.metadata-6000.5@0.0.98`，11.4 kB | 记录 |
| 新缓存产物 | `db\content-v2`、`db\index-v5` 等共 8 个文件、44,519 字节 | 通过 |
| 旧小写缓存引用 | Editor 日志中旧 `C:\Users\李帅朋\AppData\Local\Unity\cache\upm` 引用计数为 0 | 通过 |
| 依赖配置变化 | `manifest.json`、`packages-lock.json` 前后哈希相同 | 通过 |

结论：**本次进程专属 UPM 缓存重定向实际生效，并消除了 T003-02A 中的 UPM 500/包解析失败。** 该结论只适用于本次启动，不代表用户全局缓存或 Junction 已恢复。

证据：`editor\upm.log`、`editor\unity-editor-smoke.log`、`upm-cache-inventory.txt`、`post-hashes.txt`。

## 5. Editor 冒烟门禁

| 门槛 | 实际结果 | 状态 |
|---|---|---|
| 父 Editor 自然退出码 0 | 超时终止；Editor 状态 `-1`，包装器退出码 `1` | 失败 |
| `result.txt` 存在且一致 | 未生成 | 失败 |
| `SMOKE_RESULT=PASS checks=80` | 0 条 `SMOKE_RESULT` | 失败 |
| 80 条预期检查完整执行且无失败 | 0 条 `SMOKE_PASS`；无内容可比对 | 失败 |
| 预期检查内容核对 | 已保留 80 条的名称/次数直方图；因实际检查为 0，无法逐项对比 | 未覆盖 |

当前源码预期仍为 80 条：`walk-to-target` 30 次，`door-intent-dialog`、`explicit-confirmation`、`action-started` 各 5 次，其余 35 个命名检查各 1 次。证据：`expected-check-histogram.tsv`、`smoke-validation.txt`。

本轮没有以进程存活、UPM 成功、无新增编译错误或退出前日志停止代替冒烟通过。

## 6. Curl、Search、Worker 分项结果

### 6.1 CurlRequestCache

- 父 Editor 与 Worker 均无法打开或重建 `C:\Users\李帅朋\AppData\Local\Unity\Caches\CurlRequestCache.db`，随后明确退回内存缓存。
- Worker 另记录一次 `Curl error 3: URL rejected: Malformed input to a URL function`。
- UPM 联网和包解析仍成功，因此本轮证据不能把 Curl 持久缓存错误认定为 UPM 或本次停滞的已确认根因。

状态：**失败（持久 Curl 缓存访问）；回退路径已发生。**

### 6.2 Search

- Editor 记录 `Start Indexing on Editor startup` 后立即出现一次 `ArgumentOutOfRangeException`。
- 堆栈位于 `UnityEditor.Search.SearchDatabase.EnumerateAll`、`GetDefaultSearchDatabase`、`SearchInit.IndexationOnStartup`。
- 此后 Editor 日志停在 Worker 就绪与缓存裁剪，约 29 分钟无新增行，冒烟方法未产生结果，最终超时中止。

状态：**失败。** Search 异常与随后停滞有直接时序关联；本报告不越过证据将其写成唯一或最终根因。

### 6.3 AssetImportWorker

- 父日志记录一次 `Starting new worker id: 0` 和一次 `Worker ready: AssetImportWorkerHW0 0`。
- Worker 日志末尾记录一次 `AssetImportWorkerClient::OnTransportError - code=10054`，文件最后写入时间 `22:24:57.143`，与父进程树超时终止同秒。
- 因 10054 出现在强制终止时点，不能据此确认 Worker 在终止前独立崩溃；同样也不能宣告 Worker 端到端稳定通过。

状态：**Worker 启动已覆盖；端到端稳定性未覆盖。** 10054 作为终止时观察到的故障信号保留，不归因为本次停滞根因。

证据：`editor-log-findings.txt`、`worker-log-findings.txt`、`editor\unity-editor-smoke.log`、`workers\post\AssetImportWorkerHW0.log`。

## 7. 工时与批次额度

- 原批次：1.50 有效小时。
- R1 前已报告使用：0.70 有效小时；预计剩余 0.80 小时。
- 本任务有效操作、采证和报告约 **0.30 小时**，不超过本任务 0.50 小时上限。
- Editor 导入/等待：**1802.246 秒（约 0.50 小时）**，按授权单独记账，不计入上述有效操作工时。
- 预计批次有效工时剩余约 **0.50 小时**。

## 8. 剩余阻塞与放行结论

剩余阻塞：

1. Search 启动索引仍抛 `ArgumentOutOfRangeException`，随后本轮 Editor 在 30 分钟内未进入冒烟检查。
2. 父 Editor 未自然退出，`result.txt` 缺失，实际检查为 0/80。
3. Curl 持久缓存仍不可用；虽已回退内存且 UPM 成功，但其对后续 Editor 生命周期的影响尚未独立排除。
4. Worker 端到端稳定性未覆盖；超时终止时出现 10054，不能当作独立崩溃已确认，也不能写成已修复。
5. Windows 构建按任务边界未运行。

**不具备进入 T003-02B 的条件。** 下一步应交 00 总控决定是否安排针对当前 Search 索引异常/停滞的独立诊断；不得在缺少新授权时继续复跑、清理 Search 索引、修改配置或启动构建。
