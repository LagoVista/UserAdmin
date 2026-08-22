import type { FullResult, Reporter, TestCase, TestResult } from '@playwright/test/reporter';
import * as fs from 'fs';
import * as path from 'path';

interface TestEvidence { name: string; status: string; durationMs: number; error?: string; }
interface InteractionEvidence { interactionKey: string; platform: 'angular-web'; status: 'passed' | 'failed'; executedUtc: string; testHost: string; tests: TestEvidence[]; }
interface RuntimeSummary { interactionKey: string; status: 'passed' | 'failed'; evidencePath: string; }
interface ServerReviewObservation { interactionKey: string; status: 'not-reviewed' | 'passed' | 'failed'; checks?: Array<{ key: string; name: string; passed: boolean; note?: string }>; }
interface SignoffSummary { interactionKey: string; status: 'passed' | 'incomplete'; serverReviewStatus: 'not-reviewed' | 'passed' | 'failed'; passedChecks: number; totalChecks: number; }

export default class ClientInteractionEvidenceReporter implements Reporter {
  private readonly tests = new Map<string, TestEvidence[]>();

  onTestEnd(test: TestCase, result: TestResult): void {
    const [interactionKey, name] = test.title.split(' :: ');
    if (!interactionKey || !name) return;
    const rows = this.tests.get(interactionKey) ?? [];
    rows.push({ name, status: result.status, durationMs: result.duration, error: result.error?.message });
    this.tests.set(interactionKey, rows);
  }

  onEnd(result: FullResult): void {
    const authModelRoot = path.resolve(__dirname, '../..');
    const outputRoot = path.join(authModelRoot, 'implementation/client-interaction-runtime/angular-web');
    fs.mkdirSync(outputRoot, { recursive: true });
    const executedUtc = new Date().toISOString();
    const interactions: RuntimeSummary[] = [];
    for (const [interactionKey, tests] of this.tests.entries()) {
      const status: 'passed' | 'failed' = tests.every(test => test.status === 'passed') ? 'passed' : 'failed';
      const evidence: InteractionEvidence = { interactionKey, platform: 'angular-web', status, executedUtc, testHost: 'http://localhost:4200', tests };
      const fileName = `${interactionKey}.json`;
      fs.writeFileSync(path.join(outputRoot, fileName), `${JSON.stringify(evidence, null, 2)}\n`, 'utf8');
      interactions.push({ interactionKey, status, evidencePath: fileName });
    }
    fs.writeFileSync(path.join(outputRoot, 'latest.json'), `${JSON.stringify({ generatedUtc: executedUtc, platform: 'angular-web', status: result.status, interactions }, null, 2)}\n`, 'utf8');
    const signoff = this.writeFinalSignoff(authModelRoot, interactions, executedUtc);
    this.updateConformanceManifest(authModelRoot, interactions, signoff, executedUtc);
  }

  private updateConformanceManifest(authModelRoot: string, interactions: RuntimeSummary[], signoff: SignoffSummary[], executedUtc: string): void {
    const manifestPath = path.join(authModelRoot, 'implementation/client-interaction-conformance/angular-web.json');
    if (!fs.existsSync(manifestPath)) return;
    const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8')) as any;
    manifest.generatedUtc = executedUtc;
    for (const runtime of interactions) {
      const observation = manifest.interactions?.find((item: any) => item.interactionKey === runtime.interactionKey);
      if (!observation) continue;
      observation.runtimeEvidence = runtime.status;
      observation.runtimeEvidenceReferences = [`auth-model/implementation/client-interaction-runtime/angular-web/${runtime.evidencePath}`];
      const row = signoff.find(item => item.interactionKey === runtime.interactionKey);
      const generatedPrefix = 'Final sign-off:';
      observation.notes = (observation.notes ?? []).filter((note: string) => !note.startsWith(generatedPrefix));
      if (row) observation.notes.push(`${generatedPrefix} ${row.status === 'passed' ? 'PASSED' : 'INCOMPLETE'} — Client Test ${runtime.status}; Server Code Review ${row.serverReviewStatus} (${row.passedChecks}/${row.totalChecks}); evidence auth-model/implementation/client-interaction-signoff/angular-web/${runtime.interactionKey}.json`);
    }
    fs.writeFileSync(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`, 'utf8');
  }

  private writeFinalSignoff(authModelRoot: string, interactions: RuntimeSummary[], executedUtc: string): SignoffSummary[] {
    const reviewPath = path.join(authModelRoot, 'implementation/client-interaction-server-review/server.json');
    const serverReview = fs.existsSync(reviewPath) ? JSON.parse(fs.readFileSync(reviewPath, 'utf8')) as { interactions?: ServerReviewObservation[] } : { interactions: [] };
    const outputRoot = path.join(authModelRoot, 'implementation/client-interaction-signoff/angular-web');
    fs.mkdirSync(outputRoot, { recursive: true });
    const rows = interactions.map(runtime => {
      const review = serverReview.interactions?.find(item => item.interactionKey === runtime.interactionKey);
      const passedChecks = review?.checks?.filter(check => check.passed).length ?? 0;
      const totalChecks = review?.checks?.length ?? 0;
      const clientTestPassed = runtime.status === 'passed';
      const serverCodeReviewPassed = review?.status === 'passed' && totalChecks > 0 && passedChecks === totalChecks;
      const status: 'passed' | 'incomplete' = clientTestPassed && serverCodeReviewPassed ? 'passed' : 'incomplete';
      const row = {
        interactionKey: runtime.interactionKey,
        platform: 'angular-web',
        status,
        generatedUtc: executedUtc,
        clientTest: {
          status: runtime.status,
          evidenceReference: `auth-model/implementation/client-interaction-runtime/angular-web/${runtime.evidencePath}`
        },
        serverCodeReview: {
          status: review?.status ?? 'not-reviewed',
          passedChecks,
          totalChecks,
          evidenceReference: 'auth-model/implementation/client-interaction-server-review/server.json'
        }
      };
      fs.writeFileSync(path.join(outputRoot, `${runtime.interactionKey}.json`), `${JSON.stringify(row, null, 2)}\n`, 'utf8');
      return { interactionKey: runtime.interactionKey, status, serverReviewStatus: review?.status ?? 'not-reviewed', passedChecks, totalChecks } as SignoffSummary;
    });
    fs.writeFileSync(path.join(outputRoot, 'latest.json'), `${JSON.stringify({ generatedUtc: executedUtc, platform: 'angular-web', interactions: rows.map(row => ({ interactionKey: row.interactionKey, status: row.status, evidencePath: `${row.interactionKey}.json` })) }, null, 2)}\n`, 'utf8');
    return rows;
  }
}
