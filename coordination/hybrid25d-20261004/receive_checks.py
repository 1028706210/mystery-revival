"""00 read-only reception checks; writes only audit evidence."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,re
R=Path(__file__).resolve().parent
C=R.parents[1]
P=C/'prototypes/hybrid25d-20261004'
D=Path('D:/projects/神秘复苏')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
checks={}
inventory=json.loads((P/'reports/source-inventory-before.json').read_text(encoding='utf-8-sig'))
for f in inventory:checks['main:'+f['path']]=sha(D/f['path'])==f['sha256'].upper()
allowed={'Game/Packages/manifest.json','Game/Packages/packages-lock.json'}
diff=[]
for f in inventory:
    target=P/'Validation'/f['path']
    if sha(target)!=f['sha256'].upper():diff.append(f['path'])
checks['only-approved-copy-config-diff']=set(diff)==allowed
for name in ('manifest.json','packages-lock.json'):
    before=json.loads((D/'Game/Packages'/name).read_text(encoding='utf-8-sig'))
    after=json.loads((P/'Validation/Game/Packages'/name).read_text(encoding='utf-8-sig'))
    bd=before['dependencies'];ad=after['dependencies']
    checks[name+':only-physics-added']=set(ad)-set(bd)=={'com.unity.modules.physics'} and all(ad[k]==v for k,v in bd.items())
report=(P/'reports/01-player.md').read_text(encoding='utf-8-sig').split('## 文件清单/SHA256',1)[1]
player_files=re.findall(r'^\| ([^|]+?) \| ([A-Fa-f0-9]{64}) \|$',report,re.M)
for name,h in player_files:checks['01:'+name]=sha(P/'modules/01/Assets/Hybrid25D/Player'/name)==h.upper()
checks['01:manifest-count-16']=len(player_files)==16
delivery=json.loads((P/'reports/delivery-integrity.json').read_text(encoding='utf-8-sig'))
for f in delivery['files']:checks['06-report:'+f['path']]=sha(P/'reports'/f['path'])==f['sha256'].upper()
older=json.loads((C/'building_samples/corridor-wall-integration-20261003/baseline.json').read_text(encoding='utf-8'))
oldnames={s.replace('\\','/') for s in older['source_sha256']}
newnames={f['path'] for f in inventory}
result={'utc':datetime.now(timezone.utc).isoformat(),'result':'PASS' if all(checks.values()) else 'FAIL',
        'checks':checks,'main_count':len(inventory),'copy_differences':diff,
        'inventory_difference_from_older_58':sorted(newnames-oldnames),
        'failed':[n for n,v in checks.items() if not v]}
(R/'reception-checks.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k!='checks'},ensure_ascii=False))
assert result['result']=='PASS'
