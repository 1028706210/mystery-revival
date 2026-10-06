# 神秘复苏：项目工作区

当前阶段：2026-09-14用户授权本任务接管M0a原型开发。T002可执行规格已补齐并自检；技术结果与真实盲测尚需证据，未宣布阶段通过。

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

执行分工：本轮由本任务开发及自测，用户审计内容与试玩；自测不冒充独立验收。仍需独立技术复核及未读规则者的真实盲测，M0b与后续扩展不自动启动。


历史试玩入口（保留）：artifacts/build/MysteryRevival.exe。操作见[PLAYTEST.md](PLAYTEST.md)。2026-09-16技术结果与未覆盖项见[交付记录](docs/reviews/2026-09-16-M0a-technical.md)。T003待验收，T004尚未开始。


2026-10-02当前工作入口：实际Unity主工程为`D:\projects\神秘复苏\Game`，任务单一状态表为[D盘TASKS](D:/projects/神秘复苏/Docs/TASKS.md)。本目录保留历史工程副本和建筑源资产，不用这里旧Game副本打开本次修改。最新门模块内部包及可编辑工程资产：[交付说明](C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/building_samples/game-door-integration-20261002/README.md)。


2026-10-03最新内部交付v0.4（2026-10-02工作批）：在v0.3门模块基础上新增中央带窗墙，实际Unity主工程仍为`D:\projects\神秘复苏\Game`。新Windows与可编辑工程资产：[交付说明](C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/building_samples/corridor-window-integration-20261002/README.md)。旧版本保留，实际观感及阶段整体未宣称通过。


2026-10-03离线建筑样本v0.5：标准实墙、门窗间补齐墙及左右户门/中央窗组合已交付。资产包含可编辑Blender源模型、三张透明图和保存模型重导脚本；保存模型45项、图片55项、ZIP全部9成员和解压重导55项实际核验通过。入口：[样本说明](C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/building_samples/corridor-wall-kit-20261003/README.md)。本批未修改Game/tests/tools，未运行Unity或操控游戏；运行版本仍为v0.4。这是离线候选交付，A001/T003整体与T004/M0b/M1门槛保持。
