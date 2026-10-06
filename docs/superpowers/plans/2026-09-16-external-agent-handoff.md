# 外部 Agent 剩余任务交接计划

日期：2026-09-16。状态：任务包已准备，未派发、未启动。用户决定恢复外部Agent执行。本文件不表示任何阶段通过。

> 执行方式：按任务逐项执行，有相应技能时使用superpowers:executing-plans；没有技能可按本包执行，不要求安装插件或启用多Agent。不得自行并行修改共享工程。

**Goal：** 收尾M0a技术验证，经独立验收后组织真实盲测；单独完成D盘迁移收尾。
**Architecture：** 保留纯C# IncidentState与Unity表现层，优先修复现有实现，不建立通用规则框架。
**Tech Stack：** Unity 6000.5.10f1_3bd4f66ad299、C#、Windows、PowerShell。
**Spec：** docs/internal/T002-design-audit.md、docs/Acceptance.md、docs/Roadmap.md；内部规格含剧透，禁止转发给盲测者。

## 当前基线与全局边界

- 交接目标根目录：D:\projects\神秘复苏。Unity工程：其下Game。
- 原Codex任务仍在C:\Users\李帅朋\Documents\ChatGPT\神秘复苏；两处是副本，不是实时同步。外部Agent开始后仅以核对后的D盘项目为工作目录，C盘保留，不双向覆盖。
- 先核对D盘AGENTS.md、git status、文档与核心代码；不得假设C/D的Git分支或提交历史相同。源代码此前未形成可依赖的提交基线，以当前文件哈希建立接管记录。
- 2026-09-16最后同步339个文件SHA256一致。已有备份D:\projects\神秘复苏-sync-backup-20260916-183702和-184002。这里是历史证据，接手时仍需复核关键文件。
- 已构建普通人三层楼原型，2个NPC，可拒绝取件、撤离、窗观察、粉笔、开门确认、事实档案与自由笔记、失败及原型重开。
- 已有纯C#24测试、Windows75方法调用断言、Editor75断言及真实截图。历史通过不代替独立复跑；方法调用不证明键盘/鼠标/输入法正常。
- Editor启动索引存在UnityEditor.Search.SearchDatabase的ArgumentOutOfRangeException，尚未解决或正式接受。
- 当前T002仅规格自检；T003待验收；T004未开始；T005–T008、A001未启动。
- 不改变已锁定世界观、危险输入或关键规则；普通人可求生，能力必须有代价，笔记不参与判定，UI不泄露真值。
- 不升级/另装Unity，不添加第三方依赖、不大规模重构，不发布、不删除C盘原件、迁移备份或历史证据。
- 历史净工时与剩余额度未知，不能填零或声称预算合规。先完成只读接管，列出修复估时与预算缺口；进入新的开发计费批次前由用户明确额度，沿用Roadmap止损条件，不能自行重置旧预算。
- 返回结果统一格式：检查项｜实际操作或命令｜通过/失败/未覆盖｜证据路径｜影响范围。记录完整版本、变更文件、开始结束及有效工时，等待时间另记。

## 任务A：M0a技术收尾（首先发送）

可复制给执行Agent：

```text
你接手神秘复苏项目的M0a技术收尾。工作目录D:\projects\神秘复苏。先阅读该目录AGENTS.md和docs/superpowers/plans/2026-09-16-external-agent-handoff.md，执行其中任务A，不自动启动后续任务。

读取：docs/TASKS.md、docs/Acceptance.md、docs/Roadmap.md、docs/reviews/2026-09-16-M0a-technical.md、docs/internal/T002-design-audit.md、Game/ProjectSettings/ProjectVersion.txt、Game/Packages/manifest.json与packages-lock.json。

检查现状后提出最小修复清单并执行已获授权、符合成本边界的部分。不得重写现有原型。具体步骤、证据和停止条件按任务A清单。历史报告只能作线索，不能作本次通过证据。完成后交报告与构建，等待独立验收，不自批T003通过。
```

工作文件：Game/Assets/Scripts/PrototypeGame.cs、PrototypeSmoke.cs、Game/Assets/Editor/PrototypeBuild.cs；只有复现确实指向核心规则时才改Domain/IncidentState.cs及tests/IncidentStateTests.cs。最小回归可放现有测试文件；禁止为凑数量增加镜像断言。

- [ ] 建立接管记录：实际工作路径、Git状态、关键文件SHA256、Editor完整版本、现有进程与构建入口。报告C/D差异，不直接覆盖。
- [ ] 复跑tools/Test-Core.ps1，保留新日志；现有LogName参数可用于独立命名。
- [ ] 复现Editor搜索索引异常：确认发生阶段、频率和对编辑/Play Mode/构建的影响。使用最小可回滚处理；禁止通过吞异常、删证据或升级Editor宣布修复。若属引擎问题且无法本地修复，提交证据和影响，留给验收者决定，不能自行豁免。
- [ ] 在可见Windows窗口实际操作WASD、E、鼠标选择、J、Tab、Esc、物品；中文输入法输入含j的拼音、组合中按Esc、提交后继续输入；证明笔记不中途关闭、不触发移动或推进事件。无法控制真实UI时记录未覆盖，不用State.Notes直接赋值代替。
- [ ] 实测开门取消、确认中暂停、两方向门槛、房内侧墙、拒绝委托后取件、重复取件、撤离再进入、死亡回顾与重开。核对画面反馈和事实记录一致，区外无威胁声画，暂停冻结模拟及事件音频。
- [ ] 核对两条证据路线。当前楼梯是交互切层，文字声称可数楼梯但实际移动表达仍简化：必须证明台阶/标记路线可以被观察与复核。若实现不符合已定规格，做最小场景/交互修复；不得只改验收文字来掩盖差距。窗路线应不依赖NPC或粉笔，三窗记录来源可识别且不泄露实际楼层。
- [ ] 检查1366×768、1920×1080及窗口缩放下档案、按钮和长文本；记录Windows显示缩放。检查声画是否可辨，包括首次异常、不同反馈和暂停。
- [ ] 对复现缺陷先保留失败步骤或针对性失败测试，再最小修复、复跑相关路径；最后进行一次完整核心测试、Editor Play Mode、Windows构建和可见窗口回归。
- [ ] Windows构建入口为MysteryRevival.PrototypeBuild.Build，Editor路径入口为MysteryRevival.PrototypeBuild.SmokeEditor并带--smoke；Editor自动路径不带-quit，由检查完成退出。Windows带--smoke自动检查；MYSTERY_SMOKE_OUTPUT指定新证据目录。先核对方法仍存在，不机械照抄旧命令。
- [ ] 运行程序须等待真实退出并记录ExitCode，不以启动器返回0判成功。截图为空/全黑不得标视觉通过；已验证过可见窗口Direct3D11，其他后端结果分别记录。
- [ ] 输出docs/reviews/T003-external-fix-report.md；证据放artifacts/external/T003/；更新PLAYTEST.md、TASKS.md为“待独立验收”。附可复现命令、原始日志、截图/操作录像、版本和未覆盖项。

完成标准：必要路径有实际证据，已知错误已修复或明确提交验收处理；构建可运行。没有真实输入证据或路线仍与规格不符，报告局部完成并列缺口，不能全绿。发现需要修改批准的机制时停下依赖实现，提交具体冲突，不扩展到M0b。

## 任务B：独立技术验收（A提交后发送给另一Agent/审查者）

```text
请独立验收D:\projects\神秘复苏的T002规格与T003技术交付。阅读AGENTS.md、docs/superpowers/plans/2026-09-16-external-agent-handoff.md中的任务B、Acceptance、内部事件规格、T003-external-fix-report。

不得直接采信执行者报告。默认只读审查源码和文档，可将重新构建、测试缓存和新证据写入隔离输出位置；不得修改待验收源码或覆盖原报告。独立运行核心测试、Editor、Windows构建及真实交互，核对任务A每个验收点与全部必要M0技术项。对Editor索引异常作出有依据的处理结论，未覆盖不能写通过。

将结果写入docs/reviews/T003-independent-acceptance.md，逐项给出通过/失败/未覆盖与原始证据。整体结论只能是通过、返工或证据不足；发现问题交回执行者，自己不边修边验。T003通过最多允许进入T004，不放行能力、存档或街区。
```

证据目录：artifacts/external/T003-review/。若同一Agent修复了缺陷，该部分须交另一审查者复验，不能继续称独立验收。

## 任务C：真实盲测准备与结果整理（B通过后）

```text
请为T004准备不剧透的真人盲测，工作目录D:\projects\神秘复苏。先核对T003-independent-acceptance.md确实通过。只准备构建交付包、操作说明和记录表，不自动联系任何人。

按docs/Acceptance.md的M0盲测要求，创建docs/playtest/T004-player-guide.md（玩家可见）和docs/playtest/T004-observer-sheet.md（观察员使用），以及docs/reviews/T004-results.md空白结果模板。玩家包只包含运行必需文件和操作说明，不含源码、内部规格、技术测试日志或规则答案。

由用户邀请至少3名未读规则者实际试玩；你不能用AI模拟玩家、生成评价或补造测试记录。没有测试者时明确报告“准备完成，试玩未覆盖”，停在这里。收到真实记录后汇总结果与修订建议，不代填通过。
```

验收原值：每人首次10分钟，最多30分钟；至少2/3在入区5分钟内指出具体异常；至少2/3基于证据形成假设并作出有效选择；两条路线均独立走通（额外引导路径测试不能冒充首次盲测）；无必须死亡取得的关键证据。已读规则的用户/开发者不算首次盲测者。同一人重测不算新的首次盲测。

失败时回到最小事件修订；同一核心问题两轮修订仍不成立，回到设计审计，暂停扩建。T004通过并完成能力审计后才可准备T005。

## 任务D：D盘迁移收尾（独立维护窗口）

```text
请接手D盘迁移收尾。阅读D:\projects\神秘复苏\tools\migration\README.md与Move-CodexData.ps1，先审查并做只读检查，不立即改活动数据。

确认项目关键文件和构建位于D盘，核对AGENTS.md与Git状态；通过所用Agent/应用的正常打开项目功能选择D:\projects\神秘复苏，确认后续实际工作目录为D盘。不要通过编辑Codex数据库来改路径；保留C盘副本及备份，不继续双向同步。

Codex数据迁移必须在Codex和其相关进程完全退出后，从独立终端或不依赖Codex数据目录的外部工具执行。不能在活动Codex会话中启动等待后自动迁移的后台任务。先运行CheckOnly，核查目标D:\CodexData、NTFS、权限、磁盘余量、重解析点及脚本行为；目标已存在或访问被拒绝时停止，不覆盖、不跳过受保护文件、不扩大源权限。

在用户已退出应用且检查满足后，用已审查脚本复制、核对并切换，保留原始备份。重新启动由用户配合检查登录、旧任务、配置、技能、下载目录和D盘项目。每一步分别报告“准备/复制/校验/切换/重启验证”，未执行不得写完成。不得删除备份、迁移MSIX应用本体或打印凭据内容。

输出D:\projects\神秘复苏\docs\reviews\migration-final.md，列出实际源/目标/备份/链接状态、未覆盖项及回退说明。项目路径切换与.codex数据切换分别验收。
```

可在独立PowerShell中使用（先审查脚本，进程条件不满足时不得执行第二行）：

```powershell
powershell.exe -NoProfile -File "D:\projects\神秘复苏\tools\migration\Move-CodexData.ps1" -CheckOnly
powershell.exe -NoProfile -File "D:\projects\神秘复苏\tools\migration\Move-CodexData.ps1"
```

不与游戏构建/工程编辑同一时间切换路径；迁移失败不阻止继续在已核对D盘工程做独立游戏工作。备份保留时不会立即释放原C盘数据占用。

## 后续剩余任务（仅登记，不是开工指令）

| 任务 | 范围 | 开工前置 |
|---|---|---|
| T005 / M0b | 同一事件一项能力及代价，再次验证调查价值 | T004通过、能力审计通过、明确任务规格与额度 |
| T006 / M1 | 世界/角色一致存档、崩溃恢复、永久死亡、新身份、区域结算、第二微型事件 | T005通过；按Acceptance逐项测试，不一次做五套框架 |
| T007 | 一片街区、3类半随机身份模板、必要NPC与最小世界后果 | T006通过；范围按Roadmap |
| T008 | 30分钟集成构建和连续游玩、回归、体验报告 | T007通过 |
| A001 | 一个男性模型、局部变化、四方向动作与最小走廊，实际返工导出一次 | 独立工时额度、工具与任务包明确，用户单独派发；不阻塞M0 |

后续任务应在前置证据产生后再细化，不让执行Agent猜测接口或提前开发。完整大世界、量产美术、多个城市及发行不在本交接开工范围。
