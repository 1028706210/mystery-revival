from pathlib import Path
import hashlib,json,shutil
from datetime import datetime,timezone
R=Path(__file__).resolve().parent
items=json.loads((R/'document-update-manifest.json').read_text(encoding='utf-8'))
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for item in items:
    p=Path(item['target'])
    actual=digest(p) if p.exists() else None
    assert actual==item['before_sha256'],('Concurrent document change',str(p))
backup=R/'before';backup.mkdir(exist_ok=False)
result=[]
for i,item in enumerate(items):
    p=Path(item['target']);s=Path(item['staged'])
    if p.exists():shutil.copy2(p,backup/f'{i:02d}-{p.name}')
    p.parent.mkdir(parents=True,exist_ok=True)
    p.write_bytes(s.read_bytes())
    assert digest(p)==digest(s)
    result.append({**item,'after_sha256':digest(p)})
(R/'document-update-result.json').write_text(json.dumps({'utc':datetime.now(timezone.utc).isoformat(),'result':'PASS','documents':result},ensure_ascii=False,indent=2),encoding='utf-8')
print('DOCUMENT_UPDATE_PASS',len(result))
