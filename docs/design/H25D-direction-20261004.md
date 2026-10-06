# H25D：WASD 2.5D 路线决议（2026-10-04）

授权：用户明确要求按“三维场景构件＋二维像素人物＋固定斜俯视镜头”建议调整计划、安排任务，并交由对应会话执行。00只负责规划、跨模块接口、审计与状态，不再代替模块日常开发。此次批准独立验证与阶段规划，不等于既有M0/T003/盲测通过。

## 成品方向与最小验证

Windows单机、WASD地面八向移动，镜头相对方向：W往屏幕内上方、D往右，斜向不加速；E交互。固定斜俯视镜头呈现三维建筑，角色采用二维像素Sprite。调查优先、规则不随玩家猜测改变，事实与推测分开；既有事件规则保持。

候选技术：复用Unity6000.5.10f1、现有渲染管线和角色PNG；三维XZ地面、Y向上、1单位暂按1米。固定正交镜头初值俯角35度/水平45度/orthographicSize6，参数可调，不能把候选值冒充全项目美术标准。人物可见高初值1.7米、门净开口1.2米/高2.2米；先验证比例再冻结。二维角色的视觉子节点朝向镜头且脚底稳定，碰撞体保持竖直。

P1只做地面、两面墙、一扇门、一处可绕行前景遮挡及一个静态角色。灰盒几何足够；不先做整栋楼/全套动画/城市生成器。门独立开关、关闭阻挡且打开能通过。前景墙优先使用简单显隐；没有需要不实现复杂透明Shader或剖切系统。

选择混合3D可保留建筑拼装与空间编辑能力；继续纯预渲染适合固定场景但编辑受限；全3D人物增加当前动画与美术成本，因此后两者本批不实施。二维像素人物和3D材质的最终风格一致性仍需实际画面判断。

## 隔离、复用与冲突边界

主工程D:/projects/神秘复苏/Game保留为旧调查原型基线。新验证在C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/prototypes/hybrid25d-20261004；01/02各写自己modules目录，06唯一维护Validation/Game、公共配置与集成场景。00不写产品代码。无有效Git HEAD时以文件清单和SHA保护，不破坏性清理。

本批允许06仅在副本新增Unity内置com.unity.modules.physics 1.0.0；原工程仅列audio/imgui/screencapture。保持旧依赖，不迁移URP、不装Cinemachine/InputSystem或第三方包。若内置模块仍不可用，报告具体阻塞，不用手写全套碰撞替代。

01与02并行完成独立模块，不同时打开Unity；06独占运行后台验证和构建。保持用户“不操控游戏验证”的要求，不注入桌面键鼠、不抢前景。无图形烟雾不能证明视觉遮挡正确；可用安全离屏真实渲染则留原尺寸证据，否则视觉明确未覆盖，交用户查看演示包。

BLD-13–15：候选代码、Editor949及新EXE949自动检查和静态审查已有证据；尚未同步主工程、尚未完成ZIP。停止自动晋升旧二维墙体候选，保留现有目录与日志供06归档，不为迁移路线继续补旧包。已有模型可作为源零件复用，禁止覆盖。

## 模块接口（仅P1）

命名空间MysteryRevival.Hybrid25D。01提供HybridPlayerFactory.Create(Transform parent, Camera view, Sprite sprite, Vector3 spawn)返回GameObject；其HybridPlayerMotor暴露public void Step(Vector2 input,float deltaTime)和public bool InputEnabled。实际Update读取WASD调用同一Step；向量先限幅再投影到镜头地面方向，使用CharacterController移动，禁止靠Transform穿墙。静态正面资产可以验证移动，明确未验证八方向动作。

02提供HybridWorldFactory.Create(Transform parent)返回世界根。直接子对象ViewCamera含Camera；Spawn是脚底出生点；Door含HybridDoor组件，public void SetOpen(bool open)，门叶显隐和阻挡同步、门框保持。世界可有简单E附近交互，由02负责一处门，不接入事件规则。运行时模型可先用简单几何组合；同一墙件重复实例化。

06通过以上入口组合，不改01/02源实现；导入源文件和meta保留身份，记录SHA。02的遮挡脚本以public Transform target接收01角色，不反向引用其控制器；由06赋值。生命周期需能销毁重建，不能重复生成Player/Camera/AudioListener。

## 判定

P1通过需同时覆盖移动/碰撞/门/重建的行为证据和实际尺寸的角色、门口、遮挡视觉证据。只有后台通过则写技术子项通过、视觉未覆盖；不启动量产。P2再建立可复用建筑零件库；P3迁移调查闭环；P4形成小区域垂直切片。每阶段由00审计、状态表放行，M0b/M1原有玩法门槛不被替换。

2026-10-04接收接口补充：P1门的玩家入口由02实现，HybridDoor.interactor（public Transform）由06注入；TryInteract()（public bool）与Update中的GetKeyDown(E)共用，XZ半径1.6米；SetOpen保留用于明确设定状态。
