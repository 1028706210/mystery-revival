# T003-E02 保留失效 Junction 并恢复本地 Unity 缓存目录

日期：2026-09-19  
执行会话：06 测试与集成  
批准范围：仅处理当前 Windows 用户的 `Unity\cache` 与 `Unity\Caches` 两个目录项  
工时上限：0.25 有效小时，计入现有批次剩余额度  
结论：**第一项 Junction 改名即被 Windows 错误 649 阻止，未发生任何目录项变更；缓存目录访问未恢复。**

## 1. 边界与执行裁定

- 用户本次任务下发作为此前独占窗口的延续确认，同时执行前和操作前再次实时检查 Unity Editor、Hub、UPM、Worker、bee 相关进程，计数均为 0。
- LocalAppData 写入已按工具权限流程为本次精确操作申请并获准；没有把总控批准当作沙箱授权。
- 本任务修改的是用户级环境路径，不是 Git 工作树或产品代码；未创建 worktree，未执行产品测试驱动流程。验证采用任务指定的操作前状态、最小目录项操作和操作后状态核对。
- 未启动 Unity、Editor、测试或构建；未修改产品源码、Packages、全局环境变量、ACL 或挂载状态。
- 未遍历或解析 Junction 的未知目标，未删除任何链接、缓存或目标数据。

## 2. 操作前状态

| 路径/检查 | 操作前证据 | 状态 |
|---|---|---|
| Unity 相关进程 | Editor、Hub、UPM、Worker、bee 计数 `0` | 通过 |
| `C:\Users\李帅朋\AppData\Local\Unity\cache` | `Directory, ReparsePoint`；创建/修改 `2026-09-18T15:04:52.7685468+08:00` | 符合预期失效 Junction 状态 |
| `C:\Users\李帅朋\AppData\Local\Unity\Caches` | `Directory, ReparsePoint`；创建/修改 `2026-09-18T15:04:52.7784208+08:00` | 符合预期失效 Junction 状态 |
| `cache.T003-E02.pre-recovery-20260919` | 不存在 | 通过，无覆盖风险 |
| `Caches.T003-E02.pre-recovery-20260919` | 不存在 | 通过，无覆盖风险 |
| 报告与证据目录 | 均不存在 | 通过，无同名交付冲突 |
| C 盘空间 | 总计 `107,380,469,760` 字节；可用 `10,449,780,736` 字节 | 已记录 |

## 3. 实际命令与结果

开始时间：`2026-09-19T22:20:20.4257125+08:00`

在再次确认相关进程为 0、两个原路径仍为 ReparsePoint、全部备份名与回退名均不存在后，执行第一条批准命令：

```powershell
Rename-Item `
  -LiteralPath 'C:\Users\李帅朋\AppData\Local\Unity\cache' `
  -NewName 'cache.T003-E02.pre-recovery-20260919' `
  -ErrorAction Stop
```

实际结果：

```text
System.IO.IOException
创建操作失败，因为名称含有至少一个装入点，它引起生成的卷没有同指定的设备对象连接上。
```

该错误与 T003-E01 已记录的 Win32 `649` 装入点/设备未连接状态一致。事务封装进入失败分支并执行 `exit 1`。

根据任务门禁“任一步失败立即停止”，没有尝试：

- 第二个 `Caches` Junction 改名；
- 其他 PowerShell、Win32 或 `cmd` 改名方式；
- 删除、重建或替换 Junction；
- 创建普通 `cache` / `Caches` 目录；
- 创建哨兵文件；
- 修改 ACL、挂载卷或探测未知目标。

## 4. 操作后状态、部分失败与回退

| 路径 | 操作后状态 | 与操作前比较 |
|---|---|---|
| `...\Unity\cache` | 仍存在，`Directory, ReparsePoint`，创建/修改时间未变 | 未改名、未修改 |
| `...\Unity\Caches` | 仍存在，`Directory, ReparsePoint`，创建/修改时间未变 | 未执行改名、未修改 |
| `...\Unity\cache.T003-E02.pre-recovery-20260919` | 不存在 | 未创建 |
| `...\Unity\Caches.T003-E02.pre-recovery-20260919` | 不存在 | 未创建 |
| 两个 `*.generated-rollback-20260919` | 均不存在 | 未创建 |

- **部分变更：没有。** 第一项改名未成功，状态变量仍为 `CACHE_RENAMED=false`。
- **回退动作：没有实际执行。** 失败分支检查后确认没有任何已完成步骤需要回退，输出 `ROLLBACK_END=SUCCESS`。
- **普通目录：未创建。** 因此前置两项改名没有同时成功，严格未进入建目录阶段。
- **访问验证：未执行。** 没有普通目录可枚举，也没有创建或删除任何哨兵文件。
- 操作后 Unity 相关进程仍为 `0`。
- 操作后 C 盘可用空间为 `10,448,400,384` 字节；该快照差异不归因于本任务，因为本任务没有创建缓存目录或哨兵文件。

## 5. 当前阻塞与限制

T003-E01 的恢复方法假设 PowerShell 能从父目录重命名失效 Junction 目录项；本次实际证明该假设在当前 Windows 状态下不成立。原生 `Rename-Item -LiteralPath` 仍会在目录项操作前触发失效装入点解析，并返回 649。

仍然未知：

- 原 Junction 的真实 substitute target；
- 其对应卷或原迁移位置；
- 是否存在不解析目标且能安全保存目录项的受支持操作路径；
- 该替代操作的回退行为和对旧目标的影响。

因此本任务不能把失败后改用其他 API、删除链接、重建链接或直接创建同名目录视为同一已批准方案。需要 00 总控基于本阻塞包重新决定是否安排一个新的、明确授权操作机制的环境维护任务。

## 6. 工时与门禁

- 执行前检查：`2026-09-19T22:19:21.0553837+08:00`
- 失败后终态检查：`2026-09-19T22:20:51.2188004+08:00`
- 加上报告与证据整理，实际有效工时约 **0.08 小时**，未超过 0.25 小时上限。
- 出现新阻塞后已停止，没有使用剩余额度扩展诊断或修复范围。

门禁结论：

- **缓存目录访问未恢复。**
- 不得写“Unity 已恢复”“Editor 已通过”或“T003 已通过”。
- T003-02A-R1 未启动并继续锁定。
- T003-02B、T003、T004 继续锁定。

## 7. 证据

- `artifacts/external/T003-E02/00-preflight.txt`
- `artifacts/external/T003-E02/01-operation-attempt.txt`
- `artifacts/external/T003-E02/02-postcheck.txt`
- `artifacts/external/T003-E02/evidence-manifest-sha256.tsv`
