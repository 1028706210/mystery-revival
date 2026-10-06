# 神秘复苏：项目工作区

2026-10-04当前优先：WASD自由移动的2.5D验证，采用三维场景构件＋二维像素人物＋固定斜俯视镜头。旧D盘工程保留为调查规则基线；新验证目录为C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/prototypes/hybrid25d-20261004。01/02模块实施，06独立集成，00规划与审计。当前计划：[H25D任务包](D:/projects/神秘复苏/Docs/tasks/H25D-01-dispatch.md)，状态以[D盘TASKS](D:/projects/神秘复苏/Docs/TASKS.md)为准。旧BLD-13–15未同步/未打包，保留候选，不标为交付。


当前状态（2026-10-02）：M0a技术原型工程与内部Windows交付已收尾。用户取消继续操控游戏补验；未执行检查保留未覆盖，T003整体与真实盲测未宣布通过。

- [设计基线（90项，无完整规则）](docs/GDD.md)
- [M0/M1路线图](docs/Roadmap.md)
- [任务状态与交接](docs/TASKS.md)
- [验收方案](docs/Acceptance.md)
- [T001文档核验包](docs/tasks/T001.md)
- [T001外部核验报告](docs/reviews/2026-09-10-T001-external-verification.md)
- [T002无剧透审计单](docs/reviews/T002-public.md)
- [T002参数提案（审计专用，含机制参数）](docs/reviews/T002-params-public.md)
- [T002-R1审计修订基线](docs/reviews/2026-09-13-T002-R1-revision-baseline.md)
- [首轮执行记录](docs/reviews/2026-09-10-execution.md)
- [T001外部核验修订说明](docs/reviews/T001-remediation-2026-09-11.md)
- [T001第二轮报告](docs/reviews/2026-09-11-T001-external-round2.md)
- [T001限定范围放行记录](docs/reviews/2026-09-11-T001-release.md)
- [决议证据索引（审计专用）](docs/evidence/README.md)

`docs/internal/` 含历史和设计剧透，不作为用户默认阅读入口。GDD及`docs/evidence/`也不是玩家或盲测材料，不得加入发布构建。
`docs/tasks/T003.md` 已按新授权更新：T002规格自检完成后可由本任务启动M0a。执行记录见[本轮接管记录](docs/reviews/2026-09-14-takeover.md)。

当前工作批由总控直接收尾，不再派发外部执行体或等待游戏操作。自测与文件完整性检查的范围见收尾记录；M0b与后续扩展未启动。


上一版内部试玩包（保留）：[Windows ZIP](artifacts/external/EXT-03B/2026-09-28-batch-validation/mystery-revival-internal-20260928.zip)。现有运行目录：`artifacts/external/EXT-03/2026-09-27-runtime-build/build/MysteryRevival.exe`。这是Development构建；操作见[PLAYTEST.md](PLAYTEST.md)。当前结论与未覆盖项见[工程交付收尾记录](docs/reviews/2026-10-02-M0a-engineering-closeout.md)。旧`artifacts/build/`保留为历史包；启动器哈希相同不代表两包内容相同。


2026-10-02最新内部交付：建筑门显示接入v0.3已同步本工程；门叶开关、水痕和人物遮挡关系已后台验证。Windows构建为Development；实际画面/声音与盲测未覆盖。下载和源资产入口：[本批交付说明](C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/building_samples/game-door-integration-20261002/README.md)。旧试玩包保留，T003/A001整体及后续阶段状态不变。


2026-10-03最新内部交付v0.4（2026-10-02工作批）：在v0.3门模块基础上新增中央带窗墙，实际Unity主工程仍为`D:\projects\神秘复苏\Game`。新Windows与可编辑工程资产：[交付说明](C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/building_samples/corridor-window-integration-20261002/README.md)。旧版本保留，实际观感及阶段整体未宣称通过。


2026-10-03离线建筑样本v0.5：标准实墙、门窗间补齐墙及左右户门/中央窗组合已交付。资产包含可编辑Blender源模型、三张透明图和保存模型重导脚本；保存模型45项、图片55项、ZIP全部9成员和解压重导55项实际核验通过。入口：[样本说明](C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/building_samples/corridor-wall-kit-20261003/README.md)。本批未修改Game/tests/tools，未运行Unity或操控游戏；运行版本仍为v0.4。这是离线候选交付，A001/T003整体与T004/M0b/M1门槛保持。
