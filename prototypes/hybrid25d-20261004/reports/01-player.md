# H25D-01 玩家模块执行记录

状态：模块实现与本地核验已交付；待00核对和06实际集成。起始记录约2026-10-04 20:16 America/Los_Angeles；首轮60分钟有效工作止损，历史投入未知、未归零，等待另记。

依据：D盘 H25D-01-dispatch.md 与 H25D-direction-20261004.md。新模块目录独立且此前不存在；C工作区无有效提交基线，已有未跟踪文件保留。

流程适配：按 executing-plans 顺序执行，记录与证据保留在本模块和本报告。用户指定仅写模块/报告，故不使用技能默认的额外Git工作树、公共账本或提交步骤；06按合同负责独立运行验证，不新增审查子代理。

接口预检：01的 Create(parent,view,sprite,spawn) 由06组合02的 ViewCamera 和 Spawn；02遮挡接收 Transform，不引用01控制器。未发现合同冲突。

最新输入：D:/projects/神秘复苏/Game/Assets/Resources/nightwalker_idle_front.png，SHA256 9A594C49AB73C96D6017D060034905D1A94FDC53D51947088DB1F5C2DD8B5C3A，2026-10-04重新读取；未沿用历史报告作验证。

## 修改内容与原因

- HybridPlayerFactory.Create(Transform parent, Camera view, Sprite sprite, Vector3 spawn) 返回玩家根对象和子对象；不创建Camera/AudioListener/纹理/持久对象。销毁返回根对象即可清理模块。
- HybridPlayerMotor提供public void Step(Vector2 input, float deltaTime)和public bool InputEnabled。Update读取WASD并调用同一Step；先限幅输入，再使用归一化镜头XZ地面基底，斜向同速。位移只经CharacterController.Move；根朝向仅绕Y轴转向输入方向。
- InputEnabled=false时Step冻结全部位移并清空竖直速度；零输入仍允许重力贴地。速度暂取3米/秒。胶囊高1.7米、半径0.22米、中心Y=0.85、skinWidth=0.02、stepOffset=0.25、slopeLimit=45；几何底部在根点Y=0。1.2米门口有足够几何宽度，实际通行需Physics验证。
- HybridPlayerVisual在LateUpdate让视觉子节点的世界旋转对齐ViewCamera，胶囊保持竖直。PNG脚底(24,8)与导入pivot的差额在图像子节点抵消，视觉旋转不搬动脚底根点。
- 复制当前PNG至Art/nightwalker_idle_front.png。实时像素读取为48×96，非透明bbox=(14,16)..(33,87)，可见20×72、底部透明8px、半透明像素0。PNG哈希匹配；副本meta仅换新GUID，Point/FullRect/PPU100/Pivot(0.5,8/96)/无mipmap/无压缩沿用当前配置。换GUID避免06复制旧Resources后与副本冲突。

Ruling：1.7米指Sprite平面上Alpha高度，缩放=1.7×PPU/72（PPU100时约2.361111）。全朝向镜头的Sprite和竖直胶囊在斜俯视下的投影需06按门口实际画面校核；比例未冻结。单张正面保持镜头朝向，八向位移不代表八方向动作或Walk动画。

## 06集成说明

复制本目录4份C#及meta、Art目录及meta。Checks~为Unity忽略的自检目录，可保留复查；Checks~/bin是可再生成产物。外层Player目录meta由06在自身导入目录生成，本任务没有写入Player白名单以外。

06使用02的ViewCamera/Spawn，将本模块Art中的正式Sprite通过序列化引用或自身加载入口交给Create；工厂不依赖旧PrototypeGame、Resources路径或世界控制器。父节点使用单位缩放；尺寸按1单位≈1米。返回根对象交给02遮挡组件的target。批量Step检查时可令motor.enabled=false阻止Update再次读取键盘，public Step仍可被检查入口调用；InputEnabled=false则冻结Step。

## 验证结果与原始证据

- **通过：生产移动计算16/16。** 引用真实Unity CoreModule类型，没有手写Unity替身。检查W/D/S/A、四斜向、零输入、模拟量、越界输入、俯角、垂直镜头退化、非法输入与速度。零实现阶段4通过/12失败、退出1；实现后16通过/0失败、退出0。初次脚本参数错误已修正，该错误未充当测试红灯。
- **通过：完整4份运行C#的托管API编译。** 使用Unity6000.5.10f1自带Roslyn、NetStandard2.1参考和Core/Physics/InputLegacy模块；退出0，无编译诊断。未启动UnityEditor。
- **通过：当前输入像素/哈希、模块meta唯一性、受保护输入前后哈希。** 只写自身模块和报告；旧PrototypeGame、PNG及PNG meta前后SHA相同。
- **已完成作者静态复查；独立运行验收未覆盖。** 复查接口、CharacterController位移、根与视觉朝向分离、对象归属和PNG脚底补偿。

复现命令（PowerShell）：

```powershell
& 'C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/prototypes/hybrid25d-20261004/modules/01/Assets/Hybrid25D/Player/Checks~/check-player.ps1'
```

脚本只运行C#编译器和纯托管逻辑程序；默认使用D:/Unity/Editors/6000.5.10f1/Editor/Data，可用-UnityData指定同版本安装。原始记录：Checks~/red.log、Checks~/green.log。最后输出PLAYER_LOGIC passed=16 failed=0及MODULE_COMPILE passed。

| 受保护D盘旧文件 | SHA256（前后相同） |
|---|---|
| Game/Assets/Scripts/PrototypeGame.cs | BB66A2260A03E80A80ED084CC58D245C2BEC43B6F7B677BB6655E60F473E099D |
| Game/Assets/Resources/nightwalker_idle_front.png | 9A594C49AB73C96D6017D060034905D1A94FDC53D51947088DB1F5C2DD8B5C3A |
| Game/Assets/Resources/nightwalker_idle_front.png.meta | 61A30F81E1DD646A906148F324E0B314F6C5816A36FA52CA3791372F0B49C692 |

## 未覆盖项与剩余问题

Unity实际导入、CharacterController墙体/门口通行、Step暂停与重力运行、真实WASD键盘、三次重建计数、1366×768/1920×1080脚底/辨识/遮挡画面、新Windows演示均未覆盖。06在独立工程按H25D-03放行后执行；01未启动Unity。单张Idle没有Walk或八方向姿态。

无已知接口阻塞。比例和Sprite/3D深度关系需06实际渲染校核；托管编译不能证明包配置、引擎导入或碰撞运行成功。不得据此放行P2或旧T003。

## 文件清单/SHA256

相对路径基于modules/01/Assets/Hybrid25D/Player；Checks~/bin产物不列作交付源码。

| 文件 | SHA256 |
|---|---|
| Art.meta | 1CDCA170FFF86A0B4FB83DE0E1F6721F56610E657A145428818537F15AED6727 |
| HybridPlayerFactory.cs | D967E4C935258C9BC4FD58A646444B73EBEF592B129C980BC000A3AB60DFCA4B |
| HybridPlayerFactory.cs.meta | A6CFF5C1D63FB2FE8D76195699C90030EBD5F08F839C8417DC0E627BE2F8E5FB |
| HybridPlayerMotor.cs | 57F55C7E16949FE66A51802ECF57D051E34A394882E660B2D83010134ED6D8A6 |
| HybridPlayerMotor.cs.meta | 965900F6145E5E956A7564767693444C91B29987533185C75ADE9579A32C5C8E |
| HybridPlayerMovement.cs | 7C87F0BADC22251EDA762F0BB10E8DF6A61DE9F5EFD857558A7561719AB556B4 |
| HybridPlayerMovement.cs.meta | F22A196D10840A77EC79BF1BFB0E5CD0CD4AA7C6A2ACA3AB54752CF7FC80E071 |
| HybridPlayerVisual.cs | 31300B6426F6B6F2B3D551176ADD263603060048FB9A51281EF1F9A6969DF1BA |
| HybridPlayerVisual.cs.meta | 2D142F5E2D252CF53BCAEB17A8549D21CA37A218B57CB36DAB8EC8B6B5671A78 |
| Art/nightwalker_idle_front.png | 9A594C49AB73C96D6017D060034905D1A94FDC53D51947088DB1F5C2DD8B5C3A |
| Art/nightwalker_idle_front.png.meta | D7D372F695EC2FF061151BF0F77F411CC365D335AE5C4F0FA058C1653C7B70BC |
| Checks~/check-player.ps1 | EBA2F31BB76429B531B788BCB4FD60393C99987952D186617365799A2B46C525 |
| Checks~/green.log | 3E7E00BB81CBE683B6315D4A6624E5F4B3F430DA3D5FBB5322B97E36394B0E42 |
| Checks~/PlayerLogicChecks.cs | D20DF815C04854594188F05826D78B22D0BC97843CDFC77DE370A3FABB5E04CE |
| Checks~/PlayerLogicChecks.runtimeconfig.json | D15B2D4A36063C1F213AA6F028B74CD13315C80F39FEDA299B7809EC32096E56 |
| Checks~/red.log | 6D3BB7C70F04A970105B193333C69A11C2B5AB4D198EEF28199AF4C86A9F0FEB |

## 成本与交付状态

启动记录约20:16，交付清单回读核验20:33:36（2026-10-04，America/Los_Angeles）；本轮按整段墙钟保守计约18分钟有效工作，外部等待0，未触及60分钟止损。历史投入未知、未归零；账户剩余额度未查询。报告内16份文件哈希已逐项回读一致；首次清单核验的预期总数误写为17，纠正为实际16后退出0，没有发现文件哈希不符。

模块实现及本地核验已交付，等待00核对和06实际集成。报告由00读取；本会话不改公共状态、不跨任务发消息、不自动启动后续任务。
