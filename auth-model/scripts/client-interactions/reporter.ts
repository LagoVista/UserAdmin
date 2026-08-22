import type { FullResult, Reporter, TestCase, TestResult } from '@playwright/test/reporter';
import * as fs from 'fs';
import * as path from 'path';

interface TestEvidence { name: string; status: string; durationMs: number; error?: string; }
interface InteractionEvidence { interactionKey: string; platform: 'angular-web'; status: 'passed' | 'failed'; executedUtc: string; testHost: string; tests: TestEvidence[]; }

export class ClientInteractionEvidenceReporter implements Reporter {
  private readonly tests = new Map<string, TestEvidence[]>();

  onTestEnd(test: TestCase, result: TestResult): void {
    const [interactionKey, name] = test.title.split(' :: ');
    if (!interactionKey || !name) return;
    const rows = this.tests.get(interactionKey) ?? [];
    rows.push({ name, status: result.status, durationMs: result.duration, error: result.error?.message });
    this.tests.set(interactionKey, rows);
  }

  onEnd(result: FullResult): void {
    const outputRoot = path.resolve(process.cwd(), '../../implementation/client-interaction-runtime/angular-web');
    fs.mkdirSync(outputRoot, { recursive: true });
    const executedUtc = new Date().toISOString();
    const interactions: Array<{ interactionKey: string; status: 'passed' | 'failed'; evidencePath: string }> = [];
    for (const [interactionKey, tests] of this.tests.entries()) {
      const status: 'passed' | 'failed' = tests.every(test => test.status === 'passed') ? 'passed' : 'failed';
      const evidence: InteractionEvidence = { interactionKey, platform: 'angular-web', status, executedUtc, testHost: 'http://localhost:4200', tests };
      const fileName = `${interactionKey}.json`;
      fs.writeFileSync(path.join(outputRoot, fileName), `${JSON.stringify(evidence, null, 2)}\n`, 'utf8');
      interactions.push({ interactionKey, status, evidencePath: fileName });
    }
    fs.writeFileSync(path.join(outputRoot, 'latest.json'), `${JSON.stringify({ generatedUtc: executedUtc, platform: 'angular-web', status: result.status, interactions }, null, 2)}\n`, 'utf8');
  }
}
