from pathlib import Path
from datetime import datetime,timezone
import hashlib,json
R=Path(__file__).resolve().parent
D=Path('D:/projects/神秘复苏/Docs')
report='''# H25D 首批接收审计（2026-10-04）

00只做交接审查，没有修改模块、运行Unity或代替06集成。依据用户通知与三个既有会话的实际交付，已读取运行源码、接口、原始离线结果和当前SHA。

| 任务 | 接收结论 | 证据与边界 |
|---|---|---|
| H25D-00 / 06 | 通过（基线隔离范围） | 62项D盘源重新散列一致；副本仅manifest/packages-lock两个批准差异；Physics内置配置新增且旧依赖保留。Unity导入/行为仍未覆盖 |
| H25D-01 / 01 | 通过（模块交接范围） | 报告16项SHA实查一致；Create/Step/InputEnabled与对接要求一致；原日志16/16逻辑检查和托管编译通过，00未重跑。碰撞/重力/画面待06 |
| H25D-02 / 02 | R1补齐后复核 | 世界/相机/门/遮挡接口可衔接；门只有SetOpen程序入口，无玩家E操作，无法交付完整可操作演示。已退回02补最小交互 |
| H25D-03 / 06 | 尚未放行 | 02-R1源码与清单复核后另行派发；不得由06代写门功能 |

62相对旧58清单多出的4项是tools/migration里的.gitignore、Move-CodexData.ps1、README.md、Test-MoveCodexData.ps1；属于本次清单覆盖差异，没有执行迁移工具。

独立SHA复核记录：C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/coordination/hybrid25d-20261004/reception-checks.json。
模块报告：C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/prototypes/hybrid25d-20261004/reports/01-player.md、02-world.md、06-baseline.md。

R1固定接口：HybridDoor.public Transform interactor由06赋值；public bool TryInteract()按XZ距离≤1.6米判断，无目标/超距返回false，否则切换并返回true；Update用GetKeyDown(E)调用同一路径。保留SetOpen和门框、门叶/Collider同步；不增加通用交互系统。

未覆盖：Unity实际导入、碰撞、门通行、前景显隐、三次重建、两分辨率真实画面、新2.5D Windows包与真实键盘。单张Idle不算方向动作/Walk。P1/P2均未放行，旧事件及主工程保持。
'''
paths=[D/'TASKS.md',D/'tasks/H25D-01-dispatch.md',D/'design/H25D-direction-20261004.md',D/'HANDOFF.md']
original={p:p.read_bytes() for p in paths}
updates={}
s=original[paths[0]].decode('utf-8-sig')
states={'H25D-00':'通过（基线隔离范围）','H25D-01':'通过（模块交接；运行待验）','H25D-02':'返工R1（补门的E交互）'}
for task,state in states.items():
    line=next(x for x in s.splitlines() if x.startswith('| '+task+' |'))
    assert '已派发（等待模块交付）' in line
    s=s.replace(line,line.replace('已派发（等待模块交付）',state),1)
s+='\n2026-10-04 H25D首批接收：00已核对01/02/06实际交付，见[接收审计](reviews/H25D-first-reception-20261004.md)。H25D-00/01仅限定交接通过；02已派R1补E交互，06集成等待复核。\n'
updates[paths[0]]=s
updates[paths[1]]=original[paths[1]].decode('utf-8-sig')+'\n## H25D-02-R1 门交互补齐（00接收审计派发）\n\n02沿用剩余额度，在HybridDoor提供public Transform interactor和public bool TryInteract()；以XZ距离≤1.6米允许切换，无目标/超距拒绝；Update以GetKeyDown(E)调用同一路径。只改02模块与自检/报告/清单。06之后注入角色并检查远处拒绝、近处开关及碰撞同步；不得通过直接SetOpen冒称E交互链路通过。H25D-03待R1复核另行放行。\n'
updates[paths[2]]=original[paths[2]].decode('utf-8-sig')+'\n2026-10-04接收接口补充：P1门的玩家入口由02实现，HybridDoor.interactor（public Transform）由06注入；TryInteract()（public bool）与Update中的GetKeyDown(E)共用，XZ半径1.6米；SetOpen保留用于明确设定状态。\n'
updates[paths[3]]=original[paths[3]].decode('utf-8-sig')+'\n\n## 2026-10-04 H25D 门交互交接\n\n问题：02门只有程序SetOpen入口，缺玩家E交互。\n发现模块：00接收审计。\n影响模块：02世界、06集成。\n当前原因：首批仅实现开关接口。\n建议方案：02补interactor与TryInteract，06注入并验证同路径。\n是否阻塞：阻塞H25D-03放行，待02-R1复核。\n相关文件：HybridDoor.cs；tasks/H25D-01-dispatch.md；reviews/H25D-first-reception-20261004.md。\n00裁决：沿用已有门职责和02剩余额度，不由06/00代写。\n'
audit=D/'reviews/H25D-first-reception-20261004.md'
assert not audit.exists()
for p,b in original.items():assert p.read_bytes()==b,'Concurrent document change'
backup=R/'reception-before';backup.mkdir(exist_ok=False)
for i,(p,s) in enumerate(updates.items()):
    (backup/f'{i}-{p.name}').write_bytes(original[p]);p.write_text(s,encoding='utf-8')
audit.write_text(report,encoding='utf-8')
(R/'reception-documents.json').write_text(json.dumps({'utc':datetime.now(timezone.utc).isoformat(),'files':{str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in [*updates,audit]}},ensure_ascii=False,indent=2),encoding='utf-8')
print('RECEPTION_RECORDED')
