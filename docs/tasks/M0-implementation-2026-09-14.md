# M0原型执行计划

日期：2026-09-14。用户已授权本任务接手开发并选择可调整原型默认值；外部执行器暂停。既有世界观与真实盲测门槛保留。

目标：交付一个可运行的普通人调查原型与可复现的规则验证，不宣称十分钟恐怖体验已成立。

实现边界：纯C# IncidentState处理事件状态；Unity MonoBehaviour处理2D占位场景、交互、输入与极简UI。静态场景用代码构建，不创建通用框架、不下载美术包。Game/Assets/Scripts/Domain与表现层分开。

- [x] T002：补齐输入/输出、普通目标、两路证据、暂停与退出语义；原型数值列为可调默认，自检不冒充外部验收。
- [x] 核心测试：先运行失败测试，再实现动作去重、冷却、死亡、暂停、观察记录，保留原始结果。
- [x] Unity接入：固定身份、三层室内与出口、两NPC、观察与普通标记、窗参照、开门声明/确认、档案与自由笔记。
- [x] 构建验证：锁定经用户确认的已安装Editor，实际运行Editor批处理检查与Windows构建。
- [ ] 原型运行检查：自动行走与交互路径、UI暂停、取消、危险反馈及截图；报告自动路径和人工盲测的区别。
- [ ] 交付：README试玩入口、操作、日志、已知缺口。T004真实盲测需未看过内部规则的测试者，不自动标通过；M0b及M1依赖该门槛。
- [ ] 迁移收尾：校验D副本、准备退出后数据切换，保留可恢复原件；实际切换与准备分开记录。

新增主要文件：Game/Assets/Scripts/Domain/IncidentState.cs、Game/Assets/Scripts/PrototypeGame.cs、Game/Assets/Editor/PrototypeBuild.cs、Game/Assets/Scripts/PrototypeSmoke.cs、tests/IncidentStateTests.cs、tools/Test-Core.ps1。无已有工程或可复用游戏代码。

本批次于2026-09-14 10:02 UTC开始记录；历史投入未知。墙钟跨度不作为净开发工时。规则默认与构建版本由本批次记录追溯，不推称旧预算合规。

2026-09-16：Windows构建及75项自动交互完成；Editor同路径完成但存在搜索索引异常。真实输入、盲测与完整独立验收未覆盖。技术记录见../reviews/2026-09-16-M0a-technical.md。

