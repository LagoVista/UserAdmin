import { defineConfig } from '@playwright/test';
import { ClientInteractionEvidenceReporter } from './reporter';

export default defineConfig({ testDir: './specs', use: { baseURL: 'http://localhost:4200', headless: true }, reporter: [[ClientInteractionEvidenceReporter as any]], timeout: 15000 });
