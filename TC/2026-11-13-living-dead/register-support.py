"""입력 매크로와 게임 값·규칙을 API 검사를 거쳐 등록한다. 게임 입력은 실행하지 않는다."""
import json
import os
from pathlib import Path
import urllib.request
import urllib.error

ROOT = Path(__file__).resolve().parents[2]
BASE = os.environ.get('ARTEL_API_BASE_URL', 'https://stage-orch.artel.kr').rstrip('/')
TOKEN = os.environ['ARTEL_TOKEN']
OUT = Path(__file__).resolve().parent / 'support-result.json'

def request(method, path, body=None):
    data = None if body is None else json.dumps(body, ensure_ascii=False).encode('utf-8')
    req = urllib.request.Request(BASE + '/api/projects/54' + path, data=data, method=method,
        headers={'Authorization': 'Bearer ' + TOKEN, 'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=45) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        raise RuntimeError(str(error.code) + ': ' + error.read().decode('utf-8')) from None

results = {'macros': [], 'facts': [], 'rules': [], 'failures': []}
def save():
    OUT.write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding='utf-8')

macros = json.loads((ROOT / 'artel/macros.json').read_text(encoding='utf-8'))['upsert']
for macro in macros:
    try:
        body = dict(source=macro['source'], contract=macro['contract'], gameBuildId='50')
        actual = request('PUT', '/developer-macros/' + macro['name'], body)
        actual = request('GET', '/developer-macros/' + macro['name'])
        if actual['source'] != macro['source'] or str(actual['gameBuildId']) != '50':
            raise RuntimeError('Macro readback mismatch')
        results['macros'].append(actual)
        print('Verified macro: ' + macro['name'], flush=True)
    except RuntimeError as error:
        results['failures'].append({'name': macro['name'], 'error': str(error)})
        print('REJECTED ' + macro['name'] + ': ' + str(error), flush=True)
    save()

support = json.loads((ROOT / 'artel/facts-rules.json').read_text(encoding='utf-8'))
for group, endpoint in [('facts', '/game-facts'), ('rules', '/game-rules')]:
    for item in support[group]:
        results[group].append(request('PUT', endpoint, item))
        save()
readback = request('GET', '/game-facts')
for group in ('facts', 'rules'):
    indexed = {item['name']: item for item in readback[group]}
    for expected in support[group]:
        for key, value in expected.items():
            if indexed[expected['name']].get(key) != value:
                raise RuntimeError(f'{group} readback mismatch: {expected["name"]}.{key}')
results['readbackVerified'] = True
save()
print('REGISTERED ' + json.dumps({key: len(results[key]) for key in ('macros', 'facts', 'rules', 'failures')}), flush=True)
