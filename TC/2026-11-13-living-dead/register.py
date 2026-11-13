"""승인된 TC 초안을 등록하고 각 필드를 다시 읽어 비교한다. 토큰은 환경 변수로만 받는다."""
import json
import os
from pathlib import Path
import urllib.request

ROOT = Path(__file__).resolve().parent
BASE = os.environ.get('ARTEL_API_BASE_URL', 'https://stage-orch.artel.kr').rstrip('/')
TOKEN = os.environ['ARTEL_TOKEN']
PROJECT = '54'


def request(method, path, body=None):
    data = None if body is None else json.dumps(body, ensure_ascii=False).encode('utf-8')
    req = urllib.request.Request(BASE + '/api/projects/' + PROJECT + path, data=data, method=method,
                                 headers={'Authorization': 'Bearer ' + TOKEN, 'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=45) as response:
        return json.load(response)


payload = json.loads((ROOT / 'registration-request.json').read_text(encoding='utf-8-sig'))
builds = request('GET', '/game-builds')
build_items = builds.get('items', []) if isinstance(builds, dict) else builds
matches = [b for b in build_items if b.get('version') == '1.0' or b.get('gameVersion') == '1.0']
if len(matches) != 1:
    raise RuntimeError('Expected exactly one 1.0 build: ' + json.dumps(builds))
build_id = str(matches[0]['id'])
existing = request('GET', '/test-cases')['items']
observes = {
    4: ['InfimaGames.LowPolyShooterPack.Weapon::ammunitionCurrent'],
    6: ['InfimaGames.LowPolyShooterPack.Weapon::ammunitionCurrent'],
    7: ['InfimaGames.LowPolyShooterPack.Weapon::ammunitionCurrent'],
    8: ['InfimaGames.LowPolyShooterPack.Weapon::ammunitionCurrent'],
    9: ['player_health::health'], 10: ['player_health::health'], 11: ['player_health::health'],
    12: ['can_throw_grenade::grenade_present.Count'],
    13: ['can_throw_grenade::grenade_present.Count'],
    14: ['score_kill::initial_enemies.Count', 'score_kill::enemies.Count'],
    15: ['player_health::health'], 16: ['player_health::health', 'player_health::player_dead'],
    17: ['player_health::health', 'score_kill::timevalue'],
    19: ['pause_menu_script::gamepaused', 'score_kill::timevalue'],
    20: ['pause_menu_script::gamepaused', 'score_kill::timevalue'],
}
receipts = []
for number, case in enumerate(payload['cases'], 1):
    body = dict(scene=case['scene'], precondition=case['precondition'], step=case['step'],
                expectedValue=case['expected_value'], kind=case['kind'],
                arrivesAt=case.get('arrives_at'), gameBuildId=build_id, draft=True)
    matches = [c for c in existing if c['scene'] == body['scene'] and c['step'] == body['step']
               and c['precondition'] == body['precondition'] and c['expectedValue'] == body['expectedValue']]
    if len(matches) > 1:
        raise RuntimeError(f'Duplicate case {number}')
    actual = matches[0] if matches else request('POST', '/test-cases', body)
    case_id = str(actual['id'])
    if number in observes:
        request('PUT', '/test-cases/' + case_id, {'gameBuildId': build_id, 'observe': observes[number]})
    actual = request('GET', '/test-cases/' + case_id)
    for key in ('scene', 'precondition', 'step', 'expectedValue', 'kind', 'arrivesAt'):
        if actual.get(key) != body.get(key):
            raise RuntimeError(f'Case {case_id} differs at {key}')
    if actual.get('reviewStatus') != 'DRAFT' or actual.get('folderId') is not None:
        raise RuntimeError(f'Case {case_id} has unexpected review or folder state')
    if number in observes and actual.get('observe') != observes[number]:
        raise RuntimeError(f'Case {case_id} observe mismatch')
    receipts.append({'number': number, **actual})
    (ROOT / 'registration-result.json').write_text(json.dumps(
        {'projectId': PROJECT, 'gameBuildId': build_id, 'cases': receipts}, ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'Verified case {number:02}: {case_id}', flush=True)
version = request('GET', '/test-case-versions/' + build_id + '/cases')
(ROOT / 'version-result.json').write_text(json.dumps(version, ensure_ascii=False, indent=2), encoding='utf-8')
print(f'REGISTERED={len(receipts)} BUILD={build_id}', flush=True)
