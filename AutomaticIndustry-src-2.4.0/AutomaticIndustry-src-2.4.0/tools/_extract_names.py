import re, io, json
from opencc import OpenCC

root='/mnt/documents/AutoMachineRebuilt'
src=open(root+'/src/Localization/BuildingNameBinder.cs',encoding='utf-8').read()
block=src.split('OptionKeyToPrefabId =')[1].split('};')[0]
pairs=re.findall(r'\{\s*"([A-Z0-9]+)",\s*"([A-Za-z0-9]+)"\s*\}',block)
print(len(pairs),'mappings')

def parse(path):
    d={}
    ctx=None; msgid=None; msgstr=None
    cur=None
    with open(path,encoding='utf-8') as f:
        for line in f:
            line=line.rstrip('\n')
            if line.startswith('#'): continue
            m=re.match(r'msgctxt "(.*)"$',line)
            if m: ctx=m.group(1); cur='ctx'; msgid=None; msgstr=None; continue
            m=re.match(r'msgid "(.*)"$',line)
            if m: msgid=m.group(1); cur='id'; continue
            m=re.match(r'msgstr "(.*)"$',line)
            if m:
                msgstr=m.group(1); cur='str'
                if ctx: d[ctx]=(msgid,msgstr)
                continue
            m=re.match(r'"(.*)"$',line)
            if m and cur:
                if cur=='id': msgid+=m.group(1); 
                elif cur=='str':
                    msgstr+=m.group(1); d[ctx]=(msgid,msgstr)
                continue
    return d

pot=parse('/tmp/sa/strings/strings_template.pot')
zh=parse('/tmp/sa/strings/strings_preinstalled_zh_klei.po')
ko=parse('/tmp/sa/strings/strings_preinstalled_ko_klei.po')
strip=lambda s: re.sub(r'<[^>]*>','',s).replace('\\"','"').strip() if s else ''
cc=OpenCC('s2twp')
out={}
missing=[]
for key,prefab in pairs:
    k='STRINGS.BUILDINGS.PREFABS.%s.NAME'%prefab.upper()
    en=strip(pot.get(k,('',''))[0])
    z=strip(zh.get(k,('',''))[1])
    kk=strip(ko.get(k,('',''))[1])
    if not en: missing.append((key,prefab))
    out[key]={'en':en,'zhcn':z,'zhtw':cc.convert(z) if z else '','ko':kk}
print('missing:',missing)
json.dump(out,open('/tmp/gen/names.json','w'),ensure_ascii=False,indent=1)
for k,v in list(out.items())[:6]: print(k,v)
