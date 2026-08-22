import { defineConfig } from '@playwright/test';

export default defineConfig({ testDir: './specs', use: { baseURL: 'http://localhost:4200', headless: true }, reporter: [['./reporter.ts']], timeout: 15000 });
