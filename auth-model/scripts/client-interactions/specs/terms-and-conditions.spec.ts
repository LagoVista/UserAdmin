import { expect, test } from '@playwright/test';

test('client.interaction.terms-and-conditions :: accept', async ({ page }) => {
  await page.goto('/client-interaction-test');
  await expect(page.getByTestId('client-interaction-test-ready')).toHaveText('ready');
  await page.evaluate(() => window.__aptixClientInteractionTest?.load({ directiveId: 'TEST-DIRECTIVE-TERMS-001', action: 'client.terms-and-conditions', termsAndConditionsVersion: 'TEST-TERMS-V1' }));
  await expect(page.getByTestId('display:terms-version')).toHaveText('TEST-TERMS-V1');
  await expect(page.getByTestId('action:view-terms')).toBeVisible();
  await expect(page.getByTestId('action:accept')).toBeVisible();
  await expect(page.getByTestId('action:reject')).toBeVisible();
  await page.getByTestId('action:accept').click();
  await expect(page.getByTestId('client-interaction-test-completion')).toContainText('accepted');
  const completion = await page.evaluate(() => window.__aptixClientInteractionTest?.getCompletion());
  expect(completion).toEqual({ correlationId: 'TEST-DIRECTIVE-TERMS-001', outcome: 'accepted' });
});
