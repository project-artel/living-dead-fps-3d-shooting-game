"""기존 READY TC로 ORDERED와 FREE Test Run의 시나리오를 작성한다. QA는 실행하지 않는다."""
import json
import os
from pathlib import Path
import urllib.request
import urllib.error

ROOT = Path(__file__).resolve().parent
BASE = os.environ.get('ARTEL_API_BASE_URL', 'https://stage-orch.artel.kr').rstrip('/')
TOKEN = os.environ['ARTEL_TOKEN']

def request(method, path, body=None):
    data = None if body is None else json.dumps(body, ensure_ascii=False).encode('utf-8')
    req = urllib.request.Request(BASE + '/api/projects/54' + path, data=data, method=method,
        headers={'Authorization': 'Bearer ' + TOKEN, 'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=120) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        raise RuntimeError(str(error.code) + ': ' + error.read().decode('utf-8')) from None

plans = json.loads((ROOT / 'test-runs-request.json').read_text(encoding='utf-8'))
cases = request('GET', '/test-cases')['items']
ready = {int(c['id']) for c in cases if c['reviewStatus'] == 'READY'}
needed = {s['case_id'] for group in plans.values() for scenario in group for s in scenario['steps'] if s.get('case_id')}
if needed - ready:
    raise RuntimeError('Cases still need console approval: ' + str(needed - ready))
runs = request('GET', '/test-runs')['items']
results = {}
for kind, name in [('ordered', '순서 있는 핵심 플레이'), ('free', '순서 없는 핵심 기능')]:
    matches = [run for run in runs if run['name'] == name]
    if len(matches) != 1:
        raise RuntimeError('Expected one existing Test Run: ' + name)
    run_id = str(matches[0]['id'])
    existing = request('GET', '/test-runs/' + run_id + '/scenarios')
    details = [request('GET', '/test-scenario/' + str(item['testScenarioId'])) for item in existing['items']]
    titles = {item['payload']['title']: item['testScenarioId'] for item in details}
    scenarios = plans[kind]
    for scenario in scenarios:
        if scenario['title'] in titles:
            scenario['scenario_id'] = int(titles[scenario['title']])
    saved = request('POST', '/test-runs/' + run_id + '/scenarios/commit', {'scenarios': scenarios, 'draft': True})
    actual = request('GET', '/test-runs/' + run_id + '/scenarios')
    results[kind] = {'run': matches[0], 'scenarios': actual}
    (ROOT / 'test-runs-result.json').write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding='utf-8')
    print('SAVED ' + kind + ' RUN=' + run_id + ' SCENARIOS=' + str(len(actual['items'])), flush=True)
