# CHAR-V01：夜行者 v0.2 单张 Idle 静态接入

日期：2026-09-22。执行会话：01 玩家与角色。范围仅为 D 盘 Unity 原型中的玩家静态外观；不包含 Walk、Animator、NPC 改造或玩法参数调整。

## 修改内容与原因

- 新增 `Game/Assets/Resources/nightwalker_idle_front.png`，使用已确认的正式 48×96 透明 Sprite，未覆盖桌面源文件。
- 新增 `Game/Assets/Resources.meta` 和 `Game/Assets/Resources/nightwalker_idle_front.png.meta`。导入候选为 Sprite Single、Point、Full Rect、PPU 100、Pivot (0.5, 8/96)、无 mipmap、无压缩。
- 修改 `Game/Assets/Scripts/PrototypeGame.cs`：启动时通过 `Resources.Load<Sprite>` 获取正式图；`BuildWorld()` 仅把玩家的 `DrawPerson` 调用换成 `DrawPlayer`，NPC 仍沿用原绘制。玩家根对象仍接收逻辑 `Position`，纳入现有 `worldObjects` 清理；资源缺失时记录错误并回退占位人物。显示缩放暂取 1.2，尚待可见 Play 校准。

使用 Resources 路径是为了在既有构建脚本重建场景后仍能加载 Sprite；本次未修改构建脚本、相机、移动、碰撞、交互或事件状态代码。

## 已执行验证

- 写入前确认 `PrototypeGame.cs` SHA-256 为 `6E3A9C55BC7381402D63168B90ECDAAA2F7D3F473D36CDC9FD6D2458C12AE7EB`，`Resources` 及其 `.meta` 均不存在；不匹配时写入脚本会停止。提升权限写入获批准，命令退出码 0。
- 写入后重新计算哈希：`PrototypeGame.cs` 为 `AC9C35BA66378EAFDE1C2BF053984A68A4B9C8702C9D46E27F36EB5B7F2EB3FE`；正式 PNG 为 `9A594C49AB73C96D6017D060034905D1A94FDC53D51947088DB1F5C2DD8B5C3A`；PNG `.meta` 为 `5FA1CC0860AAA53D46519FAE030157879A5A4529A4668FB78D28E3C255A2D919`；目录 `.meta` 为 `B382D747650EDDB79AF6F34CC30FAB8F0FB242ED3DC2309841A7FFC8BB3F98E4`。
- 读取导入 `.meta`，确认上述设置已写在文件中；这不等于 Unity 已成功导入。
- 本次没有可用的主 Unity 进程；主工程 `Game/Logs/Editor.log` 最后修改于 12:29:04，不能作为本次改动后的编译证据。

## 未覆盖项与风险

- Unity 对新 `.meta` 的实际导入、C# 编译、可见 Play Mode、玩家唯一性与 `BuildWorld` 重建、脚底位置和 1.2 倍比例、WASD/E、Windows 新构建：**均未覆盖**。
- 隔离副本的 Editor 尝试在测试前被许可及 UPM 缓存访问问题阻断；不是测试失败结论。未再启动 Unity 或修改全局缓存。
- 没有制作独立的修改前源码备份；只记录了修改前哈希，原文件不可从本报告直接恢复。隔离草案位于 `C:\Users\李帅朋\.codex\visualizations\2026\09\19\01a0b9a7-f8a3-7a22-bbe1-3338999d847f\char-v01-validation\Game`，其中 `Assets/Scripts/PrototypeGame.cs` 是**修改后草案**，不是原版备份。
- 这只是静态接入落盘，不是 06 独立验收，也不放行 T003 或后续动画任务。需由 00 安排主工程导入/可见 Play 和 06 回归。
