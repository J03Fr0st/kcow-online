import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for Trucks Management Page
 *
 * Encapsulates all interactions with the Trucks CRUD interface
 * for cleaner, more maintainable tests.
 */
export class TrucksPage {
  readonly page: Page;
  readonly trucksTable: Locator;
  readonly addButton: Locator;
  readonly tableRows: Locator;

  // Form locators
  readonly truckForm: Locator;
  readonly nameInput: Locator;
  readonly plateInput: Locator;
  readonly yearInput: Locator;
  readonly notesInput: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;

  // Validation locators
  readonly validationErrors: Locator;

  constructor(page: Page) {
    this.page = page;

    // Table and list elements
    this.trucksTable = page.locator('table[aria-label="Trucks Registry"]');
    this.tableRows = page.locator('table tbody tr');
    this.addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();

    // Form elements
    this.truckForm = page.locator('form');
    this.nameInput = this.truckForm.locator('input[name="name"], input[placeholder*="name" i], #name').first();
    this.plateInput = this.truckForm.locator('input[name="plate"], input[name="registrationNumber"], input[placeholder*="plate" i], input[placeholder*="registration" i]').first();
    this.yearInput = this.truckForm.locator('input[name="year"], input[type="number"]').first();
    this.notesInput = this.truckForm.locator('textarea[name="notes"], #notes').first();
    this.submitButton = this.truckForm.locator('button[type="submit"]');
    this.cancelButton = this.truckForm.locator('button').filter({ hasText: /cancel|back/i }).first();

    // Validation
    this.validationErrors = page.locator('text=/required|mandatory|invalid/i');
  }

  /**
   * Navigate to the trucks page
   */
  async goto() {
    await this.page.goto('/trucks');
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(1000);
  }

  /**
   * Get the number of trucks in the list
   */
  async getTruckCount(): Promise<number> {
    return await this.tableRows.count();
  }

  /**
   * Find a truck row by name
   */
  findTruckByName(name: string): Locator {
    return this.tableRows.filter({ hasText: name }).first();
  }

  /**
   * Click add button to open create form
   */
  async clickAddTruck() {
    await this.addButton.click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
  }

  /**
   * Click edit button for a specific truck
   */
  async clickEditTruck(truckName: string) {
    const truckRow = this.findTruckByName(truckName);
    const editButton = truckRow.locator('button').filter({ hasText: /edit|modify/i }).first();

    if (await editButton.count() > 0) {
      await editButton.click();
    } else {
      await truckRow.click();
    }

    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
  }

  /**
   * Click delete button for a specific truck
   */
  async clickDeleteTruck(truckName: string) {
    const truckRow = this.findTruckByName(truckName);
    const deleteButton = truckRow.locator('button').filter({ hasText: /delete|remove|archive/i }).first();

    // Set up dialog handler before clicking
    this.page.on('dialog', async dialog => {
      await dialog.accept();
    });

    await deleteButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Fill truck form with provided data
   */
  async fillTruckForm(data: {
    name: string;
    plate?: string;
    year?: string;
    notes?: string;
  }) {
    await this.nameInput.fill(data.name);

    if (data.plate && await this.plateInput.isVisible()) {
      await this.plateInput.fill(data.plate);
    }

    if (data.year && await this.yearInput.isVisible()) {
      await this.yearInput.fill(data.year);
    }

    if (data.notes && await this.notesInput.isVisible()) {
      await this.notesInput.fill(data.notes);
    }
  }

  /**
   * Submit the truck form
   */
  async submitForm() {
    await this.submitButton.click();
    await this.page.waitForURL(/\/trucks/, { timeout: 10000 });
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(1000);
  }

  /**
   * Cancel form without saving
   */
  async cancelForm() {
    if (await this.cancelButton.count() > 0) {
      await this.cancelButton.click();
      await this.page.waitForURL(/\/trucks$/, { timeout: 5000 });
    }
  }

  /**
   * Check if truck is visible in list
   */
  async isTruckVisible(truckName: string): Promise<boolean> {
    const truckLocator = this.page.locator(`text=${truckName}`);
    return await truckLocator.count() > 0;
  }

  /**
   * Search for trucks by text
   */
  async search(searchTerm: string) {
    const searchInput = this.page.locator('input[placeholder*="search" i], input[type="search"], #search');

    if (await searchInput.count() > 0) {
      await searchInput.first().fill(searchTerm);
      await this.page.waitForTimeout(1000);
    }
  }

  /**
   * Clear search
   */
  async clearSearch() {
    const searchInput = this.page.locator('input[placeholder*="search" i], input[type="search"], #search');

    if (await searchInput.count() > 0) {
      await searchInput.first().clear();
      await this.page.waitForTimeout(1000);
    }
  }

  /**
   * Get validation error text
   */
  async getValidationErrors(): Promise<string[]> {
    const errors: string[] = [];
    const count = await this.validationErrors.count();

    for (let i = 0; i < count; i++) {
      const text = await this.validationErrors.nth(i).textContent();
      if (text) errors.push(text);
    }

    return errors;
  }
}
