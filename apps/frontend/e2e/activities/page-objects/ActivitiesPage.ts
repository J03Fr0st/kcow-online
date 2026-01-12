import { Page, Locator, expect } from '@playwright/test';

/**
 * Page Object Model for Activities Management Page
 *
 * Encapsulates all interactions with the Activities CRUD interface
 * for cleaner, more maintainable tests.
 */
export class ActivitiesPage {
  readonly page: Page;
  readonly activitiesTable: Locator;
  readonly addButton: Locator;
  readonly tableRows: Locator;

  // Form locators
  readonly activityForm: Locator;
  readonly codeInput: Locator;
  readonly nameInput: Locator;
  readonly descriptionInput: Locator;
  readonly folderInput: Locator;
  readonly gradeLevelInput: Locator;
  readonly iconInput: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly closeButton: Locator;

  // Validation locators
  readonly codeError: Locator;
  readonly nameError: Locator;

  // Icon preview
  readonly iconPreview: Locator;
  readonly clearIconButton: Locator;

  constructor(page: Page) {
    this.page = page;

    // Table and list elements
    this.activitiesTable = page.locator('table[aria-label="Activities Registry"]');
    this.tableRows = page.locator('table tbody tr');
    this.addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();

    // Form elements (in drawer)
    this.activityForm = page.locator('form');
    this.codeInput = this.activityForm.locator('input#code');
    this.nameInput = this.activityForm.locator('input#name');
    this.descriptionInput = this.activityForm.locator('textarea#description');
    this.folderInput = this.activityForm.locator('input#folder');
    this.gradeLevelInput = this.activityForm.locator('input#gradeLevel');
    this.iconInput = this.activityForm.locator('input#icon[type="file"]');
    this.submitButton = this.activityForm.locator('button[type="submit"]');
    this.cancelButton = this.activityForm.locator('button').filter({ hasText: /^cancel$/i }).first();
    this.closeButton = page.locator('button').filter({ hasText: /^\s*✕\s*$/ }).first();

    // Validation error elements
    this.codeError = page.locator('#code-error');
    this.nameError = page.locator('#name-error');

    // Icon preview elements
    this.iconPreview = page.locator('.avatar .w-20 img');
    this.clearIconButton = page.locator('button').filter({ hasText: /clear icon/i });
  }

  /**
   * Navigate to the activities page
   */
  async goto() {
    await this.page.goto('/activities');
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500); // Allow for any animations
  }

  /**
   * Get the number of activities in the list
   */
  async getActivityCount(): Promise<number> {
    return await this.tableRows.count();
  }

  /**
   * Find an activity row by code
   */
  findActivityByCode(code: string): Locator {
    return this.tableRows.filter({ hasText: code }).first();
  }

  /**
   * Find an activity row by name
   */
  findActivityByName(name: string): Locator {
    return this.tableRows.filter({ hasText: name }).first();
  }

  /**
   * Click add button to open create form
   */
  async clickAddActivity() {
    await expect(this.addButton).toBeVisible();
    await this.addButton.click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(300); // Drawer animation
  }

  /**
   * Click an activity row to open edit form
   */
  async clickActivityRow(code: string) {
    const activityRow = this.findActivityByCode(code);
    await expect(activityRow).toBeVisible();
    await activityRow.click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(300); // Drawer animation
  }

  /**
   * Click delete button for a specific activity (first confirmation step)
   */
  async clickDeleteActivity(code: string) {
    const activityRow = this.findActivityByCode(code);
    await expect(activityRow).toBeVisible();

    const deleteButton = activityRow.locator('button').filter({ hasText: /archive/i });
    await expect(deleteButton).toBeVisible();
    await deleteButton.click();
    await this.page.waitForTimeout(300);
  }

  /**
   * Confirm delete operation (second confirmation step)
   */
  async confirmDelete() {
    const confirmButton = this.page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
    await expect(confirmButton).toBeVisible();
    await confirmButton.click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
  }

  /**
   * Cancel delete operation
   */
  async cancelDelete() {
    const cancelButton = this.page.locator('button').filter({ hasText: /^\s*✗\s*$/ });
    await expect(cancelButton).toBeVisible();
    await cancelButton.click();
    await this.page.waitForTimeout(300);
  }

  /**
   * Fill activity form with provided data
   */
  async fillActivityForm(data: {
    code?: string;
    name?: string;
    description?: string;
    folder?: string;
    gradeLevel?: string;
  }) {
    if (data.code !== undefined) {
      await expect(this.codeInput).toBeVisible();
      await this.codeInput.fill(data.code);
    }

    if (data.name !== undefined) {
      await expect(this.nameInput).toBeVisible();
      await this.nameInput.fill(data.name);
    }

    if (data.description !== undefined) {
      await expect(this.descriptionInput).toBeVisible();
      await this.descriptionInput.fill(data.description);
    }

    if (data.folder !== undefined) {
      await expect(this.folderInput).toBeVisible();
      await this.folderInput.fill(data.folder);
    }

    if (data.gradeLevel !== undefined) {
      await expect(this.gradeLevelInput).toBeVisible();
      await this.gradeLevelInput.fill(data.gradeLevel);
    }
  }

  /**
   * Upload an icon file
   * @param filePath - Path to the icon file to upload
   */
  async uploadIcon(filePath: string) {
    await expect(this.iconInput).toBeVisible();
    await this.iconInput.setInputFiles(filePath);
    await this.page.waitForTimeout(500); // Allow for file processing and preview
  }

  /**
   * Clear the icon from the form
   */
  async clearIcon() {
    const clearBtn = this.page.locator('button').filter({ hasText: /clear icon/i });
    if (await clearBtn.count() > 0) {
      await clearBtn.click();
      await this.page.waitForTimeout(300);
    }
  }

  /**
   * Submit the activity form
   */
  async submitForm() {
    await expect(this.submitButton).toBeVisible();
    await this.submitButton.click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500); // Allow for success toast
  }

  /**
   * Cancel form without saving
   */
  async cancelForm() {
    await expect(this.cancelButton).toBeVisible();
    await this.cancelButton.click();
    await this.page.waitForTimeout(300);
  }

  /**
   * Close form using close button
   */
  async closeForm() {
    if (await this.closeButton.count() > 0) {
      await this.closeButton.click();
      await this.page.waitForTimeout(300);
    }
  }

  /**
   * Check if activity is visible in list
   */
  async isActivityVisible(code: string): Promise<boolean> {
    const activityLocator = this.page.locator(`text=${code}`);
    return await activityLocator.isVisible();
  }

  /**
   * Check if activity is NOT visible in list
   */
  async isActivityNotVisible(code: string): Promise<boolean> {
    const activityLocator = this.page.locator(`text=${code}`);
    return await activityLocator.count() === 0;
  }

  /**
   * Get icon thumbnail src for an activity
   */
  async getIconThumbnailSrc(code: string): Promise<string | null> {
    const activityRow = this.findActivityByCode(code);
    if (await activityRow.count() === 0) {
      return null;
    }

    const iconImg = activityRow.locator('.avatar img');
    if (await iconImg.count() === 0) {
      return null;
    }

    return await iconImg.getAttribute('src');
  }

  /**
   * Check if icon thumbnail is visible for an activity
   */
  async isIconThumbnailVisible(code: string): Promise<boolean> {
    const src = await this.getIconThumbnailSrc(code);
    return src !== null;
  }

  /**
   * Check if placeholder is shown (missing icon)
   */
  async isPlaceholderShown(code: string): Promise<boolean> {
    const activityRow = this.findActivityByCode(code);
    if (await activityRow.count() === 0) {
      return false;
    }

    const iconImg = activityRow.locator('.avatar img');
    const placeholder = activityRow.locator('.avatar span');

    return await iconImg.count() === 0 && await placeholder.count() > 0;
  }

  /**
   * Get validation error text for a specific field
   */
  async getValidationError(field: 'code' | 'name'): Promise<string | null> {
    const errorLocator = field === 'code' ? this.codeError : this.nameError;
    if (await errorLocator.count() > 0) {
      return await errorLocator.textContent();
    }
    return null;
  }

  /**
   * Check if form is visible (drawer is open)
   */
  async isFormVisible(): Promise<boolean> {
    return await this.activityForm.count() > 0 && await this.activityForm.isVisible();
  }

  /**
   * Check if loading spinner is visible
   */
  async isLoading(): Promise<boolean> {
    const loadingSpinner = this.page.locator('.loading.loading-spinner');
    return await loadingSpinner.count() > 0 && await loadingSpinner.isVisible();
  }

  /**
   * Wait for loading to complete
   */
  async waitForLoading() {
    while (await this.isLoading()) {
      await this.page.waitForTimeout(100);
    }
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Search for activities by text
   */
  async search(searchTerm: string) {
    const searchInput = this.page.locator('input[placeholder*="search" i], input[type="search"]').first();
    if (await searchInput.count() > 0) {
      await searchInput.fill(searchTerm);
      await this.page.waitForTimeout(500); // Allow for debounce
    }
  }

  /**
   * Clear search
   */
  async clearSearch() {
    const searchInput = this.page.locator('input[placeholder*="search" i], input[type="search"]').first();
    if (await searchInput.count() > 0) {
      await searchInput.clear();
      await this.page.waitForTimeout(500);
    }
  }
}
