from pathlib import Path
import hashlib,json
R=Path(__file__).resolve().parent
receipts=json.loads((R/'dispatch-receipts.json').read_text(encoding='utf-8'))
p=Path('D:/projects/神秘复苏/Docs/TASKS.md')
record=json.loads((R/'document-update-result.json').read_text(encoding='utf-8'))
expected=next(x['after_sha256'] for x in record['documents'] if Path(x['target'])==p)
assert hashlib.sha256(p.read_bytes()).hexdigest()==expected,'Task table changed concurrently'
s=p.read_text(encoding='utf-8')
for task in ('H25D-00','H25D-01','H25D-02'):
    assert receipts['receipts'][task].get('isError') is False
    matches=[line for line in s.splitlines() if line.startswith('| '+task+' |')]
    assert len(matches)==1 and '可派发' in matches[0]
    s=s.replace(matches[0],matches[0].replace('可派发','已派发（等待模块交付）'),1)
s+='\n2026-10-04 H25D派发回执：已实际发送至既有01 玩家与角色、02 世界与地图、06 测试与集成会话，三次工具回执均成功；不把派发当作完成。H25D-03尚未放行。记录位于C:/Users/李帅朋/Documents/ChatGPT/神秘复苏/coordination/hybrid25d-20261004/dispatch-receipts.json。\n'
p.write_text(s,encoding='utf-8')
print('DISPATCH_RECORDED',3)
