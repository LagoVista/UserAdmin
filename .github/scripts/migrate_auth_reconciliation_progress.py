import json
from pathlib import Path

ROOT = Path('auth-model')
BEHAVIOR_DIR = ROOT / 'behaviors-v2'
SCENARIO_DIR = ROOT / 'scenarios-v2'
STATUS_VALUES = ['not-started', 'in-progress', 'needs-review', 'complete', 'blocked']


def read_json(path: Path):
    return json.loads(path.read_text(encoding='utf-8'))


def write_json(path: Path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')


def category_name(key: str) -> str:
    acronyms = {'sso': 'SSO', 'totp': 'TOTP', 'mfa': 'MFA'}
    return ' '.join(acronyms.get(part, part.capitalize()) for part in key.split('-'))


def behavior_progress(old_status: str):
    progress = {
        'scenarios': 'not-started',
        'presentation': 'not-started',
        'implementation': 'not-started',
        'tests': 'not-started'
    }
    if old_status == 'reconciled':
        return {key: 'complete' for key in progress}
    if old_status == 'reconciling':
        progress['scenarios'] = 'in-progress'
    elif old_status == 'blocked':
        progress['scenarios'] = 'blocked'
    return progress


def scenario_progress():
    return {
        'presentation': 'not-started',
        'implementation': 'not-started',
        'tests': 'not-started'
    }


progress_schema = {
    '$schema': 'https://json-schema.org/draft/2020-12/schema',
    '$id': 'https://lagovista.com/schemas/auth-model/auth-reconciliation-progress.schema.json',
    'title': 'Authentication Reconciliation Progress',
    '$defs': {
        'phaseStatus': {
            'enum': STATUS_VALUES,
            'description': "A review-progress phase. 'complete' means the phase has been reviewed and agreed as canonical, not merely that related artifacts exist."
        },
        'categoryProgress': {
            'type': 'object',
            'additionalProperties': False,
            'required': ['behaviors', 'scenarios', 'presentation', 'implementation', 'tests'],
            'properties': {phase: {'$ref': '#/$defs/phaseStatus'} for phase in ['behaviors', 'scenarios', 'presentation', 'implementation', 'tests']}
        },
        'behaviorProgress': {
            'type': 'object',
            'additionalProperties': False,
            'required': ['scenarios', 'presentation', 'implementation', 'tests'],
            'properties': {phase: {'$ref': '#/$defs/phaseStatus'} for phase in ['scenarios', 'presentation', 'implementation', 'tests']}
        },
        'scenarioProgress': {
            'type': 'object',
            'additionalProperties': False,
            'required': ['presentation', 'implementation', 'tests'],
            'properties': {phase: {'$ref': '#/$defs/phaseStatus'} for phase in ['presentation', 'implementation', 'tests']}
        }
    }
}
write_json(ROOT / 'schemas' / 'auth-reconciliation-progress.schema.json', progress_schema)

behavior_documents = []
for path in sorted(BEHAVIOR_DIR.glob('*.json')):
    document = read_json(path)
    old_status = str(document.pop('reconciliationStatus', 'needs-reconciliation'))
    if 'progress' not in document:
        document['progress'] = behavior_progress(old_status)
    write_json(path, document)
    behavior_documents.append(document)

for path in sorted(SCENARIO_DIR.glob('**/*.json')):
    document = read_json(path)
    if 'progress' not in document:
        document['progress'] = scenario_progress()
    write_json(path, document)

behavior_schema_path = ROOT / 'schemas' / 'linear-user-behavior-v2.schema.json'
behavior_schema = read_json(behavior_schema_path)
required = behavior_schema.get('required', [])
required = ['progress' if item == 'reconciliationStatus' else item for item in required]
if 'progress' not in required:
    required.append('progress')
behavior_schema['required'] = required
properties = behavior_schema['properties']
properties.pop('reconciliationStatus', None)
properties['progress'] = {
    '$ref': 'auth-reconciliation-progress.schema.json#/$defs/behaviorProgress',
    'description': 'Tracks reviewed downstream reconciliation phases for this behavior.'
}
write_json(behavior_schema_path, behavior_schema)

scenario_schema_path = ROOT / 'schemas' / 'app-user-test-scenario-v2.schema.json'
scenario_schema = read_json(scenario_schema_path)
scenario_required = scenario_schema.get('required', [])
if 'progress' not in scenario_required:
    maturity_index = scenario_required.index('maturity') if 'maturity' in scenario_required else len(scenario_required) - 1
    scenario_required.insert(maturity_index + 1, 'progress')
scenario_schema['required'] = scenario_required
scenario_schema['properties']['progress'] = {
    '$ref': 'auth-reconciliation-progress.schema.json#/$defs/scenarioProgress',
    'description': 'Tracks reviewed presentation, implementation, and test reconciliation phases for this atomic scenario.'
}
write_json(scenario_schema_path, scenario_schema)

active_behaviors = [document for document in behavior_documents if document.get('maturity') != 'deprecated']
categories = []
for category_key in sorted({str(document['categoryKey']) for document in active_behaviors}):
    behavior_keys = sorted(str(document['key']) for document in active_behaviors if document.get('categoryKey') == category_key)
    categories.append({
        'key': category_key,
        'name': category_name(category_key),
        'behaviorKeys': behavior_keys,
        'progress': {
            'behaviors': 'not-started',
            'scenarios': 'not-started',
            'presentation': 'not-started',
            'implementation': 'not-started',
            'tests': 'not-started'
        }
    })

category_catalog_schema = {
    '$schema': 'https://json-schema.org/draft/2020-12/schema',
    '$id': 'https://lagovista.com/schemas/auth-model/behavior-category-catalog.schema.json',
    'title': 'Authentication Behavior Category Catalog',
    'type': 'object',
    'additionalProperties': False,
    'required': ['$schema', 'schemaVersion', 'key', 'name', 'inventoryStatus', 'categories', 'sourceReferences'],
    'properties': {
        '$schema': {'type': 'string'},
        'schemaVersion': {'const': '1.0'},
        'key': {'const': 'auth.catalog.behavior-categories'},
        'name': {'type': 'string', 'minLength': 1},
        'inventoryStatus': {'$ref': 'auth-reconciliation-progress.schema.json#/$defs/phaseStatus'},
        'categories': {
            'type': 'array',
            'uniqueItems': True,
            'items': {
                'type': 'object',
                'additionalProperties': False,
                'required': ['key', 'name', 'behaviorKeys', 'progress'],
                'properties': {
                    'key': {'type': 'string', 'maxLength': 64, 'pattern': '^[a-z][a-z0-9]*(?:-[a-z0-9]+)+$'},
                    'name': {'type': 'string', 'minLength': 1},
                    'behaviorKeys': {'type': 'array', 'uniqueItems': True, 'items': {'type': 'string', 'pattern': '^auth\\.behavior\\.'}},
                    'progress': {'$ref': 'auth-reconciliation-progress.schema.json#/$defs/categoryProgress'}
                }
            }
        },
        'sourceReferences': {
            'type': 'array',
            'items': {
                'type': 'object',
                'additionalProperties': False,
                'required': ['sourceType', 'reference'],
                'properties': {
                    'sourceType': {'enum': ['discussion', 'decision', 'code', 'test', 'external']},
                    'reference': {'type': 'string', 'minLength': 1},
                    'note': {'type': 'string'}
                }
            }
        }
    }
}
write_json(ROOT / 'schemas' / 'behavior-category-catalog.schema.json', category_catalog_schema)

category_catalog = {
    '$schema': './schemas/behavior-category-catalog.schema.json',
    'schemaVersion': '1.0',
    'key': 'auth.catalog.behavior-categories',
    'name': 'Authentication Behavior Categories',
    'inventoryStatus': 'complete',
    'categories': categories,
    'sourceReferences': [
        {
            'sourceType': 'discussion',
            'reference': 'ChatGPT conversation 2026-08-08',
            'note': 'Category-universe review completed after normalizing subject-action category names, dropping Magic Link from V2, and adding External Identity Provider Management.'
        },
        {
            'sourceType': 'discussion',
            'reference': 'ChatGPT conversation 2026-08-08',
            'note': "Hierarchical reconciliation progress is authored at category, behavior, and scenario levels. A phase is complete only after review and agreement, not merely because related files exist."
        }
    ]
}
write_json(ROOT / 'behavior-category-catalog.json', category_catalog)

manifest_path = ROOT / 'model-manifest.json'
manifest = read_json(manifest_path)
manifest['definitionRoots']['behaviorCategories'] = 'behavior-category-catalog.json'
manifest['schemas']['behaviorCategoryCatalog'] = 'schemas/behavior-category-catalog.schema.json'
manifest['schemas']['reconciliationProgress'] = 'schemas/auth-reconciliation-progress.schema.json'
manifest['inventory']['behaviorCategories'] = len(categories)
manifest['currentWork']['summary'] = (
    'V2 reconciliation now uses hierarchical reviewed progress from behavior category through behavior and atomic scenario. '
    'The behavior-category inventory has been reviewed as complete; each active category begins with its behavior-definition phase not started until that category is explicitly reviewed. '
    'Behavior progress tracks scenarios, presentation, implementation, and tests; scenario progress tracks presentation, implementation, and tests. '
    'This phase model supersedes the former coarse behavior reconciliation status.'
)
write_json(manifest_path, manifest)

for path in ROOT.glob('**/*.json'):
    read_json(path)

print(f'Migrated {len(behavior_documents)} V2 behaviors, added progress to V2 scenarios, and authored {len(categories)} active behavior categories.')
