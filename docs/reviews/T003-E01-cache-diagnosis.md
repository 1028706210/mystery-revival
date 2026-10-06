# T003-E01 Unity 用户缓存与重解析点只读诊断

日期：2026-09-19  
执行会话：06 测试与集成  
目标工程：`D:\projects\神秘复苏`  
有效工时上限：0.5 小时（沿用 T003-02A 剩余额度）  
结论：**已确认 Unity 用户缓存路径的直接故障层，但无法只读还原 Junction 的真实替代目标或迁移来源。本任务未执行恢复，T003-02A-R1、T003-02B、T003 和 T004 均未放行。**

## 1. 执行边界

- 先复用 `T003-02A` 的 UPM、父 Editor 和缓存探针证据，没有重复启动失败的 Editor。
- 未启动 Unity Editor、Unity Hub、UnityPackageManager、AssetImportWorker、构建或测试。
- 未删除、移动或重命名缓存/链接，未修改 ACL、重解析点、挂载状态、环境变量、全局配置或 Unity 版本。
- 未读取或输出代理认证信息、Token、凭据；环境变量检查只记录与 `UPM`/`UNITY` 直接相关的变量名是否存在，不记录值。
- 新证据仅写入 `artifacts/external/T003-E01/`；写入前目标报告和证据目录均不存在，Unity 相关进程计数为 0。

## 2. 诊断结论

### 2.1 已确认事实

1. `C:\Users\李帅朋\AppData\Local\Unity\cache` 和直接相关的 `...\Caches` 均有 `Directory, ReparsePoint` 属性；父目录的 `dir /a:l` 将两者标为 `<JUNCTION>`。
2. 两项分别创建于 `2026-09-18 15:04:52.768` 和 `15:04:52.778`，时间相差约 10 毫秒，属于同一批配置变化的强关联迹象，但创建者和创建命令未知。
3. 父路径 `...\Local` 与 `...\Local\Unity` 均可访问、可枚举；`Unity` 父目录由当前用户拥有，继承 ACL 中当前用户、SYSTEM 和 Administrators 为 FullControl。
4. 一旦解析或枚举任一 Junction，Windows 稳定返回 `HRESULT 0x80070289` / Win32 `649`：装入点解析到未与指定设备对象连接的卷。
5. T003-02A 中 UPM 失败文件位于小写 `cache` 下；本轮对该文件做只读 SHA256 查询再次得到 649。`CurlRequestCache.db` 位于大写 `Caches` 下，对它的只读查询也得到 649。
6. 当前系统只列出 C、D 两个盘符和四个卷 GUID，其中两个无盘符。现有只读查询无法把任何卷 GUID与这两个 Junction 建立对应关系。
7. Process、User、Machine 三个环境范围内均未发现名称以 `UPM` 或 `UNITY` 开头的变量；这仅排除当前可见的同名前缀覆盖，不代表穷尽所有 Unity 配置。
8. 工程内直接相关脚本/记录搜索未发现 `mklink` 命令、目标路径、卷 GUID或回退记录。

### 2.2 因果边界

| 层级 | 结论 | 证据边界 |
|---|---|---|
| T003-02A 的直接失败点 | **已确认** | UPM 访问路径穿过失效 Junction；父 Editor 日志与本轮 649 可重复只读查询一致 |
| Windows 路径解析故障 | **已确认** | 父路径正常，两个 Junction 的 resolve/enumerate/hash/ACL 查询均在目标解析阶段返回 649 |
| 原 Junction 的真实替代目标 | **未知** | PowerShell Target/LinkType 为空；`fsutil`、Win32、相对 `NtOpenFile` 均在取得原始 reparse 数据前返回 649 |
| 关联目标卷 | **未知** | 当前卷清单不能与 Junction 建立映射；不得把两个无盘符卷猜作目标 |
| 为何形成失效链接 | **未知** | 没有找到迁移脚本或创建记录；可能的卷移除、迁移或链接损坏均未获证实 |
| ACL 是否为目标侧附加问题 | **未覆盖** | 父 ACL 正常，但目标 ACL因 649 无法读取；没有修改 ACL |
| 代理、Search、Worker | **不在本任务结论内** | 本轮未启动 Editor，不能用本任务宣布其已修复或为根因 |

因此可以确认的是：**当前用户级 Unity 缓存 Junction 已不可用，它直接阻断了 UPM 文件访问。** 不能确认的是：原链接应指向哪里、哪个卷曾被使用、谁创建链接，以及目标数据当前是否仍存在。`dir /a:l` 显示的 `[..]` 仅作系统显示文本保留，不将其当作真实替代目标。

## 3. 检查项

| 检查项 | 当前证据 | 通过/失败/未覆盖 | 影响 |
|---|---|---|---|
| 写入前冲突 | 报告和 `T003-E01` 证据目录均不存在；Unity 相关进程 0 | 通过 | 未发现同名交付或并发 Unity 占用 |
| 父路径可访问性 | `Local`、`Unity` 可枚举，父 ACL 可读 | 通过 | 故障不在已检查的父路径访问层 |
| 小写 `cache` | Junction；解析、枚举、文件哈希均返回 649 | 失败 | 直接阻断 UPM 缓存访问 |
| 大写 `Caches` | Junction；解析及 `CurlRequestCache.db` 哈希均返回 649 | 失败 | 与 T003-02A 的 Curl 请求缓存错误直接对应 |
| 真实链接目标 | 所有只读解析方式均在返回目标前失败 | 未覆盖 | 不能按原迁移设计重连或挂载 |
| 目标卷 | 仅取得当前卷清单，无法映射 Junction | 未覆盖 | 禁止猜测无盘符卷或执行挂载 |
| 权限分层 | 父 ACL 正常；目标 ACL不可读 | 部分通过/目标未覆盖 | 不支持直接改 ACL |
| 迁移记录 | 工程内无直接创建/目标/回退记录 | 未覆盖 | 无法证明历史设计意图 |
| 恢复方案 | 已形成保留原 Junction 的本地普通目录方案 | 方案可审阅，未执行 | 需 00 单独批准 T003-E02 |
| Editor/Search/Worker/构建 | 未启动 | 未覆盖（符合边界） | T003-02A-R1 和 T003-02B 继续锁定 |

## 4. 首选恢复方案（仅提案）

### 4.1 精确对象

1. `C:\Users\李帅朋\AppData\Local\Unity\cache`
2. `C:\Users\李帅朋\AppData\Local\Unity\Caches`

两者应在同一个 T003-E02 中受控处理，因为两者同时成为 Junction、创建时间近乎一致、均返回 649，并分别对应 T003-02A 的 UPM 致命错误和 CurlRequestCache 错误。该范围影响的是**当前 Windows 用户运行的所有 Unity 项目**，不是仅 D 盘工程。

### 4.2 拟执行动作

获得 T003-E02 独立授权后：

1. 关闭并确认 Unity Editor、Hub、Package Manager、AssetImportWorker、bee_backend 进程均为 0；复核当前属性、时间和备份名不存在。
2. 从父目录只重命名两个 Junction 目录项，保留为：
   - `cache.T003-E02.pre-recovery-20260919`
   - `Caches.T003-E02.pre-recovery-20260919`
3. 任一重命名失败、需要删除链接或出现超出目录项的影响时立即停止；不继续第二项或创建新目录。
4. 两项均成功保留后，在原精确名称创建普通本地目录 `cache` 与 `Caches`，只继承父 ACL，不手工改 ACL。
5. 验证新路径不含 `ReparsePoint`、可枚举，并用独立临时哨兵文件验证创建、读取、删除及真实退出码。
6. E02 到此结束；不得顺带启动 Editor。之后由 `T003-02A-R1` 单独进行一次 Editor 复跑。

此方案不尝试找回或覆盖旧目标数据。原 Junction 保持原样，仅换名保存；新缓存从空目录开始，由 Unity 后续重建。

### 4.3 影响、风险与回退

- 影响：当前用户下所有 Unity 项目改用新空缓存；首次运行可能需要重新下载包、增加 C 盘占用或暴露新的网络错误。当前只读快照中 C 盘空闲约 10.4 GB，执行前应重新核对。
- 风险控制：不删除旧链接、不猜测卷、不挂载、不改 ACL；任一原链接不能作为目录项安全改名时停止。
- 回退：关闭全部 Unity 进程；把新普通目录改名保存为带时间戳的 generated-cache 目录；再把两项 `*.pre-recovery-20260919` Junction 改回原名。新旧数据均不删除。
- 权限：用户 LocalAppData 通常无需 Windows 管理员权限；但 Codex 当前沙箱对该路径只有读取能力，实际 E02 仍需明确授权外部写入。这不等同于要求修改 ACL。

不推荐方案：

- 不推荐挂载两个无盘符卷或猜测其中之一为目标，因为没有映射证据。
- 不推荐修改 ACL，因为错误发生在设备/挂载点解析阶段，目标 ACL仍未知。
- 不推荐删除或直接重建 Junction，因为会丢失关键恢复证据且回退困难。

## 5. 工时与门禁

- 开始：`2026-09-19T21:55:40.6694777+08:00`
- 结束：`2026-09-19T22:03:04.7447298+08:00`（证据收集与报告初稿）；随后仅进行交付完整性复核。
- 实际有效工时：约 **0.17 小时**（约 10 分钟，含交付复核），未超过 0.5 小时上限。
- 未重置 T003 历史预算；剩余额度不构成 E02 自动授权。

门禁结论：

- `T003-E01` 只读诊断材料已形成，但**环境尚未恢复**。
- `T003-E02` 仅可在 00 总控审阅并批准上述对象、影响与回退后执行。
- `T003-02A-R1` 继续锁定；只有 E02 环境验证完成后才可复跑一次。
- `T003-02B`、T003、T004 继续锁定。

## 6. 证据索引

- `artifacts/external/T003-E01/00-preflight.txt`
- `artifacts/external/T003-E01/01-reparse-and-access.txt`
- `artifacts/external/T003-E01/02-volume-and-environment.txt`
- `artifacts/external/T003-E01/03-related-record-search.txt`
- `artifacts/external/T003-E01/04-proposed-recovery-plan.txt`
- `artifacts/external/T003-E01/05-postcheck.txt`
- `artifacts/external/T003-E01/evidence-manifest-sha256.tsv`
