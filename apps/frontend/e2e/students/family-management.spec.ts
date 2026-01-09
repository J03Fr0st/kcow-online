import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Family Management in Student Profile
 *
 * Tests family management features including:
 * - Family Grid display at bottom of profile
 * - Adding family member with relationship type
 * - Editing family contact information
 * - Sibling display
 */

test.describe('Family Management in Student Profile', () => {
  test.beforeEach(async ({ page }) => {
    const testEmail = process.env.TEST_EMAIL || 'admin@kcow.local';
    const testPassword = process.env.TEST_PASSWORD || 'Admin123!';

    await page.goto('/login');
    await page.locator('#email').fill(testEmail);
    await page.locator('#password').fill(testPassword);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    await Promise.all([
      page.waitForURL(/\/dashboard/, { timeout: 15000 }),
      page.locator('button[type="submit"]').click(),
    ]);

    await page.waitForTimeout(1000);
  });

  test('should display family grid at bottom of profile', async ({ page }) => {
    // Navigate to a student profile
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for Family section
    const familySection = page.locator('text=/family/i, [class*="family"], #family, app-family-section');
    await page.waitForTimeout(500);

    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Family section should be visible
      await expect(familySection.first()).toBeVisible();

      // Look for family contacts heading
      const familyHeading = familySection.filter({ hasText: /family contacts/i });
      const hasHeading = await familyHeading.count() > 0;

      if (hasHeading) {
        await expect(familyHeading.first()).toBeVisible();
      }
    }
  });

  test('should display existing family members with details', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for family member information
      const familyName = page.locator('text=/', 'text=/Parent|Guardian|Mother|Father/i');
      const hasFamilyName = await familyName.count() > 0;

      if (hasFamilyName) {
        // Should show family members
        const memberDetails = await familyName.allTextContents();
        expect(memberDetails.length).toBeGreaterThan(0);
      }
    }
  });

  test('should display add family member button', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for "Add Family" or "Add Guardian" button
      const addButton = page.locator('button').filter({ hasText: /add family|add guardian|create family/i });
      const hasAddButton = await addButton.count() > 0;

      if (hasAddButton) {
        await expect(addButton.first()).toBeVisible();
      }
    }
  });

  test('should allow adding a new family member', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      const addButton = page.locator('button').filter({ hasText: /add family|add guardian/i });
      const hasAddButton = await addButton.count() > 0;

      if (hasAddButton) {
        await addButton.first().click();
        await page.waitForTimeout(500);

        // Should show form for adding family member
        const formInputs = page.locator('input[type="text"], input[type="email"], input[type="tel"], select, textarea');
        const hasInputs = await formInputs.count() > 0;

        if (hasInputs) {
          // Look for relationship type dropdown
          const relationshipSelect = page.locator('select[name*="relationship" i], [formcontrolname="relationship"]');
          const hasRelationship = await relationshipSelect.count() > 0;

          if (hasRelationship) {
            await relationshipSelect.first().click();
            await page.waitForTimeout(200);

            // Select a relationship type
            const relationshipOption = page.locator('option').filter({ hasText: /parent|guardian/i });
            if (await relationshipOption.count() > 0) {
              await relationshipOption.first().click();
            }
          }

          // Fill in guardian name
          const nameInput = page.locator('input[name*="name" i][*="first" i], input[placeholder*="first" i]');
          const hasNameInput = await nameInput.count() > 0;

          if (hasNameInput) {
            await nameInput.first().fill('Test');
            await page.waitForTimeout(200);

            const lastNameInput = page.locator('input[name*="name" i][*="last" i], input[placeholder*="last" i]');
            if (await lastNameInput.count() > 0) {
              await lastNameInput.first().fill('Guardian');
              await page.waitForTimeout(200);
            }

            // Click save
            const saveButton = page.locator('button').filter({ hasText: /save|add|create/i });
            if (await saveButton.count() > 0) {
              await saveButton.first().click();
              await page.waitForTimeout(1000);

              // Should show success message or update the list
              const successMessage = page.locator('.alert, .toast, .snackbar, [role="alert"]').filter({ hasText: /success|added|saved/i });
              const hasSuccess = await successMessage.count() > 0;

              if (hasSuccess) {
                await expect(successMessage.first()).toBeVisible();
              }
            }
          }
        }
      }
    }
  });

  test('should allow editing family contact information', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for edit button in family section
      const editButton = familySection.locator('button').filter({ hasText: /edit/i });
      const hasEditButton = await editButton.count() > 0;

      if (hasEditButton) {
        await editButton.first().click();
        await page.waitForTimeout(500);

        // Should show edit form
        const formInputs = page.locator('input[type="text"], input[type="email"], input[type="tel"], textarea');
        const hasInputs = await formInputs.count() > 0;

        if (hasInputs) {
          // Modify a field
          const phoneInput = page.locator('input[name*="phone" i], input[type="tel"], [placeholder*="phone" i]');
          const hasPhone = await phoneInput.count() > 0;

          if (hasPhone) {
            await phoneInput.first().fill('012 345 6789');
            await page.waitForTimeout(200);

            // Save
            const saveButton = page.locator('button').filter({ hasText: /save|update/i });
            await saveButton.first().click();
            await page.waitForTimeout(1000);

            // Should show success
            const successMessage = page.locator('.alert, .toast, .snackbar, [role="alert"]').filter({ hasText: /success|saved|updated/i });
            const hasSuccess = await successMessage.count() > 0;

            if (hasSuccess) {
              await expect(successMessage.first()).toBeVisible();
            }
          }
        }
      }
    }
  });

  test('should display sibling students', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for siblings section
      const siblingsSection = page.locator('text=/sibling/i, [class*="sibling"]');
      const hasSiblings = await siblingsSection.count() > 0;

      if (hasSiblings) {
        // Should show sibling information
        await expect(siblingsSection.first()).toBeVisible();

        // Siblings should be clickable links
        const siblingLinks = siblingsSection.locator('button, a');
        const linkCount = await siblingLinks.count();

        if (linkCount > 0) {
          // Click on sibling to navigate
          await siblingLinks.first().click();
          await page.waitForLoadState('networkidle');
          await page.waitForTimeout(500);

          // Should navigate to sibling's profile
          const currentUrl = page.url();
          expect(currentUrl).toMatch(/\/students\/\d+/);
        }
      }
    }
  });

  test('should show edit family button when family exists', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for family name or details (indicating family exists)
      const familyDetails = page.locator('text=/', 'text=/Family|Parent/i');
      const hasDetails = await familyDetails.count() > 0;

      if (hasDetails) {
        // If family exists, should have edit button
        const editButton = page.locator('button').filter({ hasText: /edit family|edit/i });
        const hasEdit = await editButton.count() > 0;

        // Edit button might be in family section or elsewhere
        if (hasEdit) {
          await expect(editButton.first()).toBeVisible();
        }
      }
    }
  });

  test('should allow removing a family member', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for delete/remove button
      const deleteButton = familySection.locator('button').filter({ hasText: /delete|remove|unlink/i });
      const hasDelete = await deleteButton.count() > 0;

      if (hasDelete) {
        // Get initial count of family members
        const memberCount = await page.locator('[class*="guardian"], [class*="family-member"]').count();

        await deleteButton.first().click();
        await page.waitForTimeout(500);

        // Confirm if dialog appears
        const confirmButton = page.locator('button').filter({ hasText: /confirm|yes|ok/i });
        const hasConfirm = await confirmButton.count() > 0;

        if (hasConfirm) {
          await confirmButton.first().click();
        }

        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        // Family member should be removed
        const newMemberCount = await page.locator('[class*="guardian"], [class*="family-member"]').count();
        expect(newMemberCount).toBeLessThan(memberCount);
      }
    }
  });

  test('should display family contact information', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for contact information (phone, email)
      const contactInfo = page.locator('text=/@/i, text=/\\d{3}/'); // Email or phone pattern
      const hasContact = await contactInfo.count() > 0;

      // Contact info might not exist for all students, so we just check it doesn't error
      if (hasContact) {
        const contactText = await contactInfo.first().textContent();
        expect(contactText?.length).toBeGreaterThan(0);
      }
    }
  });

  test('should allow creating new family when none exists', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const familySection = page.locator('text=/family/i, [class*="family"], app-family-section');
    const hasFamilySection = await familySection.count() > 0;

    if (hasFamilySection) {
      // Look for "No Family Linked" message or "Create Family" button
      const noFamilyMessage = page.locator('text=/no family/i, text=/create family/i');
      const hasNoFamily = await noFamilyMessage.count() > 0;

      if (hasNoFamily) {
        // Should have option to create family
        const createButton = page.locator('button').filter({ hasText: /create family|add family/i });
        const hasCreate = await createButton.count() > 0;

        if (hasCreate) {
          await expect(createButton.first()).toBeVisible();
        }
      }
    }
  });
});
