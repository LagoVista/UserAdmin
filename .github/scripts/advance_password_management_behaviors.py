import json
from pathlib import Path

root = Path('auth-model')
catalog_path = root / 'behavior-category-catalog.json'
manifest_path = root / 'model-manifest.json'

catalog = json.loads(catalog_path.read_text(encoding='utf-8'))
category = next(item for item in catalog['categories'] if item['key'] == 'password-management')
category['behaviorKeys'] = [
    'auth.behavior.password.change-failed',
    'auth.behavior.password.change-success'
]
category['progress']['behaviors'] = 'complete'
category['progress']['scenarios'] = 'in-progress'
catalog['sourceReferences'].append({
    'sourceType': 'discussion',
    'reference': 'ChatGPT conversation 2026-08-08',
    'note': 'Password Management behavior review resolved the category to Password Change Success and Password Change Failed; validation and identity failure permutations belong at the scenario/test layers.'
})
catalog_path.write_text(json.dumps(catalog, indent=2) + '\n', encoding='utf-8')

manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
manifest['inventory']['endToEndBehaviors'] = len(list((root / 'behaviors-v2').glob('*.json')))
manifest['currentWork']['summary'] = 'Password Management is the first category advancing through hierarchical reconciliation. Its behavior universe is reviewed complete with Password Change Success and Password Change Failed; scenario reconciliation is now in progress. Other categories retain their prior reconciliation phase states.'
manifest_path.write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')

for path in root.glob('**/*.json'):
    json.loads(path.read_text(encoding='utf-8'))
