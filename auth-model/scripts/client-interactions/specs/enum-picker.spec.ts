import { expect, test } from '@playwright/test';

test('client.interaction.enum-picker :: select', async ({ page }) => {
  await page.goto('/client-interaction-test');
  await expect(page.getByTestId('client-interaction-test-ready')).toHaveText('ready');
  await page.evaluate(() => (window as any).__aptixClientInteractionTest?.load({ sessionId: 'TEST-SESSION', turnId: 'TEST-TURN', clientDirectives: [{ directiveId: 'TEST-DIRECTIVE-PICKER-001', action: 'client.enum-picker', preamble: 'Choose a priority.', payload: { options: [{ value: 'low', label: 'Low' }, { value: 'normal', label: 'Normal' }, { value: 'high', label: 'High' }] } }] }));
  await expect(page.locator('[data-aptix-finder="select:selection"]')).toBeVisible();
  await page.locator('[data-aptix-finder="select:selection"]').selectOption('high');
  await page.locator('[data-aptix-finder="action:submit"]').click();
  const submittedRequest = await page.evaluate(() => (window as any).__aptixClientInteractionTest?.getSubmittedRequest());
  expect(submittedRequest).toEqual({ sessionId: 'TEST-SESSION', turnId: 'TEST-TURN', clientDirectiveResults: [{ directiveId: 'TEST-DIRECTIVE-PICKER-001', action: 'client.enum-picker', result: 'selected', scalar: { stringValue: 'high' } }] });
});
