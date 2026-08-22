import { expect, test } from '@playwright/test';

test('client.interaction.yes-no-confirmation :: yes', async ({ page }) => {
  await page.goto('/client-interaction-test');
  await expect(page.getByTestId('client-interaction-test-ready')).toHaveText('ready');
  await page.evaluate(() => (window as any).__aptixClientInteractionTest?.load({ sessionId: 'TEST-SESSION', turnId: 'TEST-TURN', clientDirectives: [{ directiveId: 'TEST-DIRECTIVE-CONFIRM-001', action: 'client.yes-no-confirmation', preamble: 'Would you like to continue?' }] }));
  await expect(page.locator('[data-aptix-finder="display:prompt"]')).toHaveText('Would you like to continue?');
  await expect(page.locator('[data-aptix-finder="action:yes"]')).toBeVisible();
  await expect(page.locator('[data-aptix-finder="action:no"]')).toBeVisible();
  await page.locator('[data-aptix-finder="action:yes"]').click();
  const submittedRequest = await page.evaluate(() => (window as any).__aptixClientInteractionTest?.getSubmittedRequest());
  expect(submittedRequest).toEqual({ sessionId: 'TEST-SESSION', turnId: 'TEST-TURN', clientDirectiveResults: [{ directiveId: 'TEST-DIRECTIVE-CONFIRM-001', action: 'client.yes-no-confirmation', result: 'yes' }] });
});

test('client.interaction.yes-no-confirmation :: no', async ({ page }) => {
  await page.goto('/client-interaction-test');
  await page.evaluate(() => (window as any).__aptixClientInteractionTest?.load({ sessionId: 'TEST-SESSION', turnId: 'TEST-TURN', clientDirectives: [{ directiveId: 'TEST-DIRECTIVE-CONFIRM-002', action: 'client.yes-no-confirmation', preamble: 'Would you like to continue?' }] }));
  await page.locator('[data-aptix-finder="action:no"]').click();
  const submittedRequest = await page.evaluate(() => (window as any).__aptixClientInteractionTest?.getSubmittedRequest());
  expect(submittedRequest).toEqual({ sessionId: 'TEST-SESSION', turnId: 'TEST-TURN', clientDirectiveResults: [{ directiveId: 'TEST-DIRECTIVE-CONFIRM-002', action: 'client.yes-no-confirmation', result: 'no' }] });
});
