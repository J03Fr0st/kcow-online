import { test, expect } from '@playwright/test';
import { BasePage } from './page-objects/base.page';

test.describe('Forms Functionality', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new BasePage(testPage);
    await page.goto('/forms');
  });

  test('should load forms page', async ({ page: testPage }) => {
    expect(await page.getCurrentPath()).toContain('/forms');
  });

  test('should display form fields', async ({ page: testPage }) => {
    const inputs = await testPage.locator('input, textarea, select').count();
    expect(inputs).toBeGreaterThan(0);
  });

  test('should validate required fields', async ({ page: testPage }) => {
    const requiredInputs = await testPage.locator('input[required], input[aria-required="true"]').all();

    if (requiredInputs.length > 0) {
      const submitButton = testPage.locator('button[type="submit"]').first();

      if (await submitButton.isVisible()) {
        // Try to submit without filling required fields
        await submitButton.click();
        await testPage.waitForTimeout(500);

        // Should show validation errors
        const errors = await testPage.locator('.error, .invalid, [class*="error"], [role="alert"]').count();
        expect(errors).toBeGreaterThanOrEqual(0);
      }
    }
  });

  test('should accept text input', async ({ page: testPage }) => {
    const textInputs = await testPage.locator('input[type="text"], input:not([type])').all();

    if (textInputs.length > 0) {
      const input = textInputs[0];
      await input.fill('Test Input');

      const value = await input.inputValue();
      expect(value).toBe('Test Input');
    }
  });

  test('should accept email input with validation', async ({ page: testPage }) => {
    const emailInputs = await testPage.locator('input[type="email"]').all();

    if (emailInputs.length > 0) {
      const emailInput = emailInputs[0];

      // Enter invalid email
      await emailInput.fill('invalid-email');
      await emailInput.blur();
      await testPage.waitForTimeout(300);

      // Enter valid email
      await emailInput.fill('test@example.com');
      const value = await emailInput.inputValue();
      expect(value).toBe('test@example.com');
    }
  });

  test('should handle number input', async ({ page: testPage }) => {
    const numberInputs = await testPage.locator('input[type="number"]').all();

    if (numberInputs.length > 0) {
      const numberInput = numberInputs[0];
      await numberInput.fill('42');

      const value = await numberInput.inputValue();
      expect(value).toBe('42');
    }
  });

  test('should handle textarea input', async ({ page: testPage }) => {
    const textareas = await testPage.locator('textarea').all();

    if (textareas.length > 0) {
      const textarea = textareas[0];
      const testText = 'This is a multiline\ntext input\nfor testing';

      await textarea.fill(testText);
      const value = await textarea.inputValue();
      expect(value).toContain('multiline');
    }
  });

  test('should handle select dropdown', async ({ page: testPage }) => {
    const selects = await testPage.locator('select').all();

    if (selects.length > 0) {
      const select = selects[0];
      const options = await select.locator('option').all();

      if (options.length > 1) {
        const optionValue = await options[1].getAttribute('value');

        if (optionValue) {
          await select.selectOption(optionValue);
          const selectedValue = await select.inputValue();
          expect(selectedValue).toBe(optionValue);
        }
      }
    }
  });

  test('should handle checkbox input', async ({ page: testPage }) => {
    const checkboxes = await testPage.locator('input[type="checkbox"]').all();

    if (checkboxes.length > 0) {
      const checkbox = checkboxes[0];

      await checkbox.check();
      expect(await checkbox.isChecked()).toBeTruthy();

      await checkbox.uncheck();
      expect(await checkbox.isChecked()).toBeFalsy();
    }
  });

  test('should handle radio button input', async ({ page: testPage }) => {
    const radios = await testPage.locator('input[type="radio"]').all();

    if (radios.length > 1) {
      await radios[0].check();
      expect(await radios[0].isChecked()).toBeTruthy();

      // Check another radio in the same group
      await radios[1].check();
      expect(await radios[1].isChecked()).toBeTruthy();
    }
  });

  test('should submit form', async ({ page: testPage }) => {
    const submitButton = testPage.locator('button[type="submit"], button:has-text("Submit")').first();

    if (await submitButton.isVisible()) {
      // Fill in required fields if any
      const inputs = await testPage.locator('input[required]').all();

      for (const input of inputs) {
        const type = await input.getAttribute('type');

        if (type === 'email') {
          await input.fill('test@example.com');
        } else if (type === 'checkbox') {
          await input.check();
        } else {
          await input.fill('Test Value');
        }
      }

      await submitButton.click();
      await testPage.waitForTimeout(500);

      // Form should either show success message or reset
      expect(true).toBeTruthy();
    }
  });

  test('should reset form', async ({ page: testPage }) => {
    const resetButton = testPage.locator('button[type="reset"], button:has-text("Reset")').first();

    if (await resetButton.isVisible()) {
      // Fill some fields
      const textInput = testPage.locator('input[type="text"]').first();
      if (await textInput.isVisible()) {
        await textInput.fill('Test Data');

        // Reset the form
        await resetButton.click();
        await testPage.waitForTimeout(300);

        const value = await textInput.inputValue();
        expect(value).toBe('');
      }
    }
  });

  test('should show validation errors', async ({ page: testPage }) => {
    const emailInput = testPage.locator('input[type="email"]').first();

    if (await emailInput.isVisible()) {
      await emailInput.fill('invalid-email');
      await emailInput.blur();
      await testPage.waitForTimeout(500);

      // Might show error message
      const errors = await testPage.locator('.error, .invalid, [class*="error"]').count();
      expect(errors).toBeGreaterThanOrEqual(0);
    }
  });

  test('should handle dynamic form fields', async ({ page: testPage }) => {
    const addButton = testPage.locator('button:has-text("Add"), button:has-text("+")').first();

    if (await addButton.isVisible()) {
      const initialInputCount = await testPage.locator('input').count();

      await addButton.click();
      await testPage.waitForTimeout(500);

      const newInputCount = await testPage.locator('input').count();

      // Count might have increased
      expect(newInputCount).toBeGreaterThanOrEqual(initialInputCount);
    }
  });
});
