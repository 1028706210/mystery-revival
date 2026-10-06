# UI 可读性改良 / 2026-09-22

- 范围：PrototypeGame.cs 的相机、探索HUD、标签、全屏遮罩；GDD第59项。用户已明确授权改善角色可读性与界面。
- 全屏相机，正交半高3.5，LateUpdate有限界跟随；保持人物scale1.2、PPU100。按0.864世界单位高度，720p约89像素，1080p约133像素；这是理论投影值，不是截图验收结果。
- 探索HUD采用左上状态、右上地点和生活动机、底部粉笔/手电/档案/物品/暂停入口。删除厚框、右侧教学段落、重复头顶E提示。
- 场景标签只显示附近且位于视野内的对象，并避开HUD区域。菜单与死亡遮罩覆盖实际全屏。
- 移动、碰撞、规则、门与楼层切换、NPC大小、Sprite、中文笔记输入及菜单暂停逻辑保持原实现。
- 原版备份：C:\Users\李帅朋\Documents\ChatGPT\神秘复苏\ui-readability-20260922\PrototypeGame.before.cs
- 原版SHA256：AC9C35BA66378EAFDE1C2BF053984A68A4B9C8702C9D46E27F36EB5B7F2EB3FE
- 已做：哈希守卫、差异审查、GUI坐标与相机理论尺寸核查。git diff --no-index退出1代表存在预期差异。
- 未覆盖：Unity实际编译、Play画面、点击与键盘交互、16:9/16:10/4:3运行画面、Windows构建。未启动Unity或干预用户Editor。
- 风险：固定正面Sprite的特征上限受原图宽度限制；近景地图美术仍是占位。需要实际画面确认相机边缘和HUD位置，不能宣称视觉验收通过。
## 2026-09-26 总控实际验证与修正

本节更新上文的历史未覆盖状态，仅代表本次限定范围。

- 修复角色 PNG.meta 的字段错误：spriteAlignment 改为 Unity 序列化字段 alignment: 9，保留 pivot (0.5,8/96)。修复前粉笔出现在躯干附近，修复后实际 Play 中位于脚边；PNG 本身未修改。
- 修复重建后地砖被底色覆盖：Concrete sortingOrder=-2、Tile=-1，其余地图及玩法参数不改。
- 最终源码 SHA256：06EAFDC83D74FC154423767CCF08DF984D33122996C6ADBFE7A67CA4D093C0CA。
- 最终角色 meta SHA256：61A30F81E1DD646A906148F324E0B314F6C5816A36FA52CA3791372F0B49C692。
- 新增备份位于 C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/ui-readability-20260922/：nightwalker.before-anchor-fix.meta、PrototypeGame.before-floor-fix.cs。
- C# 编译：使用 Unity 自带 DotNetSdk 8.0.318 的 csc.dll 与已有 Assembly-CSharp.rsp 的引用/定义，输出重定向至工作区 validation.dll（实际文件名 Assembly-CSharp.validation.dll），去除分析器和附加生成文件；命令退出0。此项仅验证编译，不等于完整构建。
- 主 Editor 6000.5.10f1：刷新后实际编译日志 UI-validation-20260926.log 第430行记录 Tundra build success (1.19 seconds), 3 items updated, 167 evaluated；进入 Play 成功。
- 实际操作：进入游戏、W移动响应、E打开出口交互/Esc返回、J档案、Tab物品切换、粉笔快捷按钮触发场景重建、手电按钮开关与状态反馈、Esc暂停入口。上述限定检查通过；暂停计时数值、中文输入法、全方向碰撞与全事件链未重新验证。
- 最终版本重建后地砖仍可见，玩家仍显示且粉笔落在脚边；全屏场景与紧凑HUD无此前右侧说明栏。截图：UI-readability-play-20260926.png（真实Editor截图，非效果图）。
- 未覆盖：跨楼层/重进场景、16:9/16:10/4:3完整矩阵、Windows新构建、完整80项烟雾测试、独立06验收。没有宣布T003或其他阶段通过。
- 环境问题：Package Manager报告offline与旧缓存目录UNKNOWN错误；未修改全局缓存。虽然可进入Play，不能宣称Editor无报错。
- 视觉限制：单张静态角色、占位地图/NPC、现有IMGUI按钮仍是原型；本轮只确认可读性改良，不等于正式美术验收。
- 实际使用：Computer Use、systematic-debugging、完成前证据核查、PowerShell编译/哈希/文件验证。锚点字段参考 Unity官方仓库：https://github.com/Unity-Technologies/arfoundation-samples/blob/main/Assets/Textures/Circle.png.meta 。
